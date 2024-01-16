using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dhs5.Utility.Settings;
using System;
using Dhs5.Utility.DirectoryPicker;
using System.Linq;

#if UNITY_EDITOR
using Dhs5.Utility.Settings.Editor;
using UnityEditor;
#endif

namespace Dhs5.SceneCreation
{
    [Settings(SettingsUsage.EditorProject, "Scene Creation Settings")]
    public class SceneCreationSettings : Settings<SceneCreationSettings>
    {
#if UNITY_EDITOR
        [SettingsProvider]
        static SettingsProvider GetSettingsProvider() =>
        instance.GetSettingsProvider();
#endif

        [SerializeField][ReadOnly("_debugMode", inverse = true)] private IntersceneVariablesSO intersceneVariablesSO;
        public IntersceneVariablesSO IntersceneVars => intersceneVariablesSO;

        [SerializeField][ReadOnly("_debugMode", inverse = true)] private SceneObjectSettings sceneObjectSettings;
        public SceneObjectSettings SceneObjectSettings => sceneObjectSettings;

        [Space(20f)]

        [SerializeField] private DirectoryPicker sceneVariablesDirectory;
        public string SceneVariablesContainerPath => sceneVariablesDirectory.Path;

        [Space(20f)]
        [Header("Editor")]
        [SerializeField] private bool _debugMode;
        public bool IsInDebugMode => _debugMode;

        [Space(10f)]

        [SerializeField] private SceneCreationBasePrefabs sceneCreationPrefabs;
        public SceneCreationBasePrefabs Prefabs => sceneCreationPrefabs;

        [SerializeField] private SceneCreationEditorColors editorColors;
        public SceneCreationEditorColors EditorColors => editorColors;

        #region Setup
        readonly string IntersceneVariablesName = "Interscene Variables Container";
        readonly string SceneObjectSettingsName = "SceneObject Settings";
        public void SetupProject()
        {
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(this);

            UnityEngine.Object[] os = AssetDatabase.LoadAllAssetsAtPath(path);
            List<UnityEngine.Object> objects = os.ToList();

            foreach (UnityEngine.Object o in os)
            {
                Debug.Log(o);
            }

            intersceneVariablesSO = SetupObject<IntersceneVariablesSO>(intersceneVariablesSO, IntersceneVariablesName);

            sceneObjectSettings = SetupObject<SceneObjectSettings>(sceneObjectSettings, SceneObjectSettingsName);

            if (sceneObjectSettings != null)
            {
                sceneObjectSettings.SetupProject();
            }

            AssetDatabase.SaveAssets();

            T SetupObject<T>(UnityEngine.Object obj, string correctName) where T : ScriptableObject
            {
                if (obj == null)
                {
                    UnityEngine.Object foundObj = objects.Find(o => o is T);
                    if (foundObj != null)
                    {
                        obj = foundObj;
                    }
                    else
                    {
                        obj = CreateInstance<T>();
                        AssetDatabase.AddObjectToAsset(obj, path);
                        obj.name = correctName;
                    }
                }
                else if (!objects.Contains(obj))
                {
                    UnityEngine.Object newObj = Instantiate(obj);
                    AssetDatabase.AddObjectToAsset(newObj, path);
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(obj));
                    obj = newObj;
                }

                if (obj != null)
                {
                    obj.hideFlags = HideFlags.HideInHierarchy;
                }

                return obj == null ? null : obj as T;
            }
#endif
        }
        internal void SetSettingsAssetsVisibility(bool show)
        {
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(this);

            UnityEngine.Object[] os = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (UnityEngine.Object o in os)
            {
                if (o != this)
                {
                    o.hideFlags = show ? HideFlags.None : HideFlags.HideInHierarchy;
                    EditorUtility.SetDirty(o);
                }
            }
            AssetDatabase.SaveAssets();
#endif
        }
        #endregion
    }

    [Serializable]
    public struct SceneCreationBasePrefabs
    {
        [Header("Base")]
        public GameObject sceneManagerPrefab;
        public GameObject sceneClockPrefab;
        public GameObject sceneObjectPrefab;
        public GameObject sceneSpawnerPrefab;

        [Header("Helpers")]
        public GameObject colliderSceneObjectPrefab;
    }
    
    [Serializable]
    public struct SceneCreationEditorColors
    {
        public Color headerBackground;
        public Color headerForeground;
        public Color selectionBlue;
    }
}
