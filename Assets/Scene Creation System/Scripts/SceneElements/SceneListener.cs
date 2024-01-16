using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Text;

namespace Dhs5.SceneCreation
{
    [Serializable]
    public abstract class BaseSceneListener : SceneState.ISceneVarSetupable, SceneState.ISceneObjectBelongable, SceneState.ISceneSubscribable, SceneState.ISceneVarDependant, SceneState.ISceneLogableWithChild
    {
        #region Listening Condition

        [Serializable]
        public class Condition : SceneState.ISceneVarSetupable, SceneState.ISceneVarDependant, SceneState.ISceneLogable
        {
            [SerializeField] protected bool hasCondition = false;

            [SerializeField] protected bool layerCheck = false;
            [SerializeField] protected SceneObjectLayerMask layerMask;
            [SerializeField] protected bool tagCheck = false;
            [SerializeField] protected SceneObjectTag tagMask;
            [SerializeField] protected List<SceneCondition> sceneConditions;

            public bool HasCondition => hasCondition;
            public bool VerifyCondition(SceneEventParam param)
            {
                if (!hasCondition) return true;

                if (!layerCheck || layerMask.Include(param.Sender.Layer))
                {
                    if (!tagCheck || tagMask.ContainsAny(param.Sender.Tag))
                    {
                        return sceneConditions.VerifyConditions();
                    }
                }
                return false;
            }

            #region Set Up
            public void SetUp(SceneVariablesSO _sceneVariablesSO)
            {
                sceneConditions.SetUp(_sceneVariablesSO);
            }
            #endregion

            #region Dependency

            public virtual List<int> Dependencies => sceneConditions.Dependencies();

            public bool DependOn(int UID) => Dependencies.Contains(UID);

            #endregion

            #region SceneLog

            public List<string> LogLines(bool detailed = false, bool showEmpty = false, string alinea = null)
            {
                List<string> lines = new();
                StringBuilder sb = new();

                // First Alinea
                if (alinea != null) sb.Append(alinea);

                if (layerCheck)
                {
                    sb.Append("Sender Layer contained in : ");
                    sb.Append(layerMask.NamesAsString);
                    Line();
                }

                if (tagCheck)
                {
                    sb.Append("Sender Tag containes any in : ");
                    sb.Append(tagMask.NamesAsString);
                    Line();
                }

                if (sceneConditions.IsValid())
                {
                    for (int i = 0; i < sceneConditions.Count; i++)
                    {
                        sb.Append(sceneConditions[i].ToString());
                        if (i < sceneConditions.Count - 1)
                        {
                            Line();
                            sb.Append(sceneConditions[i].logicOperator);
                        }
                        Line();
                    }
                }

                return lines;

                #region Local
                void Line()
                {
                    sb.Append('\n');
                    lines.Add(sb.ToString());
                    sb.Clear();
                    if (alinea != null) sb.Append(alinea);
                }
                #endregion
            }

            public bool IsEmpty()
            {
                return !hasCondition || (!layerCheck && !tagCheck && !sceneConditions.IsValid());
            }

            #endregion
        }

        #endregion

        [SerializeField] protected SceneVariablesSO sceneVariablesSO;
        public SceneVariablesSO SceneVariablesSO => sceneVariablesSO;

        protected BaseSceneObject sceneObject;

        // SceneVar selection
        [SerializeField] protected int varUniqueID;
        public int UID => varUniqueID;
        public SceneVar CurrentSceneVar
        {
            get { return SceneState.GetSceneVar(varUniqueID); }
        }
        protected SceneVar EditorSceneVar
        {
            get => sceneVariablesSO[varUniqueID];
        }

        [SerializeField] protected Condition condition;

        [SerializeField] protected bool debug = false;
        [SerializeField] protected float propertyHeight;

