using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    [CustomEditor(typeof(BaseSceneObject), true)]
    [CanEditMultipleObjects]
    public class BaseSceneObjectEditor : Editor
    {
        protected BaseSceneObject baseSceneObject;

        protected GUIContent empty = new GUIContent("");

        protected string[] pageNames = new string[] { "Default", "Scene Vars", "Dependencies" };

        Color backgroundColor;
        Color foregroundColor;

        bool isManager = false;


        protected virtual void OnEnable()
        {
            baseSceneObject = target as BaseSceneObject;
            if (baseSceneObject is SceneManager) isManager = true;

            pageNames[0] = baseSceneObject.DisplayName;

            baseSceneObject.OnEditorEnable();
            
            CreateDependenciesList(new());
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            int header = Header();

            EditorGUILayout.Space(10f);

            //EditorGUILayout.LabelField(baseSceneObject.DisplayName, headerStyle);
            EditorGUI.DropShadowLabel(EditorGUILayout.GetControlRect(false, 20f),
                header == 0 ? baseSceneObject.DisplayName : pageNames[header]);

            EditorGUILayout.Space(5f);

            switch (header)
            {
                case 0:
                    DrawDefault();
                    break;
                case 1:
                    DrawSceneVars();
                    break;
                case 2:
                    DrawDependencies();
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawDefault()
        {
            DrawPropertiesExcluding(serializedObject, "m_Script");
        }
        protected virtual void DrawSceneVars()
        {
            Editor editor = Editor.CreateEditor(baseSceneObject.SceneVariablesSO);
            editor.OnInspectorGUI();
        }

        ReorderableList dependenciesList;
        protected virtual void DrawDependencies()
        {
            Rect r = EditorGUILayout.GetControlRect(false, 0f);

            dependenciesList.DoLayoutList();

            if (GUI.Button(new Rect(
                r.x + r.width -30f, r.y + 2f, 30f, 30f),
                EditorGUIUtility.IconContent("d_RotateTool On")))
            {
                CreateDependenciesList(baseSceneObject.GetDisplayDependencies());
            }
        }
        private void CreateDependenciesList(List<string> dependencies)
        {
            dependenciesList = new ReorderableList(dependencies, typeof(string), false, true, false, false)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "Dependencies");
                },

                drawElementCallback = (rect, index, active, focused) =>
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.LabelField(rect, dependencies[index]);
                    EditorGUI.indentLevel--;
                },

                elementHeight = EditorGUIUtility.singleLineHeight * 1.1f,
            };
        }

        protected int Header()
        {
            return Header(pageNames);
        }
        protected int Header(string[] menuNames)
        {
            backgroundColor = SceneCreationSettings.instance.EditorColors.headerBackground;//new Color32(191, 247, 255, 255);
            foregroundColor = SceneCreationSettings.instance.EditorColors.headerForeground;//new Color(1f, 0.49f, 0f);

            GUIStyle headerStyle = new()
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            headerStyle.normal.textColor = Color.white;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), empty);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(2f);

            if (!isManager && baseSceneObject.SceneVariablesSO == null)
            {
                EditorGUILayout.HelpBox("You need to setup the SceneManager and its SceneVariablesSO", MessageType.Error);
                return 0;
            }
            else
            {
                Rect lineRect = EditorGUILayout.GetControlRect(false, 2f);
                lineRect.x -= 18f;
                lineRect.width += 25f;
                EditorGUI.DrawRect(lineRect, foregroundColor);
                lineRect.y += 76f;
                EditorGUI.DrawRect(lineRect, foregroundColor);

                Rect backgroundRect = EditorGUILayout.GetControlRect(false, 1f);
                backgroundRect.height = 72f;
                backgroundRect.width += 25f;
                backgroundRect.x -= 18f;
                backgroundRect.y -= 1f;
                EditorGUI.DrawRect(backgroundRect, backgroundColor);

                Rect currentRect = EditorGUILayout.GetControlRect(false);

                if (GUI.Button(new Rect
                    (currentRect.x + (currentRect.width - 55f), currentRect.y, 26f, currentRect.height),
                    EditorGUIUtility.IconContent("d_ToolSettings")))
                {
                    EditorHelper.GetSceneObjectSettings();
                }
                if (GUI.Button(new Rect
                    (currentRect.x + (currentRect.width - 26f), currentRect.y, 26f, currentRect.height),
                    EditorGUIUtility.IconContent("d__Popup")))
                {
                    EditorHelper.GetSceneCreationSettings();
                }

                if (!isManager) GUI.contentColor = foregroundColor;

                EditorGUI.BeginDisabledGroup(!isManager);
                EditorGUI.PropertyField(new Rect
                    (currentRect.x, currentRect.y, currentRect.width - 58f, currentRect.height),
                    serializedObject.FindProperty("sceneVariablesSO"), empty);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space(1f);

                if (isManager) GUI.contentColor = foregroundColor;

                currentRect = EditorGUILayout.GetControlRect(false, 20f);

                // Tag Text
                EditorGUI.LabelField(new Rect
                    (currentRect.x, currentRect.y, 30f, currentRect.height)
                    , new GUIContent("Tag", "Scene Object Tag of this object"), headerStyle);
                // Layer Text
                EditorGUI.LabelField(new Rect
                    (currentRect.x + currentRect.width * 0.52f, currentRect.y, 35f, currentRect.height)
                    , new GUIContent("Layer", "Scene Object Layer of this object"), headerStyle);
                GUI.contentColor = Color.white;
                // Tag
                EditorGUI.PropertyField(new Rect
                    (currentRect.x + 30f, currentRect.y, currentRect.width * 0.5f - 58f, currentRect.height)
                    , serializedObject.FindProperty("sceneObjectTag"), empty);
                // Layer
                EditorGUI.PropertyField(new Rect
                    (currentRect.x + currentRect.width * 0.52f + 39f, currentRect.y, currentRect.width * 0.48f - 39f, currentRect.height)
                    , serializedObject.FindProperty("sceneObjectLayer"), empty);

                // Tag Button
                if (GUI.Button(new Rect
                    (currentRect.x + currentRect.width * 0.5f - 26f, currentRect.y, 26f, currentRect.height * 0.9f),
                    EditorGUIUtility.IconContent("d_FilterByLabel")))
                {
                    EditorHelper.GetSceneObjectTagDatabase();
                }

                GUILayout.Space(2f);
                SerializedProperty pageProp = serializedObject.FindProperty("currentPage");
                pageProp.intValue = GUILayout.Toolbar(pageProp.intValue, menuNames);

                return baseSceneObject.SceneVariablesSO == null ? 0 : pageProp.intValue;
            }
        }
    }
}
