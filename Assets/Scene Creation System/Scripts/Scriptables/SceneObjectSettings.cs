using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dhs5.SceneCreation
{
    //[CreateAssetMenu(fileName = "SceneObject Settings", menuName = "Scene Creation/Settings/SceneObject Settings")]
    public class SceneObjectSettings : ScriptableObject
    {
        public static SceneObjectSettings Instance
        {
            get
            {
#if UNITY_EDITOR
                return SceneCreationSettings.instance.SceneObjectSettings;
#else
                if (Application.isPlaying)
                {
                    return SceneManager.Settings;
                }

                return null;
#endif
            }
        }

        [Header("Databases")]
        [SerializeField][ReadOnly] private SceneObjectTagDatabase _tagDatabase;
        internal SceneObjectTagDatabase TagDatabase => _tagDatabase;

        [SerializeField][ReadOnly] private SceneObjectLayerDatabase _layerDatabase;
        internal SceneObjectLayerDatabase LayerDatabase => _layerDatabase;

        [Header("Scene Log")]
        [SerializeField] private Color _eventColor;

        [Header("Debug")]
        [SerializeField][Range(0, 5)] private int _debugLevel;
        internal int DebugLevel => _debugLevel;


        internal Color LevelToColor(int level)
        {
            switch (level)
            {
                case 0: return _level0Color;
                case 1: return _level1Color;
                case 2: return _level2Color;
                case 3: return _level3Color;
                case 4: return _level4Color;
                case 5: return _level5Color;
            }
            return Color.white;
        }
        [SerializeField] private Color _level0Color; internal Color Level0Color => _level0Color;
        [SerializeField] private Color _level1Color; internal Color Level1Color => _level1Color;
        [SerializeField] private Color _level2Color; internal Color Level2Color => _level2Color;
        [SerializeField] private Color _level3Color; internal Color Level3Color => _level3Color;
        [SerializeField] private Color _level4Color; internal Color Level4Color => _level4Color;
        [SerializeField] private Color _level5Color; internal Color Level5Color => _level5Color;

        #region Editor

        readonly string TagDatabaseName = "SceneObject Tag Database";
        readonly string LayerDatabaseName = "SceneObject Layer Database";

        internal void SetupProject()
        {
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(this);

            UnityEngine.Object[] os = AssetDatabase.LoadAllAssetsAtPath(path);
            List<UnityEngine.Object> objects = os.ToList();

            foreach (UnityEngine.Object o in os)
            {
                Debug.Log(o);
            }

            _tagDatabase = SetupObject<SceneObjectTagDatabase>(_tagDatabase, TagDatabaseName);

            _layerDatabase = SetupObject<SceneObjectLayerDatabase>(_layerDatabase, LayerDatabaseName);

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

        #endregion
    }
}
