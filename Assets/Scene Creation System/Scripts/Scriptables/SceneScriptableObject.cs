using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    public abstract class SceneScriptableObject : ScriptableObject, SceneState.ISceneVarSetupable
    {
        [SerializeField, HideInInspector] protected SceneVariablesSO sceneVariablesSO;
        public SceneVariablesSO SceneVariablesSO => sceneVariablesSO;

        [SerializeField, HideInInspector] protected BaseSceneObject sceneObject;

        #region Link
        public bool Linked { get; private set; } = false;
        public void Link(BaseSceneObject _sceneObject)
        {
            if (Linked)
            {
                Debug.LogError("Tried to link " + name + " with " + _sceneObject.name + " while it is already linked with " + sceneObject.name);
                return;
            }
            
            Linked = true;

            sceneObject = _sceneObject;

            Init();
            SetBelongings(_sceneObject);

            OnScriptableAwake();
        }

        public void SetUp(SceneVariablesSO _sceneVariablesSO)
        {
            sceneVariablesSO = _sceneVariablesSO;
            UpdateSceneVariables();
        }
        #endregion

        #region Base
        private void OnValidate()
        {
            UpdateSceneVariables();

            OnScriptableValidate();
        }
        public void OnSceneObjectEnable()
        {
            RegisterElements();

            OnScriptableEnable();
        }
        public void OnSceneObjectDisable()
        {
            UnregisterElements();

            OnScriptableDisable();
        }
        #endregion

        #region Abstracts
        /// <summary>
        /// Called on <see cref="Awake"/>.<br></br>
        /// Init <see cref="SceneState.IInitializable"/>s elements HERE.
        /// </summary>
        protected abstract void Init();
        /// <summary>
        /// Called on <see cref="Awake"/>.<br></br>
        /// Update the belongings of <see cref="SceneState.ISceneObjectBelongable"/>s elements to this object HERE.
        /// </summary>
        protected abstract void SetBelongings(BaseSceneObject sceneObject);
        /// <summary>
        /// Called on <see cref="OnValidate"/>.<br></br>
        /// Update the <see cref="Dhs5.SceneCreation.SceneVariablesSO"/> of <see cref="SceneState.ISceneVarSetupable"/> and <see cref="SceneState.ISceneVarTypedSetupable"/> elements HERE.
        /// </summary>
        protected abstract void UpdateSceneVariables();
        /// <summary>
        /// Called on <see cref="OnEnable"/>.<br></br>
        /// Register <see cref="SceneState.ISceneRegisterable"/>s elements HERE.
        /// </summary>
        protected abstract void RegisterElements();
        /// <summary>
        /// Called on <see cref="OnDisable"/>.<br></br>
        /// Unregister <see cref="SceneState.ISceneRegisterable"/>s elements HERE.
        /// </summary>
        protected abstract void UnregisterElements();
        #endregion

        #region Extensions
        /// <summary>
        /// Called on <see cref="Link(BaseSceneObject)"/>.
        /// </summary>
        protected virtual void OnScriptableAwake() { }
        /// <summary>
        /// <see cref="OnValidate"/> extension.
        /// </summary>
        protected virtual void OnScriptableValidate() { }
        /// <summary>
        /// <see cref="OnSceneObjectEnable"/> extension.
        /// </summary>
        protected virtual void OnScriptableEnable() { }
        /// <summary>
        /// <see cref="OnSceneObjectDisable"/> extension.
        /// </summary>
        protected virtual void OnScriptableDisable() { }
        #endregion
    }
}
