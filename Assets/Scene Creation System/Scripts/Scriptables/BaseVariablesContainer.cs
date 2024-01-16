using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("SceneCreationSystem.Editor")]
namespace Dhs5.SceneCreation
{
    public abstract class BaseVariablesContainer : ScriptableObject
    {
        [SerializeField] protected List<SceneVar> sceneVars;

        //[SerializeField] protected List<ComplexSceneVar> complexSceneVars;


        public abstract List<SceneVar> SceneVars { get; }

        public SceneVar this[int uniqueID]
        {
            get
            {
                SceneVar sVar = SceneVars.Find(v => v.uniqueID == uniqueID);
                if (sVar == null) Debug.LogError("Can't find SceneVar from UID " + uniqueID + " in " + name);
                return sVar;
            }
        }

        protected abstract Vector2Int UIDRange { get; }


        #region Scene Vars

        public virtual void AddSceneVarOfType(SceneVarType type)
        {
            if (sceneVars == null) sceneVars = new();
            sceneVars.Add(new(GenerateUniqueID(), type));
        }

        public void TryRemoveSceneVarAtIndex(int index)
        {
            if (sceneVars.IsIndexValid(index))
            {
                if (!sceneVars[index].IsLink)
                {
                    sceneVars.RemoveAt(index);
                }
                else
                {
                    Debug.LogError("Can't remove link");
                }
            }
            else
            {
                Debug.LogError("Invalid index");
            }
        }
        protected virtual void RemoveSceneVar(SceneVar var)
        {
            if (sceneVars.Contains(var))
            {
                if (sceneVars.Remove(var))
                    Debug.Log("Removed " + var);
                else
                    Debug.Log("Couldn't remove " + var);
            }
            else
            {
                Debug.LogError("Can't remove SceneVar " + var + " as it is not contained in " + name + " sceneVars");
            }
        }

        public virtual bool CanRemoveAtIndex(int index)
        {
            return sceneVars.IsIndexValid(index) && !sceneVars[index].IsLink;
        }
        public virtual bool IsDisabledAtIndex(int index)
        {
            return sceneVars.IsIndexValid(index) && sceneVars[index].IsLink;
        }

        #endregion

        #region Complex Scene Vars
        /*
        #region Getters
        public ComplexSceneVar GetComplexSceneVarWithUID(int UID)
        {
            ComplexSceneVar v = complexSceneVars.Find(x => x.uniqueID == UID);
            if (v == null)
            {
                Debug.LogError("No Complex Scene Var with UID " + UID + " found in " + name);
            }
            return v;
        }
        public bool TryGetComplexSceneVarWithUID(int UID, out ComplexSceneVar complexSceneVar)
        {
            complexSceneVar = GetComplexSceneVarWithUID(UID);

            return complexSceneVar != null;
        }
        #endregion

        #region List Management
        public void AddComplexSceneVarOfType(ComplexSceneVarType type)
        {
            ComplexSceneVar complexVar = new(GenerateUniqueID(), type);

            complexSceneVars.Add(complexVar);
            sceneVars.Add(complexVar.Link);
        }

        public void TryRemoveComplexSceneVarAtIndex(int index)
        {
            if (complexSceneVars.IsIndexValid(index))
            {
                ComplexSceneVar v = complexSceneVars[index];

                if (sceneVars.Remove(v.Link))
                {
                    complexSceneVars.Remove(v);
                }
                else
                {
                    Debug.LogError("Link " + v.Link + " is not in the SceneVar list ? " + !sceneVars.Contains(v.Link));
                }
            }
            else
            {
                Debug.LogError("Invalid index");
            }
        }
        #endregion

        #region Set up

        private void SetupComplexSceneVars()
        {
            complexSceneVars.SetUp(this);
            complexSceneVars.SetForbiddenUID(0);
        }

        #endregion

        #region Links Management
        private void UpdateSceneVarLinks()
        {
            // Browse on Complex Scene Vars
            foreach (var cVar in complexSceneVars)
            {
                // If link is null, find it or create one
                if (cVar.Link == null)
                {
                    SceneVar lostLink = sceneVars.Find(v => v.uniqueID == cVar.uniqueID);

                    if (lostLink == null)
                    {
                        Debug.LogError("No Scene Var Link found for Complex Scene Var" + cVar.uniqueID + " , creating one");
                        cVar.Link = SceneVar.CreateLink(cVar);
                        sceneVars.Add(cVar.Link);
                    }
                    else
                    {
                        cVar.Link = lostLink;
                    }
                }

                // If link exist but is not in the scene vars
                else if (!sceneVars.Contains(cVar.Link))
                {
                    SceneVar lostLink = sceneVars.Find(v => v.uniqueID == cVar.uniqueID);

                    if (lostLink == null)
                    {
                        Debug.LogError("Complex Scene Var link exist but can't find link in the SceneVar list, adding it");
                        sceneVars.Add(cVar.Link);
                    }
                    else if (cVar.Link != lostLink)
                    {
                        cVar.Link = lostLink;
                    }
                }
            }

            // Browse on Scene Vars
            foreach (var v in sceneVars.Copy())
            {
                if (v.IsLink && !TryGetComplexSceneVarWithUID(v.uniqueID, out _))
                {
                    sceneVars.Remove(v);
                }
            }
        }

        #endregion
        */
        #endregion

