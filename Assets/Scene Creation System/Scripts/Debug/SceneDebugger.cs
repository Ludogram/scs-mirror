using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    public static class SceneDebugger
    {
        private static SceneObjectSettings _settings;
        private static SceneObjectSettings Settings
        {
            get
            {
                if (_settings == null) _settings = SceneManager.Settings;
                return _settings;
            }
        }


        #region Color

        private static string _level0Color;
        private static string _level1Color;
        private static string _level2Color;
        private static string _level3Color;
        private static string _level4Color;
        private static string _level5Color;

        private static string LevelToColorString(int level)
        {
            switch (level)
            {
                case 0:
                    {
                        if (_level0Color == null) _level0Color = ColorUtility.ToHtmlStringRGBA(Settings.LevelToColor(0));
                        return _level0Color;
                    }
                case 1:
                    {
                        if (_level1Color == null) _level1Color = ColorUtility.ToHtmlStringRGBA(Settings.LevelToColor(1));
                        return _level1Color;
                    }
                case 2:
                    {
                        if (_level2Color == null) _level2Color = ColorUtility.ToHtmlStringRGBA(Settings.LevelToColor(2));
                        return _level2Color;
                    }
                case 3:
                    {
                        if (_level3Color == null) _level3Color = ColorUtility.ToHtmlStringRGBA(Settings.LevelToColor(3));
                        return _level3Color;
                    }
                case 4:
                    {
                        if (_level4Color == null) _level4Color = ColorUtility.ToHtmlStringRGBA(Settings.LevelToColor(4));
                        return _level4Color;
                    }
                case 5:
                    {
                        if (_level5Color == null) _level5Color = ColorUtility.ToHtmlStringRGBA(Settings.LevelToColor(5));
                        return _level5Color;
                    }
            }
            return ColorUtility.ToHtmlStringRGBA(Color.white);
        }

        #endregion


        public static void Log(string _message, BaseSceneObject _object, int level = 0)
        {
            if (Settings.DebugLevel >= level)
                Debug.Log($"<color=#{LevelToColorString(level)}>{_message}</color>", _object);
        }
        public static void Log(object _message, BaseSceneObject _object, int level = 0)
        {
            if (Settings.DebugLevel >= level)
                Debug.Log($"<color=#{LevelToColorString(level)}>{_message}</color>", _object);
        }
    }
}
