using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dhs5.SceneCreation
{
    [Serializable]
    public abstract class SceneProfile : SceneState.ISceneVarSetupable, SceneState.ISceneObjectBelongable, SceneState.IInitializable, SceneState.ISceneSubscribable
    {
        protected SceneVariablesSO sceneVariablesSO;

        protected BaseSceneObject sceneObject;

        public abstract string Name { get; }

        #region SceneObject Override Permissions
        public virtual bool CanOverrideListeners => false;
        public virtual bool CanOverrideEvents => false;
        #endregion

        #region Interfaces
        public void Init()
        {
            RegisterSceneEventsLists();
            RegisterTweens();

            InitSceneEventsLists();
        }
        public virtual void SetUp(SceneVariablesSO _sceneVariablesSO)
        {
            sceneVariablesSO = _sceneVariablesSO;

            // Set Up Scene Events, Tweens and Listeners
        }
        public virtual void BelongTo(BaseSceneObject _sceneObject)
        {
            UpdateSceneEventsBelongings(_sceneObject);
            UpdateTweensBelongings(_sceneObject);
        }
        public virtual void Subscribe()
        {

        }
        public virtual void Unsubscribe()
        {

        }
        #endregion

        #region Overridable Functions
        public virtual void Attach(BaseSceneObject _sceneObject)
        {
            sceneObject = _sceneObject;

            BelongTo(_sceneObject);
        }
        public virtual void Detach()
        {
            sceneObject = null;

            BelongTo(null);

            UnregisterSceneEvents();
            UnregisterTweens();
        }
        #endregion

        #region Abstract Functions
        /// <summary>
        /// Function where all the <see cref="List{T}"/> of <see cref="BaseSceneEvent"/> should be registered with <see cref="Register{T}(List{T})"/>
        /// </summary>
        protected abstract void RegisterSceneEventsLists();
        /// <summary>
        /// Function where all the <see cref="SceneVarTween"/> should be registered with <see cref="Register(SceneVarTween)"/>
        /// </summary>
        protected abstract void RegisterTweens();
        #endregion

        #region Profile Override

        public abstract bool Override<T>(T overridingProfile) where T : SceneProfile;

        #endregion

        #region Scene Events Management
        public List<string> EventsIDs { get; private set; } = new();
        protected List<List<BaseSceneEvent>> sceneEventsList = new();

        protected bool HasEvent(string eventID)
        {
            if (!EventsIDs.IsValid()) return false;

            return EventsIDs.Contains(eventID);
        }

        #region Registration
        protected void Register<T>(List<T> sceneEvents, bool registerEventIDs = true) where T : BaseSceneEvent
        {
            sceneEventsList.Add(sceneEvents.Cast<BaseSceneEvent>().ToList());
            if (registerEventIDs)
                foreach (var s in sceneEvents)
                    if (!string.IsNullOrWhiteSpace(s.eventID))
                        EventsIDs.Add(s.eventID);
        }
        protected void UnregisterSceneEvents()
        {
            EventsIDs?.Clear();
            sceneEventsList?.Clear();
        }
        #endregion
        private void InitSceneEventsLists()
        {
            if (sceneEventsList == null || sceneEventsList.Count <= 0) return;

            foreach (var s in sceneEventsList)
            {
                s.Init();
            }
        }
        private void UpdateSceneEventsBelongings(BaseSceneObject _sceneObject)
        {
            if (sceneEventsList == null || sceneEventsList.Count <= 0) return;

            foreach (var s in sceneEventsList)
            {
                s.BelongTo(_sceneObject);
            }
        }
        #endregion

        #region Scene Events Triggering
        /// <summary>
        /// Triggers all the <see cref="List{T}"/> of <see cref="BaseSceneEvent"/> of this profile
        /// </summary>
        public virtual void Trigger(params object[] vars)
        {
            if (!sceneEventsList.IsValid()) return;

            foreach (var l in sceneEventsList)
                l.Trigger();
        }
        public virtual void TriggerWithID(string eventID, params object[] vars)
        {
            if (HasEvent(eventID))
            {
                foreach (var l in sceneEventsList)
                {
                    l.TriggerWithID(eventID);
                }
            }
        }
        // ----- REMOVE -----
        /// <summary>
        /// For every <see cref="BaseSceneEvent"/> list in <see cref="sceneEventsList"/> : <br></br>
        /// "<inheritdoc cref="SceneState.TriggerAndRemove{T}(List{T}, bool)"/> "
        /// </summary>
        /// <param name="onlyIfTriggered">Whether to remove only triggered events or all of them</param>
        public virtual void TriggerAndRemove(bool onlyIfTriggered)
        {
            foreach (var l in sceneEventsList)
            {
                l.TriggerAndRemove(onlyIfTriggered);
            }
        }
        /// <summary>
        /// For every <see cref="BaseSceneEvent"/> list in <see cref="sceneEventsList"/> : <br></br>
        /// "<inheritdoc cref="SceneState.TriggerAndRemoveWithID{T}(List{T}, string, bool)"/> "
        /// </summary>
        /// <param name="eventID">ID of the <see cref="BaseSceneEvent"/>s to trigger</param>
        /// <param name="onlyIfTriggered">Whether to remove only triggered events or all of them</param>
        public virtual void TriggerAndRemoveWithID(string eventID, bool onlyIfTriggered)
        {
            if (HasEvent(eventID))
            {
                foreach (var l in sceneEventsList)
                {
                    l.TriggerAndRemoveWithID(eventID, onlyIfTriggered);
                }
            }
        }
        // ----- RANDOM -----
        public virtual bool TriggerRandom(string filter = null, bool remove = false)
        {
            if (!sceneEventsList.IsValid()) return false;

            return sceneEventsList[Random.Range(0, sceneEventsList.Count)].TriggerRandom(filter, remove);
        }
        #endregion

        #region Tweens Management
        protected List<SceneVarTween> tweensList = new();

        protected void Register(SceneVarTween tween)
        {
            tweensList.Add(tween);
        }
        protected void UnregisterTweens()
        {
            tweensList?.Clear();
        }
        private void UpdateTweensBelongings(BaseSceneObject _sceneObject)
        {
            if (tweensList == null || tweensList.Count <= 0) return;

            foreach (var t in tweensList)
            {
                t.BelongTo(_sceneObject);
            }
        }
        #endregion
    }
}