        #region Event Subscription
        public void Subscribe()
        {
            SceneEventManager.StartListening(varUniqueID, OnListenerEvent);
        }
        public void Unsubscribe()
        {
            SceneEventManager.StopListening(varUniqueID, OnListenerEvent);
        }
        private void OnListenerEvent(SceneEventParam _param)
        {
            if (VerifyConditions(_param))
            {
                SceneEventParam param = new(_param);
                param.Context.UpRank(sceneObject.name, " listener received ", param.ToString());

                if (debug)
                    DebugSceneListener(param.Context);

                Trigger(param);
            }
        }
        #endregion

        #region Interfaces
        public void SetUp(SceneVariablesSO _sceneVariablesSO)
        {
            sceneVariablesSO = _sceneVariablesSO;

            condition.SetUp(sceneVariablesSO);
        }
        public void BelongTo(BaseSceneObject _sceneObject)
        {
            sceneObject = _sceneObject;
        }
        #endregion

        #region Utility
        private bool VerifyConditions(SceneEventParam param)
        {
            return condition.VerifyCondition(param);
        }
        protected abstract void Trigger(SceneEventParam _param);
        #endregion

        #region Debug
        protected virtual void DebugSceneListener(SceneContext context)
        {
            Debug.LogError(context.Get());
        }
        #endregion

        #region SceneLog
        public override string ToString()
        {
            StringBuilder sb = new();

            sb.Append("Listen to : [");
            sb.Append(varUniqueID);
            sb.Append("] ");
            sb.Append(EditorSceneVar?.ID);
            sb.Append(" (");
            sb.Append(EditorSceneVar.type);
            sb.Append(")");
            sb.Append("\n");

            return sb.ToString();
        }
        public List<string> LogLines(bool detailed = false, bool showEmpty = false, string alinea = null)
        {
            List<string> lines = new();
            StringBuilder sb = new();

            // First Alinea
            if (alinea != null) sb.Append(alinea);

            sb.Append(SceneLogger.ListenerColor);
            sb.Append("|");
            sb.Append(SceneLogger.ColorEnd);
            sb.Append(" Listen to : ");
            sb.Append(EditorSceneVar?.LogString());
            Line();

            if (detailed)
            {
                if (!condition.IsEmpty())
                {
                    sb.Append("   * IF : ");
                    Line();

                    lines.AddRange(condition.LogLines(detailed, showEmpty, "          "));
                }

                ChildLog(lines, sb, detailed, showEmpty, alinea);
            }

            return lines;

            #region Local
            void Line()
            {
                sb.Append('\n');
                lines.Add(sb.ToString());
                sb.Clear();
                if (alinea != null) sb.Append(alinea);
            }
            #endregion
        }

        /// <returns>Whether the <see cref="BaseSceneListener"/> has actions</returns>
        public bool IsEmpty()
        {
            return IsChildEmpty();
        }
        public abstract void ChildLog(List<string> lines, StringBuilder sb, bool detailed, bool showEmpty, string alinea = null);
        public abstract bool IsChildEmpty();

        #endregion

        #region Dependencies
        public virtual List<int> Dependencies
        {
            get
            {
                List<int> dependencies = new List<int>() { UID };

                if (condition.HasCondition)
                    dependencies.AddRange(condition.Dependencies);
                return dependencies;
            }
        }
        public bool DependOn(int UID) { return Dependencies.Contains(UID); }
        #endregion
    }
    
    [Serializable]
    public class SceneListener : BaseSceneListener
    {
        #region SceneEventTrigger
        [Serializable]
        public struct SceneEventTrigger
        {
            public string eventID;
            public bool random;
            public bool remove;

            public override string ToString()
            {
                return "Trigger : " + eventID + (random ? " randomly " : "") + (remove ? " and remove" : "");
            }
        }
        #endregion
        
        public UnityEvent<SceneEventParam> events;

        public List<SceneEventTrigger> triggers;

        #region Utility
        protected override void Trigger(SceneEventParam _param)
        {
            events.Invoke(_param);

            sceneObject.Trigger(triggers, _param);
        }
        #endregion

