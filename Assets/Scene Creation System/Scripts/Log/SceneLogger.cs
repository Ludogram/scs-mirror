using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace Dhs5.SceneCreation
{
    public static class SceneLogger
    {
        private static Stack<string> logStack = new();

        private static bool detailed;
        private static bool showEmpty;
        private static bool inFile;
        public static string GetSceneLog(GameObject go, bool _detailed = false, bool _showEmpty = false, bool _inFile = false)
        {
            detailed = _detailed;
            showEmpty = _showEmpty;
            inFile = _inFile;

            StringBuilder sb = new StringBuilder();
            logStack.Clear();

            GameObject[] roots = go.scene.GetRootGameObjects();

            for (int i = roots.Length - 1; i >= 0; i--)
            {
                if (AppendGO(roots[i], 0, RootColor + "ROOT:"))
                    Back();
            }
            Append(detailed ? "Detailed Scene Log : \n" : "Simple Scene Log : \n");

            return UnpackStack(sb);
        }

        #region Helpers
        private static string UnpackStack(StringBuilder sb)
        {
            foreach (var s in logStack)
            {
                sb.Append(s);
            }
            return sb.ToString();
        }

        private static void Append(string str)
        {
            logStack.Push(str);
        }
        private static void Appends(params string[] str)
        {
            for (int i = str.Length - 1; i >= 0; i--)
            {
                Append(str[i]);
            }
        }
        private static void AppendSO(SceneObject so, int rank)
        {
            if (so == null || (!showEmpty && so.IsEmpty())) return;

            List<string> lines = so.LogLines(detailed, showEmpty);

            if (lines == null || lines.Count == 0)
            {
                Debug.LogError("SceneObject lines null or empty");
                return;
            }
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                Append(lines[i]);
                BlankAlinea(rank);
            }

            Appends(" is ", SceneObjectColor, "SO:", ColorEnd, "\n");
        }

        private static void Alinea(int number = 1)
        {
            for (int i = 0; i < number; i++)
                Append("_____");
        }
        private static void BlankAlinea(int number = 1)
        {
            for (int i = 0; i < number; i++)
                Append("        ");
        }
        private static void Back(int number = 1)
        {
            for (int i = 0; i < number; i++)
                Append("\n");
        }
        private static void Prefix(string prefix)
        {
            Append(ColorEnd);
            Append(prefix);
        }

        private static bool AppendGO(GameObject go, int rank, string prefix = null, bool forceLog = false)
        {
            void Name()
            {
                Append(go.name);
            }

            bool result = go.TryGetComponent(out SceneObject so);
            bool childResult = false;
            bool hasPrefix = prefix != null;

            int childCount = go.transform.childCount;
            if (childCount == 0)
            {
                if (result)
                {
                    Back();
                    AppendSO(so, rank);
                    Name();
                    if (hasPrefix) Prefix(prefix);
                    Alinea(rank);
                }
                return result;
            }
            else
            {
                for (int i = childCount - 1; i >= 0; i--)
                {
                    if (AppendGO(go.transform.GetChild(i).gameObject, rank + 1))
                    {
                        childResult = true;
                    }
                }
            }

            if (result || childResult || forceLog)
            {
                Back();
            }
            if (result)
            {
                AppendSO(so, rank);
            }
            if (result || childResult || forceLog)
            {
                Name();
                if (hasPrefix) Prefix(prefix);
                Alinea(rank);
                return true;
            }
            return false;
        }
        #endregion

        #region Color Utility
        private const string rootColor = "<color=#7E3600ff>";
        private const string sceneObjectColor = "<color=#0000ffff>";
        private const string listenerColor = "<color=#00ff00ff>";
        private const string eventColor = "<color=#ff0000ff>";
        private const string extensionEventColor = "<color=#ff3b7dff>";
        private const string tweenColor = "<color=#ffad23ff>";
        private const string timelineColor = "<color=#ad00ffff>";
        private const string colorEnd = "</color>";

        private const string bold = "<b>";
        private const string boldEnd = "</b>";

        public static string RootColor => Color(rootColor);
        public static string SceneObjectColor => Color(sceneObjectColor);
        public static string ListenerColor => Color(listenerColor);
        public static string EventColor => Color(eventColor);
        public static string ExtensionEventColor => Color(extensionEventColor);
        public static string TweenColor => Color(tweenColor);
        public static string TimelineColor => Color(timelineColor);
        public static string ColorEnd => Color(colorEnd);

        public static string Bold => Color(bold);
        public static string BoldEnd => Color(boldEnd);

        private static string Color(string color)
        {
            if (inFile) return null;
            return color;
        }
        #endregion
    }
}