        #region IDs
        public int GetUniqueIDByIndex(int index)
        {
            if (index < 0 || index >= SceneVars.Count) return 0;
            return SceneVars[index].uniqueID;
        }
        public int GetIndexByUniqueID(int uniqueID)
        {
            if (uniqueID == 0) return -1;
            return SceneVars.FindIndex(v => v.uniqueID == uniqueID);
        }

        private List<int> UniqueIDs
        {
            get
            {
                if (!SceneVars.IsValid()) return new();

                List<int> list = new();
                foreach (var var in SceneVars)
                {
                    list.Add(var.uniqueID);
                }
                return list;
            }
        }
        public List<string> IDs
        {
            get
            {
                List<string> list = new();
                foreach (var var in SceneVars)
                {
                    if (var.uniqueID != 0)
                        list.Add(var.ID);
                    else
                        list.Add("No unique ID");
                }
                return list;
            }
        }
        public List<string> SceneVarStrings
        {
            get
            {
                List<string> list = new();
                foreach (var var in SceneVars)
                {
                    if (var.uniqueID != 0)
                        list.Add(var.PopupString());
                    else
                        list.Add("No unique ID");
                }
                return list;
            }
        }

        protected int GenerateUniqueID()
        {
            int uniqueID;
            List<int> uniqueIDs = UniqueIDs;
            do
            {
                uniqueID = Random.Range(UIDRange.x, UIDRange.y);
            } while (uniqueIDs.Contains(uniqueID));

            return uniqueID;
        }
        #endregion


        //private void OnValidate()
        //{
        //    UpdateSceneVarLinks();
        //
        //    SetupComplexSceneVars();
        //
        //    OnOnValidate();
        //}
        //
        //protected virtual void OnOnValidate() { }


        #region Helper Functions

        [ContextMenu("Clean")]
        protected void CleanUp()
        {
            if (CallBaseCleanUp())
            {
                foreach (var var in sceneVars.Copy())
                {
                    if (var.uniqueID == 0)
                        RemoveSceneVar(var);
                }
            }

            ChildCleanUp();
        }
        protected virtual bool CallBaseCleanUp() => true;
        protected virtual void ChildCleanUp() { }

        #endregion

        #region Static SceneVar List Functions

        public static List<string> VarStrings(List<SceneVar> vars)
        {
            List<string> list = new();
            foreach (var var in vars)
            {
                if (var.uniqueID != 0)
                    list.Add(var.PopupString());
                else
                    list.Add("No unique ID");
            }
            return list;
        }
        public static int GetUniqueIDByIndex(List<SceneVar> vars, int index)
        {
            if (index < 0 || index >= vars.Count) return 0;
            return vars[index].uniqueID;
        }
        public static int GetIndexByUniqueID(List<SceneVar> vars, int uniqueID)
        {
            if (uniqueID == 0) return -1;
            return vars.FindIndex(v => v.uniqueID == uniqueID);
        }

        #endregion
    }
}
