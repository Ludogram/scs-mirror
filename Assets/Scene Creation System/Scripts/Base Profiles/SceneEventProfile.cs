using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    public class SceneEventProfile : SceneProfile
    {
        public List<SceneEvent<SceneEventParam>> sceneEvents;

        #region Overrides
        public override void SetUp(SceneVariablesSO _sceneVariablesSO)
        {
            base.SetUp(_sceneVariablesSO);

            sceneEvents.SetUp(sceneVariablesSO);
        }

        public override void Attach(BaseSceneObject _sceneObject)
        {
            base.Attach(_sceneObject);

            if (_sceneObject is SceneObject so)
                so.OverrideEvents(this, sceneEvents);
        }

        public override bool CanOverrideEvents => true;

        public override bool Override<T>(T overridingProfile)
        {
            if (overridingProfile is SceneEventProfile p)
            {
                sceneEvents.Clear();
                sceneEvents.AddRange(p.sceneEvents);
                return true;
            }
            return false;
        }

        public override string Name => "Scene Event Profile";
        #endregion

        #region Scene Events
        protected override void RegisterSceneEventsLists()
        {
            Register(sceneEvents, false);
        }
        protected override void RegisterTweens()
        {
            // No Tweens
        }
        #endregion
    }
}