        #region SceneLog
        public override void ChildLog(List<string> lines, StringBuilder sb, bool detailed, bool showEmpty, string alinea = null)
        {
            // Clear String Builder
            sb.Clear();

            // First Alinea
            if (alinea != null) sb.Append(alinea);

            if (events.GetPersistentEventCount() > 0)
            {
                sb.Append("   * UNITY EVENT : ");
                Line();

                for (int i = 0; i < events.GetPersistentEventCount(); i++)
                {
                    sb.Append("      --> ");
                    sb.Append(events.GetPersistentTarget(i).ToString());
                    sb.Append(".");
                    sb.Append(events.GetPersistentMethodName(i));
                    Line();
                }
            }

            if (triggers.Count > 0)
            {
                sb.Append("   * TRIGGERS : ");
                Line();

                foreach (var trigger in triggers)
                {
                    sb.Append("      --> ");
                    sb.Append(trigger.ToString());
                    Line();
                }
            }

            #region Local
            void Line()
            {
                sb.Append('\n');
                lines.Add(sb.ToString());
                sb.Clear();
                if (alinea != null) sb.Append(alinea);
            }
            #endregion
        }
        public override bool IsChildEmpty()
        {
            return events.GetPersistentEventCount() <= 0 && triggers.Count <= 0;
        }
        #endregion
    }
    
    [Serializable]
    public class SceneSpecificListener : BaseSceneListener
    {
        private Action<SceneEventParam> events;

        #region Utility
        protected override void Trigger(SceneEventParam _param)
        {
            events.Invoke(_param);
        }
        /// <summary>
        /// Sets the event to invoke by the <see cref="SceneSpecificListener"/>
        /// </summary>
        /// <remarks>Call this function in <see cref="BaseSceneObject.UpdateSceneVariables"/> to make the event appear in the Scene Log</remarks>
        /// <param name="_events">Event to invoke</param>
        public void SetEvents(Action<SceneEventParam> _events)
        {
            events = _events;
        }
        #endregion

        #region SceneLog
        public override void ChildLog(List<string> lines, StringBuilder sb, bool detailed, bool showEmpty, string alinea = null)
        {
            // Clear String Builder
            sb.Clear();

            // First Alinea
            if (alinea != null) sb.Append(alinea);

            if (events != null)
            {
                sb.Append("   * EVENT :");
                Line();
                sb.Append("      --> ");
                sb.Append(events.Target.ToString());
                sb.Append(".");
                sb.Append(events.Method.Name);
                Line();
            }
            else if (!Application.isPlaying)
            {
                sb.Append("   * EVENT : attached at runtime");
                lines.Add(sb.ToString());
                return;
            }

            #region Local
            void Line()
            {
                sb.Append('\n');
                lines.Add(sb.ToString());
                sb.Clear();
            }
            #endregion
        }

        public override bool IsChildEmpty()
        {
            return false;
        }
        #endregion
    }
    
