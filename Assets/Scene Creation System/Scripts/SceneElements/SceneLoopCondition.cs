using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Dhs5.SceneCreation.SceneTimedCondition;

namespace Dhs5.SceneCreation
{
    [Serializable]
    public class SceneLoopCondition
    {
        public enum LoopConditionType
        {
            SCENE = 0,
            TIMED = 1,
            ITERATION = 2,
        }

        public LoopConditionType conditionType;

        [SerializeField] private bool loop;
        
        public SceneVarTween timeToWait;
        public SceneVarTween iterationNumber;
        public List<SceneCondition> sceneConditions;
        
        
        private float startTime;
        private int currentIteration = 0;

        public bool DoLoop => loop;
        public bool TimedCondition
        {
            get => conditionType == LoopConditionType.TIMED;
        }

        public bool CurrentConditionResult
        {
            get
            {
                switch (conditionType)
                {
                    case LoopConditionType.TIMED:
                        return Time.time - startTime >= timeToWait.FloatValue;
                    case LoopConditionType.SCENE:
                        return sceneConditions.VerifyConditions();
                    case LoopConditionType.ITERATION:
                        currentIteration++;
                        return currentIteration >= iterationNumber.IntValue;
                    default:
                        return true;
                }
            }
        }

        public void SetUp(SceneVariablesSO sceneVariablesSO)
        {
            sceneConditions.SetUp(sceneVariablesSO);
            timeToWait.SetUp(sceneVariablesSO, SceneVarType.FLOAT, true);
            iterationNumber.SetUp(sceneVariablesSO, SceneVarType.INT, true);
        }

        public void StartTimer()
        {
            startTime = Time.time;
        }

        public void Reset()
        {
            currentIteration = 0;
            startTime = Time.time;
        }

        #region Log
        public List<string> LogLines(bool detailed, string alinea = null)
        {
            List<string> lines = new();
            StringBuilder sb = new();

            if (alinea != null) sb.Append(alinea);

            switch (conditionType)
            {
                case LoopConditionType.SCENE:
                    {
                        sb.Append("~ LOOP until : ");
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
                case LoopConditionType.TIMED:
                    {
                        sb.Append("~ LOOP for ");
                        sb.Append(timeToWait.LogString());
                        sb.Append(" seconds");
                        Line();
                        break;
                    }
                case LoopConditionType.ITERATION:
                    {
                        sb.Append("~ LOOP ");
                        sb.Append(iterationNumber.LogString());
                        sb.Append(" times");
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
                    case LoopConditionType.ITERATION:
                        dependencies.AddRange(iterationNumber.Dependencies);
                        break;
                    case LoopConditionType.SCENE:
                        dependencies.AddRange(sceneConditions.Dependencies());
                        break;
                    case LoopConditionType.TIMED:
                        dependencies.AddRange(timeToWait.Dependencies);
                        break;
                }

                return dependencies;
            }
        }
        public bool DependOn(int UID) { return Dependencies.Contains(UID); }
        public void SetForbiddenUID(int UID) { }
        #endregion
    }
}
