using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Events;
using System.Text;

namespace Dhs5.SceneCreation
{
    public class SceneClock : BaseSceneObject
    {
        public override string DisplayName => "Scene Clock";

        #region Singleton

        public static SceneClock Instance { get; private set; }

        protected override void OnSceneObjectAwake()
        {
            base.OnSceneObjectAwake();

            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        #endregion

        //public List<SceneTimeline> sceneTimelines;
        public SceneTimelineList sceneTimelines;

        #region BaseSceneObject Extension
        protected override void Init()
        {
            base.Init();

            sceneTimelines.Init();
        }
        protected override void UpdateSceneVariables()
        {
            Setup(sceneTimelines);
        }
        protected override void SetBelongings()
        {
            base.SetBelongings();

            Belong(sceneTimelines);
        }
        protected override void RegisterSceneElements() { }
        #endregion

        #region Listener functions
        [Preserve]
        public void StartTimeline(string timelineID, int step)
        {
            sceneTimelines.Find(t => t.ID == timelineID)?.Start(step);
        }
        public void StartTimeline(string timelineID) { StartTimeline(timelineID, 0); }
        [Preserve]
        public void StopTimeline(string timelineID)
        {
            sceneTimelines.Find(t => t.ID == timelineID)?.Stop();
        }
        [Preserve]
        public void GoToStep(string timelineID, int step, bool interrupt)
        {
            sceneTimelines.Find(t => t.ID == timelineID)?.StartOrGoTo(step, interrupt);
        }
        public void GoToPreviousStep(TimelineContext context)
        {
            sceneTimelines.Find(t => t.ID == context.TimelineID)?.StartOrGoTo(context.CurrentStepNumber - 1, false);
        }
        public void GoToNextStep(TimelineContext context)
        {
            sceneTimelines.Find(t => t.ID == context.TimelineID)?.StartOrGoTo(context.CurrentStepNumber + 1, false);
        }
        public void DebugContext(TimelineContext context)
        {
            Debug.Log(context);
        }
        #endregion

        #region Log
        public override void ChildLog(List<string> lines, StringBuilder sb, bool detailed, bool showEmpty, string alinea = null)
        {
            string passToLine = "Line()";

            base.ChildLog(lines, sb, detailed, showEmpty);

            // Clear String Builder
            sb.Clear();

            // First Alinea
            if (alinea != null) sb.Append(alinea);

            AppendColor(SceneLogger.TimelineColor, "Timelines :");
            Line();

            if (sceneTimelines != null && sceneTimelines.Count > 0)
            {
                AppendColor(SceneLogger.TimelineColor, "----------------------------------------");
                Line();

                foreach (var timeline in sceneTimelines)
                {
                    lines.AddRange(timeline.LogLines(detailed, showEmpty, alinea));
                }

                AppendColor(SceneLogger.TimelineColor, "----------------------------------------");
                Line();
            }
            

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

        public override bool IsChildEmpty()
        {
            return !sceneTimelines.IsValid();
        }
        #endregion

        #region Dependencies
        public override List<int> ChildDependencies()
        {
            return sceneTimelines.Dependencies();
        }
        #endregion
    }

    public class TimelineContext
    {
        #region Constructor
        public TimelineContext(SceneTimeline timeline, TimelineObject current, int stepNumber)
        {
            CurrentStep = current;
            CurrentStepNumber = stepNumber;
            IsCurrentStepLooping = current.Loop;
            CurrentStepLoopIteration = 0;
            CurrentStepStartTime = Time.time;
            CurrentStepTriggerTime = -1f;

            PreviousStep = null;
            TimelineID = timeline.ID;
            IsTimelineLooping = timeline.Loop;
            TimelineLoopIteration = 1;
            TimelineStartTime = Time.time;
        }
        public TimelineContext(TimelineContext previous, TimelineObject current, int stepNumber)
        {
            CurrentStep = current;
            CurrentStepNumber = stepNumber;
            IsCurrentStepLooping = current.Loop;
            CurrentStepLoopIteration = 0;
            CurrentStepStartTime = Time.time;
            CurrentStepTriggerTime = -1f;

            PreviousStep = previous;
            TimelineID = previous.TimelineID;
            IsTimelineLooping = previous.IsTimelineLooping;
            TimelineLoopIteration = previous.TimelineLoopIteration;
            TimelineStartTime = previous.TimelineStartTime;
        }
        #endregion

        #region Variables
        // Steps
        public TimelineContext PreviousStep { get; private set; }
        public TimelineObject CurrentStep { get; private set; }
        public TimelineContext NextStep { get; private set; }

        // IDs
        public string TimelineID { get; private set; }
        public int CurrentStepNumber { get; private set; }

        // Loop
        public bool IsTimelineLooping { get; private set; }
        public bool IsCurrentStepLooping { get; private set; }
        // Loop -> Iteration
        public int TimelineLoopIteration { get; private set; }
        public int CurrentStepLoopIteration { get; private set; }

        // Time
        public float TimelineStartTime { get; private set; }
        public float CurrentStepStartTime { get; private set; }
        public float CurrentStepTriggerTime { get; private set; }



        private bool lastOfTimelineIteration = false;
        #endregion

        #region Functions
        public TimelineContext CreateNext(TimelineObject next, int stepNumber)
        {
            NextStep = new(this, next, stepNumber);
            return NextStep;
        }
        public void TimelineLoop()
        {
            TimelineLoopIteration++;
            lastOfTimelineIteration = true;
        }
        public void StepLoop()
        {
            CurrentStepLoopIteration++;
        }
        public void Trigger()
        {
            CurrentStepTriggerTime = Time.time;
        }
        #endregion

        #region Log
        private void Log(StringBuilder sb)
        {
            if (PreviousStep != null)
            {
                PreviousStep.Log(sb);
            }
            else
            {
                sb.Append("Timeline : ");
                sb.AppendLine(TimelineID);
            }

            sb.Append("-> Step ");
            sb.Append(CurrentStepNumber);
            sb.AppendLine(" :");

            sb.Append("Started at ");
            sb.Append(CurrentStepStartTime);
            sb.Append(" ; Triggered last at ");
            sb.Append(CurrentStepTriggerTime);
            sb.Append(" ; Iterated ");
            sb.Append(CurrentStepLoopIteration);
            sb.AppendLine(" time(s)");

            if (lastOfTimelineIteration)
            {
                sb.Append("----- Timeline Iteration Number ");
                sb.Append(TimelineLoopIteration);
                sb.AppendLine(" -----");
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            Log(sb);

            sb.AppendLine();
            sb.Append("Timeline iterated ");
            sb.Append(TimelineLoopIteration);
            sb.Append(" time(s)");

            return sb.ToString();
        }
        #endregion
    }
}
