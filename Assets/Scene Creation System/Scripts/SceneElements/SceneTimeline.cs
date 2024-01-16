using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dhs5.SceneCreation
{
    [Serializable]
    public class SceneTimeline : SceneState.ISceneVarSetupable, SceneState.ISceneObjectBelongable, SceneState.IInitializable, SceneState.ISceneVarDependant, SceneState.ISceneLogable
    {
        public string ID;
        public SceneLoopCondition endLoopCondition;
        [FormerlySerializedAs("timelineObjects")]
        public List<TimelineObject> steps;

        public bool Loop => endLoopCondition.DoLoop;
        
        public bool IsActive { get; private set; }
        public TimelineContext Context { get; private set; }

        private Coroutine coroutine;
        private Queue<TimelineObject> timelineQueue = new();
        private TimelineObject currentTimelineObject;
        private int currentStep;
        
        public void Init()
        {
            steps.Init();
        }
        public void SetUp(SceneVariablesSO sceneVariablesSO)
        {
            endLoopCondition.SetUp(sceneVariablesSO);
            steps.SetUp(sceneVariablesSO);
        }
        public void BelongTo(BaseSceneObject _sceneObject)
        {
            steps.BelongTo(_sceneObject);
        }

        private IEnumerator TimelineRoutine()
        {
            bool start = true;

            IsActive = true;

            //Reset the end loop condition
            endLoopCondition.Reset();

            currentTimelineObject = timelineQueue.Dequeue();
            currentStep++;
            Context = new(this, currentTimelineObject, currentStep);
            yield return StartCoroutine(currentTimelineObject.Process(Context));

            do
            {
                if (!start)
                    Context.TimelineLoop();
                else
                    start = false;

                //Debug.LogError(ID + " begin at step : " + currentStep + " at : " + Time.time);
                for (;timelineQueue.Count > 0;)
                {
                    currentTimelineObject = timelineQueue.Dequeue();
                    currentStep++;
                    Context = Context.CreateNext(currentTimelineObject, currentStep);
                    yield return StartCoroutine(currentTimelineObject.Process(Context));
                }
                SetUpQueue();
            } while (Loop && !endLoopCondition.CurrentConditionResult);
            //Debug.LogError(ID + " ended at : " + Time.time);

            IsActive = false;
        }
        
        #region Timeline Actions
        public void Start(int step = 0)
        {
            if (IsActive) return;
            
            SetUpQueue(step);
            if (timelineQueue.Count > 0)
                coroutine = StartMainCR(TimelineRoutine());
        }
        public void Stop()
        {
            if (!IsActive) return;
            
            StopMainCR();
            currentTimelineObject.StopCoroutine();

            IsActive = false;
        }
        public void GoToStep(int step, bool interrupt)
        {
            Debug.LogError(ID + " GoTo step : " + step);
            SetUpQueue(step);
            currentTimelineObject.StopExecution(interrupt);
        }
        public void StartOrGoTo(int step, bool interrupt)
        {
            if (IsActive)
            {
                GoToStep(step, interrupt);
                return;
            }
            Start(step);
        }
        #endregion
        
        #region Utility
        private void SetUpQueue(int step = 0)
        {
            timelineQueue = new();
            currentStep = step - 1;

            for (int i = step; i < steps.Count; i++)
            {
                timelineQueue.Enqueue(steps[i]);
            }
        }
        
        private Coroutine StartMainCR(IEnumerator Coroutine)
        {
            return SceneClock.Instance.StartCoroutine(Coroutine);
        }
        private void StopMainCR()
        {
            SceneClock.Instance.StopCoroutine(coroutine);
        }

        private IEnumerator StartCoroutine(IEnumerator Coroutine)
        {
            yield return SceneClock.Instance.StartCoroutine(Coroutine);
        }
        #endregion

        #region Log
        public string Log(bool detailed = false, bool showEmpty = false)
        {
            return ((SceneState.ISceneLogableWithChild)this).Log(detailed, showEmpty);
        }
        public List<string> LogLines(bool detailed = false, bool showEmpty = false, string alinea = null)
        {
            string passToLine = "Line()";
            List<string> lines = new();
            StringBuilder sb = new();

            AppendColor(SceneLogger.TimelineColor, "| ");
            sb.Append("Timeline ID : ");
            sb.Append(ID);
            Line();

            if (detailed)
            {

                if (Loop)
                {
                    lines.AddRange(endLoopCondition.LogLines(detailed));
                }

                for (int i = 0; i < steps.Count; i++)
                {
                    sb.Append("   ");
                    AppendColor(SceneLogger.TimelineColor, "* ");
                    sb.Append("Step ");
                    sb.Append(i);
                    sb.Append(":");
                    Line();
                    lines.AddRange(steps[i].LogLines(detailed, showEmpty, "      "));
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
            void AppendColor(string color, params string[] strings)
            {
                sb.Append(color);
                foreach (var s in strings)
                {
                    if (s == passToLine) Line();
                    else sb.Append(s);
                }
                sb.Append(SceneLogger.ColorEnd);
            }
            #endregion
        }

        public bool IsEmpty()
        {
            return steps.IsEmpty();
        }
        #endregion

        #region Dependencies
        public List<int> Dependencies
        {
            get
            {
                List<int> dependencies = new();

                dependencies.AddRange(steps.Dependencies());
                if (Loop) dependencies.AddRange(endLoopCondition.Dependencies);

                return dependencies;
            }
        }
        public bool DependOn(int UID) { return Dependencies.Contains(UID); }
        #endregion
    }
}
