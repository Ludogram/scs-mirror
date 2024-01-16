using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    public class SceneElement
    {
        public SceneVariablesSO SceneVars => sceneVariablesSO;
        [SerializeField] protected SceneVariablesSO sceneVariablesSO;

        public BaseSceneObject SceneObj => sceneObject;
        [SerializeField] protected BaseSceneObject sceneObject;


        public void Setup(SceneVariablesSO _sceneVariablesSO, BaseSceneObject _sceneObject)
        {
            sceneVariablesSO = _sceneVariablesSO;
            sceneObject = _sceneObject;
        }
    }
}
