using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace Dhs5.SceneCreation
{
    public class SceneContext
    {
        #region Constructor
        public SceneContext()
        {
            sb = new();
            rank = 0;
        }
        public SceneContext(string start)
        {
            sb = new();
            sb.AppendLine(start);
            rank = 1;
        }
        public SceneContext(SceneContext context)
        {
            string content = context.Get();
            sb = new(content);
            rank = context.rank;
        }
        #endregion

        private StringBuilder sb;
        private int rank;

        public SceneContext Add(string context, bool uprank = false)
        {
            Alinea();
            sb.Append(context);
            sb.Append('\n');
            return this;
        }
        public SceneContext Add(params string[] contexts)
        {
            if (contexts == null || contexts.Length == 0) return this;
            Alinea();
            foreach (string context in contexts)
            {
                sb.Append(context);
            }
            sb.Append('\n');
            return this;
        }
        public void UpRank() { rank++; }
        public void UpRank(params string[] contexts) 
        { 
            rank++;
            Add(contexts);
        }

        public string Get()
        {
            return sb.ToString();
        }

        #region Helpers
        private void Alinea()
        {
            if (rank == 0) return;
            sb.Append(" ");
            for (int i = 0; i < rank - 1; i++)
            {
                sb.Append("-- ");
            }
            sb.Append("--> ");
        }
        #endregion
    }

    public class SceneEventParam
    {
        public SceneEventParam(SceneVar _var, object _formerValue, BaseSceneObject _sender, SceneContext _context)
        { 
            Var = new(_var);
            FormerValue = _formerValue;
            Sender = _sender;
            Context = _context;
        }
        public SceneEventParam(SceneEventParam param)
        { 
            Var = param.Var;
            FormerValue = param.FormerValue;
            Sender = param.Sender;
            Context = new(param.Context);
        }

        public SceneVar Var { get; private set; }
        public object FormerValue { get; private set; }
        public BaseSceneObject Sender { get; private set; }
        public SceneContext Context { get; private set; }

        // Getters
        public int UID => Var.uniqueID;
        public string ID => Var.ID;
        public SceneVarType Type => Var.type;
        public object Value => Var.Value;


        public override string ToString()
        {
            if (Type == SceneVarType.EVENT) return "[" + UID + "] " + ID + " (EVENT) Triggered";
            return "[" + UID + "] " + ID + " (" + Type + ") = " + Value + " (before = " + FormerValue + ")";
        }
    }

    public static class SceneEventManager
    {
        private static Dictionary<int, Action<SceneEventParam>> eventDico = new();


        public static void StartListening(int keyEvent, Action<SceneEventParam> listener)
        {
            if (eventDico.ContainsKey(keyEvent))
            {
                eventDico[keyEvent] += listener;
            }
            else
            {
                eventDico.Add(keyEvent, listener);
            }
        }

        public static void StopListening(int keyEvent, Action<SceneEventParam> listener)
        {
            if (eventDico.ContainsKey(keyEvent))
            {
                eventDico[keyEvent] -= listener;
            }
        }

        public static void TriggerEvent(int keyEvent, SceneEventParam param)
        {
            if (eventDico.TryGetValue(keyEvent, out Action<SceneEventParam> thisEvent))
            {
                thisEvent?.Invoke(param);
            }
        }
    }
}
