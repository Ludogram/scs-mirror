using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    [Serializable]
    public class SceneTimedCondition : SceneState.ISceneVarSetupable, SceneState.ISceneVarDependant
    {
        [Serializable]
        public enum TimedConditionType
        {
            WAIT_FOR_TIME = 0,
            WAIT_UNTIL_SCENE_CONDITION = 1,
            WAIT_WHILE_SCENE_CONDITION = 2,
            WAIT_FOR_EVENT = 3,
        }

        public TimedConditionType conditionType;
        
        public SceneVarTween timeToWait;
        public List<SceneCondition> sceneConditions;
        public SceneVarTween eventVar;
        
        public void SetUp(SceneVariablesSO sceneVariablesSO)
        {
            sceneConditions.SetUp(sceneVariablesSO);
            timeToWait.SetUp(sceneVariablesSO, SceneVarType.FLOAT, true);
            eventVar.SetUp(sceneVariablesSO, SceneVarType.EVENT);
        }
        
        public IEnumerator Condition()
        {
            startTime = Time.time;
            stop = false;
            eventTriggered = false;
            switch (conditionType)
            {
                case TimedConditionType.WAIT_FOR_TIME:
                    //yield return new WaitForSeconds(timeToWait);
                    yield return new WaitUntil(TimeIsUp);
                    break;
                case TimedConditionType.WAIT_UNTIL_SCENE_CONDITION:
                    //yield return new WaitUntil(sceneConditions.VerifyConditions);
                    yield return new WaitUntil(SceneConditionVerified);
                    break;
                case TimedConditionType.WAIT_WHILE_SCENE_CONDITION:
                    //yield return new WaitWhile(sceneConditions.VerifyConditions);
                    yield return new WaitWhile(SceneConditionUnverified);
                    break;
                case TimedConditionType.WAIT_FOR_EVENT:
                    StartListening();
                    yield return new WaitUntil(EventTriggered);
                    break;
            }

            stop = false;
            yield break;
        }


        private bool stop = false;
        public void BreakCoroutine()
        {
            stop = true;
        }

        private float startTime;
        private bool TimeIsUp()
        {
            return stop || (Time.time - startTime >= timeToWait.FloatValue);
        }

        private bool SceneConditionVerified()
        {
            return stop || sceneConditions.VerifyConditions();
        }

        private bool SceneConditionUnverified()
        {
            return !stop && sceneConditions.VerifyConditions();
        }

        private bool eventTriggered;
        private bool EventTriggered()
        {
            return stop || eventTriggered;
        }

        #region Event Listening
        private void StartListening()
        {
            SceneEventManager.StartListening(eventVar.UID, OnListenerEvent);
        }
        private void StopListening()
        {
            SceneEventManager.StopListening(eventVar.UID, OnListenerEvent);
        }
        private void OnListenerEvent(SceneEventParam param)
        {
            eventTriggered = true;
            StopListening();
        }
        #endregion

        #region Log
        public override string ToString()
        {
            switch (conditionType)
            {
                case TimedConditionType.WAIT_FOR_TIME:
                    return "WAIT for " + timeToWait.LogString() + " seconds";
                case TimedConditionType.WAIT_UNTIL_SCENE_CONDITION:
                    return "WAIT until " + sceneConditions;
                case TimedConditionType.WAIT_WHILE_SCENE_CONDITION:
                    return "WAIT while " + sceneConditions;
                case TimedConditionType.WAIT_FOR_EVENT:
                    return "WAIT for " + eventVar.LogString() + " to be triggered";
                default: return "Wait";
            }
        }
        public List<string> LogLines(bool detailed, string alinea = null)
        {
            List<string> lines = new();
            StringBuilder sb = new();

            if (alinea != null) sb.Append(alinea);

            switch (conditionType)
            {
                case TimedConditionType.WAIT_FOR_TIME:
                    {
                        sb.Append("~ WAIT for ");
                        sb.Append(timeToWait.LogString());
                        sb.Append(" seconds");
                        Line();
                        break;
                    }
                case TimedConditionType.WAIT_UNTIL_SCENE_CONDITION:
                    {
                        sb.Append("~ WAIT UNTIL : ");
                        if (!detailed) sb.Append("Condition");
                        Line();
                        if (detailed)
                        {
                            for (int i = 0; i < sceneConditions.Count; i++)
                            {
                                sb.Append("     ");
                                sb.Append(sceneConditions[i].ToString());
                                if (i < sceneConditions.Count - 1)
                                {
                                    Line();
                                    sb.Append("     ");
                                    sb.Append(sceneConditions[i].logicOperator);
                                }
                                Line();
                            }
                        }
                        break;
                    }
                case TimedConditionType.WAIT_WHILE_SCENE_CONDITION:
                    {
                        sb.Append("~ WAIT WHILE : ");
                        if (!detailed) sb.Append("Condition");
                        Line();
                        if (detailed)
                        {
                            for (int i = 0; i < sceneConditions.Count; i++)
                            {
                                sb.Append("     ");
                                sb.Append(sceneConditions[i].ToString());
                                if (i < sceneConditions.Count - 1)
                                {
                                    Line();
                                    sb.Append(sceneConditions[i].logicOperator);
                                }
                                Line();
                            }
                        }
                        break;
                    }
                case TimedConditionType.WAIT_FOR_EVENT:
                    {
                        sb.Append("~ WAIT for ");
                        sb.Append(eventVar.LogString());
                        sb.Append(" to be triggered");
                        Line();
                        break;
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
        #endregion

        #region Dependencies
        public List<int> Dependencies
        {
            get
            {
                List<int> dependencies = new();
                switch (conditionType)
                {
                    case TimedConditionType.WAIT_FOR_TIME:
                        dependencies.AddRange(timeToWait.Dependencies);
                        break;
                    case TimedConditionType.WAIT_UNTIL_SCENE_CONDITION:
                        dependencies.AddRange(sceneConditions.Dependencies());
                        break;
                    case TimedConditionType.WAIT_WHILE_SCENE_CONDITION:
                        dependencies.AddRange(sceneConditions.Dependencies());
                        break;
                    case TimedConditionType.WAIT_FOR_EVENT:
                        dependencies.AddRange(eventVar.Dependencies);
                        break;
                }

                return dependencies;
            }
        }
        public bool DependOn(int UID) { return Dependencies.Contains(UID); }
        #endregion
    }
}
