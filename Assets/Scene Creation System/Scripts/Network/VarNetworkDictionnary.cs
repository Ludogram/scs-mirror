using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    public class VarNetworkIDictionnary : SyncObject, IDictionary<int, SceneVar>, IReadOnlyDictionary<int, SceneVar>
    {
        public delegate void SyncDictionaryChanged(Operation op, int key, SceneVar item);

        protected readonly IDictionary<int, SceneVar> objects;

        public int Count => objects.Count;
        public bool IsReadOnly { get; private set; }
        public event SyncDictionaryChanged Callback;

        public enum Operation : byte
        {
            OP_ADD,
            OP_CLEAR,
            OP_REMOVE,
            OP_SET
        }

        struct Change
        {
            internal Operation operation;
            internal int key;
            internal SceneVar item;
            internal int sceneObjectID;
        }

        // list of changes.
        readonly Dictionary<int, Change> changes = new();

        // how many changes we need to ignore
        // this is needed because when we initialize the list,
        // we might later receive changes that have already been applied
        // so we need to skip them
        int changesAhead;

        public bool LocalOnly;

        public override void Reset()
        {
            IsReadOnly = false;
            changes.Clear();
            changesAhead = 0;
            objects.Clear();
        }

        public ICollection<int> Keys => objects.Keys;

        public ICollection<SceneVar> Values => objects.Values;

        IEnumerable<int> IReadOnlyDictionary<int, SceneVar>.Keys => objects.Keys;

        IEnumerable<SceneVar> IReadOnlyDictionary<int, SceneVar>.Values => objects.Values;

        // throw away all the changes
        // this should be called after a successful sync
        public override void ClearChanges() => changes.Clear();

        public VarNetworkIDictionnary(IDictionary<int, SceneVar> objects)
        {
            this.objects = objects;
        }

        void AddOperation(Operation op, int key, SceneVar item, BaseSceneObject sender = null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (IsReadOnly)
            {
                throw new System.InvalidOperationException("SyncDictionaries can only be modified by the server");
            }

            Change change = new Change
            {
                operation = op,
                key = key,
                item = item,
                sceneObjectID = sender != null ? sender.SOID : 0
            };

            if (IsRecording() && !LocalOnly)
            {
                changes[key] = change;
                OnDirty?.Invoke();
            }

            Callback?.Invoke(op, key, item);
        }

        private void WriteObject(SceneVar obj, NetworkWriter writer)
        {
            switch (obj.type)
            {
                case SceneVarType.BOOL:
                    writer.Write(obj.BoolValue);
                    return;
                case SceneVarType.INT:
                    writer.Write(obj.IntValue);
                    return;
                case SceneVarType.FLOAT:
                    writer.Write(obj.FloatValue);
                    return;
                case SceneVarType.STRING:
                    writer.Write(obj.StringValue);
                    return;
                default:
                    throw new Exception("Unsupported type used: " + obj.uniqueID + " is of type " + obj.type);
            }
        }

        public override void OnSerializeAll(NetworkWriter writer)
        {
            // if init, write the full list content
            writer.WriteUInt((uint)objects.Count);

            foreach (KeyValuePair<int, SceneVar> syncItem in objects)
            {
                writer.Write(syncItem.Key);
                writer.Write((byte)syncItem.Value.type);
                WriteObject(syncItem.Value, writer);
            }

            // all changes have been applied already
            // thus the client will need to skip all the pending changes
            // or they would be applied again.
            // So we write how many changes are pending
            writer.WriteUInt((uint)changes.Count);
        }

        public override void OnSerializeDelta(NetworkWriter writer)
        {
            // write all the queued up changes
            writer.WriteUInt((uint)changes.Count);

            foreach (var change in changes.Values)
            {
                writer.WriteByte((byte)change.operation);

                switch (change.operation)
                {
                    case Operation.OP_ADD:
                    case Operation.OP_REMOVE:
                    case Operation.OP_SET:
                        writer.Write(change.key);
                        writer.Write((byte)change.item.type);
                        WriteObject(change.item, writer);
                        writer.Write(change.sceneObjectID);
                        break;
                    case Operation.OP_CLEAR:
                        break;
                }
            }
        }

        private static object ReadObject(SceneVarType type, NetworkReader reader)
        {
            return type switch
            {
                SceneVarType.BOOL => reader.ReadBool(),
                SceneVarType.INT => reader.ReadInt(),
                SceneVarType.FLOAT => reader.ReadFloat(),
                SceneVarType.STRING => reader.ReadString(),
                _ => throw new Exception("Unsupported type used: " + type)
            };
        }

        public override void OnDeserializeAll(NetworkReader reader)
        {
            Debug.Log("on deserialize all");
            if (LocalOnly) return;

            Debug.Log("on deserialize all valid");
            // This list can now only be modified by synchronization
            IsReadOnly = true;

            // if init,  write the full list content
            int count = (int)reader.ReadUInt();

            objects.Clear();
            changes.Clear();

            for (int i = 0; i < count; i++)
            {
                int key = reader.Read<int>();
                SceneVarType type = (SceneVarType)reader.Read<byte>();
                object obj = ReadObject(type, reader);
                if (!objects.ContainsKey(key))
                {
                    objects[key] = new SceneVar(key, type);
                }
                SceneState.SetVarValue(key, obj, null);
                //switch (type)
                //{
                //    case SceneVarType.BOOL:
                //        objects[key].BoolValue = (bool)obj; break;
                //    case SceneVarType.INT:
                //        objects[key].IntValue = (int)obj; break;
                //    case SceneVarType.FLOAT:
                //        objects[key].FloatValue = (float)obj; break;
                //    case SceneVarType.STRING:
                //        objects[key].StringValue = (string)obj; break;
                //}
            }

            // We will need to skip all these changes
            // the next time the list is synchronized
            // because they have already been applied
            changesAhead = (int)reader.ReadUInt();
        }

        public override void OnDeserializeDelta(NetworkReader reader)
        {
            if (LocalOnly) return;

            // This list can now only be modified by synchronization
            IsReadOnly = true;

            int changesCount = (int)reader.ReadUInt();

            for (int i = 0; i < changesCount; i++)
            {
                Operation operation = (Operation)reader.ReadByte();

                // apply the operation only if it is a new change
                // that we have not applied yet
                // bool apply = changesAhead == 0;

                // For this delta, we never want to ignore changes
                // This fixes refresh for synchronized NuiElements when loading a save with as host from CampaignSettings
                bool apply = true;

                int key = default;
                SceneVar item = default;
                SceneVarType type;
                int sceneObjectID;

                switch (operation)
                {
                    case Operation.OP_ADD:
                    case Operation.OP_SET:

                        key = reader.Read<int>();
                        type = (SceneVarType)reader.Read<byte>();
                        object obj = ReadObject(type, reader);
                        sceneObjectID = reader.ReadInt();
                        if (!SceneState.TryGetSceneObject(sceneObjectID, out BaseSceneObject sender))
                        {
                            Debug.LogError("can't find baseSceneObject");
                        }

                        if (apply)
                        {
                            if (!objects.ContainsKey(key))
                            {
                                Debug.LogWarning("SceneVar " + key + " doesn't exist yet");
                                objects[key] = new SceneVar(key, type);
                            }
                            item = objects[key];
                            SceneState.SetVarValue(key, obj, sender);
                        }
                        break;

                    case Operation.OP_CLEAR:
                        if (apply)
                        {
                            objects.Clear();
                        }
                        break;

                    case Operation.OP_REMOVE:
                        key = reader.Read<int>();
                        type = reader.Read<SceneVarType>();
                        ReadObject(type, reader);
                        if (apply)
                        {
                            objects.Remove(key);
                        }
                        break;
                }

                if (apply)
                {
                    Callback?.Invoke(operation, key, item);
                }
                // we just skipped this change
                else
                {
                    changesAhead--;
                }
            }
        }

        public void Clear()
        {
            objects.Clear();
            AddOperation(Operation.OP_CLEAR, default, default);
        }

        public bool ContainsKey(int key) => objects.ContainsKey(key);

        public bool Remove(int key)
        {
            if (objects.TryGetValue(key, out SceneVar item) && objects.Remove(key))
            {
                AddOperation(Operation.OP_REMOVE, key, item);
                return true;
            }
            return false;
        }

        public SceneVar this[int i]
        {
            get => objects[i];
            set
            {
                if (ContainsKey(i))
                {
                    objects[i] = value;
                    AddOperation(Operation.OP_SET, i, value);
                }
                else
                {
                    objects[i] = value;
                    AddOperation(Operation.OP_ADD, i, value);
                }
            }
        }

        public bool TryGetValue(int key, out SceneVar value)
        {
            var output = objects.TryGetValue(key, out value);
            return output;
        }

        public void Set(int key, SceneVar value, BaseSceneObject sender)
        {
            AddOperation(objects.ContainsKey(key) ? Operation.OP_SET : Operation.OP_ADD, key, value, sender);
            objects[key] = value;
        }

        public void Add(int key, SceneVar value)
        {
            objects.Add(key, value);
            AddOperation(Operation.OP_ADD, key, value);
        }

        public void Add(KeyValuePair<int, SceneVar> item) => Add(item.Key, item.Value);

        public bool Contains(KeyValuePair<int, SceneVar> item)
        {
            return TryGetValue(item.Key, out SceneVar val) && EqualityComparer<SceneVar>.Default.Equals(val, item.Value);
        }

        public void CopyTo(KeyValuePair<int, SceneVar>[] array, int arrayIndex)
        {
            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new System.ArgumentOutOfRangeException(nameof(arrayIndex), "Array Index Out of Range");
            }
            if (array.Length - arrayIndex < Count)
            {
                throw new System.ArgumentException("The number of items in the SyncDictionary is greater than the available space from arrayIndex to the end of the destination array");
            }

            int i = arrayIndex;
            foreach (KeyValuePair<int, SceneVar> item in objects)
            {
                array[i] = item;
                i++;
            }
        }

        public bool Remove(KeyValuePair<int, SceneVar> item)
        {
            bool result = objects.Remove(item.Key);
            if (result)
            {
                AddOperation(Operation.OP_REMOVE, item.Key, item.Value);
            }
            return result;
        }

        public IEnumerator<KeyValuePair<int, SceneVar>> GetEnumerator() => objects.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => objects.GetEnumerator();
    }

    public class VarNetworkDictionnary : VarNetworkIDictionnary
    {
        public VarNetworkDictionnary() : base(new Dictionary<int, SceneVar>()) { }
        public VarNetworkDictionnary(IEqualityComparer<int> eq) : base(new Dictionary<int, SceneVar>(eq)) { }
        public VarNetworkDictionnary(IDictionary<int, SceneVar> d) : base(new Dictionary<int, SceneVar>(d)) { }
        public new Dictionary<int, SceneVar>.ValueCollection Values => ((Dictionary<int, SceneVar>)objects).Values;
        public new Dictionary<int, SceneVar>.KeyCollection Keys => ((Dictionary<int, SceneVar>)objects).Keys;
        public new Dictionary<int, SceneVar>.Enumerator GetEnumerator() => ((Dictionary<int, SceneVar>)objects).GetEnumerator();
    }
}