    /*
    [Serializable]
    public class SceneListener : SceneState.ISceneVarSetupable, SceneState.ISceneObjectBelongable, SceneState.ISceneRegisterable, SceneState.ISceneVarDependant
    {
        #region SceneEventTrigger
        [Serializable]
        public struct SceneEventTrigger
        {
            public string eventID;
            public bool random;
            public bool remove;

            public override string ToString()
            {
                return "Trigger : " + eventID + (random ? " randomly " : "") + (remove ? " and remove" : "");
            }
        }
        #endregion

        public SceneVariablesSO sceneVariablesSO;
        private SceneObject sceneObject;

        // SceneVar selection
        [SerializeField] private int varUniqueID;
        public int UID => varUniqueID;
        public SceneVar CurrentSceneVar
        {
            get { return SceneState.GetSceneVar(varUniqueID); }
        }
        private SceneVar EditorSceneVar
        {
            get => sceneVariablesSO[varUniqueID];
        }
        
        // Condition
        public bool hasCondition;

        public List<SceneCondition> conditions;
        
        public UnityEvent<SceneEventParam> events;

        public List<SceneEventTrigger> triggers;

        public bool debug = false;
        public float propertyHeight;

        #region Event Subscription
        public void Register()
        {
            SceneEventManager.StartListening(varUniqueID, OnListenerEvent);
        }
        public void Unregister()
        {
            SceneEventManager.StopListening(varUniqueID, OnListenerEvent);
        }
        private void OnListenerEvent(SceneEventParam _param)
        {
            if (VerifyConditions())
            {
                SceneEventParam param = new(_param);
                param.Context.UpRank(sceneObject.name, " listener received ", param.ToString());

                Trigger(param);
                if (debug)
                    DebugSceneListener(param.Context);
            }
        }
        #endregion

        #region Interfaces
        public void SetUp(SceneVariablesSO _sceneVariablesSO)
        {
            sceneVariablesSO = _sceneVariablesSO;

            conditions.SetUp(sceneVariablesSO);
        }
        public void BelongTo(SceneObject _sceneObject)
        {
            sceneObject = _sceneObject;
        }
        #endregion

        #region Utility
        private bool VerifyConditions()
        {
            return !hasCondition || conditions.VerifyConditions();
        }
        private void Trigger(SceneEventParam _param)
        {
            events.Invoke(_param);
            sceneObject.Trigger(triggers, _param);
        }
        #endregion

        #region Debug
        private void DebugSceneListener(SceneContext context)
        {
            Debug.LogError(context.Get());
        }
        #endregion

        #region SceneLog
        public string Log()
        {
            StringBuilder sb = new();

            sb.Append("* Listen to : [");
            sb.Append(varUniqueID);
            sb.Append("] ");
            sb.Append(EditorSceneVar?.ID);
            sb.Append(" (");
            sb.Append(EditorSceneVar.type);
            sb.Append(")");
            sb.Append("\n");

            return sb.ToString();
        }
        public List<string> LogLines(bool detailed = false)
        {
            List<string> lines = new();
            StringBuilder sb = new();

            sb.Append(SceneLogger.ListenerColor);
            sb.Append("|");
            sb.Append(SceneLogger.ColorEnd);
            sb.Append(" Listen to : ");
            sb.Append(EditorSceneVar?.LogString());
            Line();

            if (detailed)
            {
                if (hasCondition && conditions != null && conditions.Count > 0)
                {
                    sb.Append("   * IF : ");
                    Line();

                    for (int i = 0; i < conditions.Count; i++)
                    {
                        sb.Append("          ");
                        sb.Append(conditions[i].ToString());
                        if (i < conditions.Count - 1)
                        {
                            Line();
                            sb.Append("          ");
                            sb.Append(conditions[i].logicOperator);
                        }
                        Line();
                    }
                }                

                sb.Append("   * UNITY EVENT : ");
                Line();

                for (int i = 0; i < events.GetPersistentEventCount(); i++)
                {
                    sb.Append("      --> ");
                    sb.Append(events.GetPersistentTarget(i).ToString());
                    sb.Append(".");
                    sb.Append(events.GetPersistentMethodName(i));
                    Line();
                }

                sb.Append("   * TRIGGERS : ");
                Line();

                foreach (var trigger in triggers)
                {
                    sb.Append("      --> ");
                    sb.Append(trigger.ToString());
                    Line();
                }
            }

            return lines;

            #region Local
            void Line()
            {
                sb.Append('\n');
                lines.Add(sb.ToString());
                sb.Clear();
            }
            #endregion
        }
        #endregion

        #region Dependencies
        public List<int> Dependencies { get => new List<int>() { UID }; }
        public bool DependOn(int UID) { return Dependencies.Contains(UID); }
        public void SetForbiddenUID(int UID) { }
        #endregion
    }
    */
}
