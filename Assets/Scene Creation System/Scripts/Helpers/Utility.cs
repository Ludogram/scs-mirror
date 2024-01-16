using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dhs5.SceneCreation
{
    public class Utility : MonoBehaviour
    {
        #region Editor

#if UNITY_EDITOR
        public static readonly string SceneCreationSettingsPath 
            = "/Settings/Editor/SceneCreationSettings.asset";
        public static readonly string SceneCreationSettingsManualSavePath 
            = "/Settings/Editor/SceneCreationSettingsManualSave.txt";
        public static void ManualSaveSceneCreationSettings()
        {
            string settingsContent = File.ReadAllText(Application.dataPath + SceneCreationSettingsPath);
            File.WriteAllText(Application.dataPath + SceneCreationSettingsManualSavePath, settingsContent);
            AssetDatabase.Refresh();
        }
        public static void LoadFromManualSaveSceneCreationSettings()
        {
            string settingsContent = File.ReadAllText(Application.dataPath + SceneCreationSettingsManualSavePath);
            File.WriteAllText(Application.dataPath + SceneCreationSettingsPath, settingsContent);
            AssetDatabase.Refresh();
        }
#endif

        #endregion
    }
}
