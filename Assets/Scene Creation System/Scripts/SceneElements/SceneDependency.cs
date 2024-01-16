using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    [Serializable]
    public class SceneDependency : SceneState.ISceneVarSetupable
    {
        [SerializeField] private SceneVariablesSO sceneVariablesSO;

        [SerializeField] private SceneVarTween sceneVar;
        [SerializeField] private List<BaseSceneObject> sceneObjects = new();
        [SerializeField] private List<string> sceneVars = new();

        [SerializeField] private float propertyHeight;

        public void SetUp(SceneVariablesSO _sceneVariablesSO)
        {
            sceneVariablesSO = _sceneVariablesSO;
            sceneVar?.SetUp(_sceneVariablesSO, SceneVarType.INT, false, true);
        }

        internal void GetSceneObjectDependencies(BaseSceneObject baseSceneObject)
        {
            sceneVars.Clear();

            List<int> deps = new();

            foreach (var d in baseSceneObject.Dependencies)
            {
                if (!deps.Contains(d))
                {
                    deps.Add(d);
                }
            }

            foreach (var d in deps)
            {
                sceneVars.Add(sceneVariablesSO[d]?.LogString());
            }
        }
        internal void GetSceneVarDependants()
        {
            sceneObjects.Clear();

            foreach (var so in GameObject.FindObjectsOfType<BaseSceneObject>())
            {
                if (so.DependOn(sceneVar.UID))
                {
                    sceneObjects.Add(so);
                }
            }
        }
        internal static List<BaseSceneObject> GetSceneVarDependants(int UID)
        {
            List<BaseSceneObject> sceneObjs = new();
            foreach (var so in GameObject.FindObjectsOfType<BaseSceneObject>())
            {
                if (so.DependOn(UID))
                {
                    sceneObjs.Add(so);
                }
            }

            return sceneObjs;
        }

        public static List<BaseSceneObject> GetDependencies(BaseVariablesContainer container, int UID)
        {
            List<BaseSceneObject> sceneObjects = new();

            foreach (var so in GameObject.FindObjectsOfType<BaseSceneObject>())
            {
                if ((container is IntersceneVariablesSO || 
                    (container is SceneVariablesSO sceneVariablesSO && so.SceneVariablesSO == sceneVariablesSO)) 
                    && so.DependOn(UID))
                {
                    sceneObjects.Add(so);
                }
            }

            return sceneObjects;
        }
        public static bool IsValidInCurrentScene(BaseVariablesContainer container)
        {
            if (container is IntersceneVariablesSO) return true;

            if (container is SceneVariablesSO sceneVariablesSO)
            {
                SceneManager manager = GameObject.FindObjectOfType<SceneManager>();
                if (manager != null)
                {
                    return manager.SceneVariablesSO == sceneVariablesSO;
                }
            }
            return false;
        }
    }
}
