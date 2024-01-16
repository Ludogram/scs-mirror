using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Dhs5.SceneCreation
{    
    [Serializable]
    public class TimelineObject : SceneState.ISceneVarSetupable, SceneState.ISceneObjectBelongable, SceneState.IInitializable, SceneState.ISceneVarDependant, SceneState.ISceneLogable
    {
        public string TimelineID { get; private set; }
        public int StepNumber { get; private set; }

        [SerializeField] private string ID;
        public SceneTimedCondition startCondition;
        public SceneLoopCondition endLoopCondition;
        public bool Loop => endLoopCondition.DoLoop;
        
        // Action
        public List<SceneEvent<TimelineContext>> sceneEvents;

        [SerializeField] private float propertyHeight;

        private IEnumerator startConditionCR;
        private bool executing;
        private bool canInterrupt;

        public void Init()
        {
            sceneEvents.Init();
        }
        public void SetUp(SceneVariablesSO sceneVariablesSO)
        {
            sceneEvents.SetUp(sceneVariablesSO);
            
            startCondition.SetUp(sceneVariablesSO);
            endLoopCondition.SetUp(sceneVariablesSO);
        }
        public void BelongTo(BaseSceneObject _sceneObject)
        {
            sceneEvents.BelongTo(_sceneObject);
        }

        public IEnumerator Process(TimelineContext context)
        {
            TimelineID = context.TimelineID;
            StepNumber = context.CurrentStepNumber;
            
            // Reset the end loop condition
            endLoopCondition.Reset();
            
            do
            {
                executing = true;
                context.StepLoop();
                
                // Wait for the condition to be verified
                startConditionCR = startCondition.Condition();
                yield return StartCoroutine(startConditionCR);

                if (executing || !canInterrupt)
                {
                    // Trigger Events
                    context.Trigger();
                    Trigger(context);
                }

            } while (Loop && !endLoopCondition.CurrentConditionResult && executing);
        }

        private void Trigger(TimelineContext context)
        {
            sceneEvents.Trigger(context);
        }

        #region Utility
        private IEnumerator StartCoroutine(IEnumerator Coroutine)
        {
            yield return SceneClock.Instance.StartCoroutine(Coroutine);
        }
        public void StopExecution(bool interrupt)
        {
            executing = false;
            canInterrupt = interrupt;
            if (interrupt) startCondition.BreakCoroutine();
        }
        public void StopCoroutine()
        {
            SceneClock.Instance.StopCoroutine(startConditionCR);
        }
        #endregion

        #region Log
        public List<string> LogLines(bool detailed = false, bool showEmpty = false, string alinea = null)
        {
            List<string> lines = new();
            StringBuilder sb = new();

            if (alinea != null) sb.Append(alinea);
            lines.AddRange(startCondition.LogLines(detailed, alinea));
            //Line();
            sb.Append("~ THEN : ");
            Line();
            foreach (var events in sceneEvents)
            {
                lines.AddRange(events.LogLines(detailed, showEmpty, alinea + "     "));
            }

            if (Loop)
            {
                lines.AddRange(endLoopCondition.LogLines(detailed, alinea));
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
            return sceneEvents.IsEmpty();
        }
        #endregion

        #region Dependencies
        public List<int> Dependencies
        {
            get
            {
                List<int> dependencies = new();

                dependencies.AddRange(startCondition.Dependencies);
                dependencies.AddRange(sceneEvents.Dependencies());
                if (Loop) dependencies.AddRange(endLoopCondition.Dependencies);

                return dependencies;
            }
        }
        public bool DependOn(int UID) { return Dependencies.Contains(UID); }
        #endregion
    }
}
