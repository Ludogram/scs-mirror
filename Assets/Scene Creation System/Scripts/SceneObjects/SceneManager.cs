using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;
using Mirror;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dhs5.SceneCreation
{
    public class SceneManager : BaseSceneObject
    {
        #region Registration

        protected override void OnSceneObjectEnable()
        {
            base.OnSceneObjectEnable();

            NetworkSceneVariablesContainer.Register(this);
        }
        protected override void OnSceneObjectDisable()
        {
            base.OnSceneObjectDisable();

            NetworkSceneVariablesContainer.Unregister(this);
        }

        #endregion

        #region ID Range

        [SerializeField] private Vector2Int _sceneObjectsIDRange;
        public Vector2Int IDRange => _sceneObjectsIDRange;

        internal void FixIDRange()
        {
            Vector2Int previousRange = _sceneObjectsIDRange;
            _sceneObjectsIDRange = new Vector2Int(1, 1000);
            Debug.LogError("Had to fix " + name + " ID range from " + previousRange + " to " + _sceneObjectsIDRange);

            Refresh();
        }

        protected override void OnSceneObjectValidate()
        {
            base.OnSceneObjectValidate();

            if (!PrefabUtility.IsPartOfPrefabAsset(gameObject) && (IDRange.x >= IDRange.y || IDRange.x < 0))
            {
                FixIDRange();
            }
        }

        #endregion

        public override string DisplayName => "Scene Manager";

        [Header("Main Game Events")]
        [Tooltip("Events called when the Scene starts,\n just before every SceneObject.OnStartScene()")]
        [SerializeField] protected List<SceneEvent> onSceneStart;
        [Tooltip("Events called when the Scene is going to change,\n just before every SceneObject.OnChangeScene()")]
        [SerializeField] protected List<SceneEvent> onSceneChange;
        [Tooltip("Events called when the Scene is completed,\n just before every SceneObject.OnCompleteScene()")]
        [SerializeField] protected List<SceneEvent> onSceneComplete;
        [Tooltip("Events called on GameOver,\n just before every SceneObject.OnGameOver()")]
        [SerializeField] protected List<SceneEvent> onGameOver;

        /// <summary>
        /// Event called when the Scene starts for non-<see cref="SceneObject"/> elements to subscribe
        /// </summary>
        public static event Action SceneStartEvent;
        /// <summary>
        /// Event called when the Scene is going to change for non-<see cref="SceneObject"/> elements to subscribe
        /// </summary>
        public static event Action SceneChangeEvent;
        /// <summary>
        /// Event called when the Scene is completed for non-<see cref="SceneObject"/> elements to subscribe
        /// </summary>
        public static event Action SceneCompleteEvent;
        /// <summary>
        /// Event called on GameOver for non-<see cref="SceneObject"/> elements to subscribe
        /// </summary>
        public static event Action GameOverEvent;

        public virtual void StartNetworkScene()
        {
            SetBalancingIndex();

            SetSceneVars();

            StartScene();
        }

        #region SceneObject Extension
        protected override void UpdateSceneVariables()
        {
            Setup(onSceneStart, onSceneChange, onGameOver);
        }
        protected override void RegisterSceneElements()
        {
            RegisterEvents(
                (nameof(onSceneStart), onSceneStart),
                (nameof(onSceneChange), onSceneChange),
                (nameof(onGameOver), onGameOver));
        }
        #endregion


        #region Scene Main Events

        protected virtual void StartScene()
        {
            onSceneStart.Trigger();
            SceneState.StartScene();

            SceneStartEvent?.Invoke();
        }
        public virtual void ChangeScene()
        {
            onSceneChange.Trigger();
            SceneState.ChangeScene();

            SceneChangeEvent?.Invoke();
        }
        public virtual void CompleteScene()
        {
            onSceneComplete.Trigger();
            SceneState.CompleteScene();

            SceneCompleteEvent?.Invoke();
        }
        public virtual void GameOver()
        {
            onGameOver.Trigger();
            SceneState.GameOver();

            GameOverEvent?.Invoke();
        }
        #endregion

        #region Scenes Management

        #endregion

        #region SceneVars Setup
        /// <summary>
        /// Set the <see cref="SceneState"/>'s SceneVars at the beginning of the Scene
        /// </summary>
        protected void SetSceneVars()
        {
            if (NetworkServer.active) SceneState.SetSceneVars(sceneVariablesSO, BalancingIndex);
        }
        #endregion

        #region Balancing Setup
        /// <summary>
        /// Index of the balancing sheet to use for this scene.<br/>
        /// <b>0 is the base SceneVariablesSO, 1 is the first BalancingSheet of the list</b>
        /// </summary>
        public static int BalancingIndex { get; protected set; } = 0;

        /// <summary>
        /// Function called in <see cref="Start"/> just before <see cref="SetSceneVars"/>.<br></br>
        /// By default : <c>BalancingIndex = IntersceneState.BalancingLevel;</c><br></br><br></br>
        /// Override this function to change the <see cref="BalancingIndex"/>.
        /// </summary>
        protected virtual void SetBalancingIndex()
        {
            BalancingIndex = IntersceneState.BalancingLevel;
        }

        /// <summary>
        /// Update the balancing during a Scene <b>ARLEADY SETUP</b><br/>
        /// and update <b>ONLY the STATIC and RANDOM vars</b><br/>
        /// without triggering any <i>ChangedVar event</i>.<br></br>
        /// Only the balancing of this scene will be changed.
        /// </summary>
        /// <remarks>Never use before <see cref="SetSceneVars"/></remarks>
        /// <param name="balancingIndex">Index of the balancing sheet to apply</param>
        public void UpdateBalancing(int balancingIndex)
        {
            BalancingIndex = balancingIndex;
            SceneState.ActuBalancing(sceneVariablesSO, BalancingIndex);
        }
        #endregion

        #region Editor

#if UNITY_EDITOR
        /// <summary>
        /// !!! EDITOR FUNCTION !!! 
        /// Do not use at runtime !
        /// </summary>
        /// <param name="_sceneVariablesSO"></param>
        internal void SetSceneVariablesSO(SceneVariablesSO _sceneVariablesSO)
        {

            sceneVariablesSO = _sceneVariablesSO;
        }

        public void DebugSceneEventManagerListeners()
        {
            SceneEventManager.DebugListeners();
        }
        public void DebugRegisteredSceneObjects()
        {
            SceneState.DebugRegisteredSceneObjects();
        }
        public void DebugSceneVariables()
        {
            SceneState.DebugSceneVariables();
        }
#endif
        
        #endregion
    }
}
