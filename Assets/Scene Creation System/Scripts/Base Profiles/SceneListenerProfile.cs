using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    public class SceneListenerProfile : SceneProfile
    {
        public List<SceneListener> sceneListeners;

        #region Overrides
        public override void SetUp(SceneVariablesSO _sceneVariablesSO)
        {
            base.SetUp(_sceneVariablesSO);

            sceneListeners.SetUp(sceneVariablesSO);
        }
        public override void BelongTo(BaseSceneObject _sceneObject)
        {
            base.BelongTo(_sceneObject);

            sceneListeners.BelongTo(sceneObject);
        }
        public override void Attach(BaseSceneObject _sceneObject)
        {
            base.Attach(_sceneObject);

            sceneListeners.Subscribe();

            if (_sceneObject is SceneObject so)
                so.OverrideListeners(this, sceneListeners);
        }
        public override void Detach()
        {
            base.Detach();

            sceneListeners.Unsubscribe();
        }

        public override bool CanOverrideListeners => true;

        public override bool Override<T>(T overridingProfile)
        {
            if (overridingProfile is SceneListenerProfile p)
            {
                sceneListeners.Clear();
                sceneListeners.AddRange(p.sceneListeners);
                return true;
            }
            return false;
        }

        public override string Name => "Scene Listener Profile";
        #endregion

        #region Scene Events
        protected override void RegisterSceneEventsLists() { }
        protected override void RegisterTweens() { }
        #endregion
    }
}
