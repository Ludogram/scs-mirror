using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    [DisallowMultipleComponent]
    public class SceneObject : BaseSceneObject//MonoBehaviour, SceneState.ISceneVarDependantWithChild, SceneState.ISceneLogableWithChild
    {
        public override string DisplayName => "Scene Object";

        [Header("Listeners")]
        [SerializeField] protected List<SceneListener> sceneListeners;

        [Header("Actions")]
        [SerializeField] protected List<SceneEvent<SceneEventParam>> sceneEvents;

        #region BaseSceneObject Extension
        protected override void RegisterSceneElements()
        {
            RegisterListener(nameof(sceneListeners), sceneListeners);
            RegisterEvent(nameof(sceneEvents), sceneEvents);
        }
        protected override void UpdateSceneVariables()
        {
            Setup(sceneListeners);
            Setup(sceneEvents);
        }
        #endregion

        #region Trigger Events

        #region Exposed Functions
        public void TriggerSceneEvents()
        {
            TriggerSceneEvents(default);
        }
        public void TriggerSceneEventsWithID(string eventID)
        {
            TriggerSceneEventsWithID(eventID, default);
        }
        public void TriggerRandom(string filter)
        {
            TriggerRandom(filter, default);
        }
        public void TriggerRandomAndRemove(string filter)
        {
            TriggerRandomAndRemove(filter, default);
        }
        #endregion

        public void TriggerSceneEvents(SceneEventParam param)
        {
            sceneEvents.Trigger(param);
        }
        public void TriggerSceneEventsWithID(string eventID, SceneEventParam param)
        {
            SceneDebugger.Log(name + " trigger SceneEvents with ID : " + eventID, this, 3);
            sceneEvents.TriggerWithID(param, eventID);
        }
        public void TriggerRandom(string filter, SceneEventParam param)
        {
            sceneEvents.TriggerRandom(param, filter);
        }
        public void TriggerRandomAndRemove(string filter, SceneEventParam param)
        {
            sceneEvents.TriggerRandom(param, filter, true);
        }

        public override void Trigger(SceneListener.SceneEventTrigger trigger, SceneEventParam param)
        {
            if (!trigger.random)
            {
                TriggerSceneEventsWithID(trigger.eventID, param);
            }
            else
            {
                if (!trigger.remove)
                {
                    TriggerRandom(trigger.eventID, param);
                }
                else
                {
                    TriggerRandomAndRemove(trigger.eventID, param);
                }
            }
        }
        #endregion

        #region Profiles Override
        public bool OverrideListeners(SceneProfile profile, List<SceneListener> listeners)
        {
            if (profile == null || !profile.CanOverrideListeners || listeners == null) return false;

            sceneListeners.AddRange(listeners);
            return true;
        }
        public bool OverrideEvents(SceneProfile profile, List<SceneEvent<SceneEventParam>> events)
        {
            if (profile == null || !profile.CanOverrideEvents || events == null) return false;

            sceneEvents.AddRange(events);
            return true;
        }        
        #endregion
    }
}
