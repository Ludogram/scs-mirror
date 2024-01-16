using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    [CustomPropertyDrawer(typeof(BaseSceneEvent), true)]
    public class BaseSceneEventEditor : PropertyDrawer
    {
        protected SerializedProperty idProperty;
        protected SerializedProperty detailsProperty;
        protected SerializedProperty pageProperty;
        protected SerializedProperty conditionsProperty;
        protected SerializedProperty actionsProperty;
        protected SerializedProperty paramedEventProperty;
        protected SerializedProperty uEventProperty;

        protected string[] pageNames = new string[] { "Condition", "Scene Action", "UnityEvent", "Parametered Event" };

        protected float height;

        Color selectionBlue = new Color(0.75f, 0.75f, 1f);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            idProperty = property.FindPropertyRelative("eventID");
            detailsProperty = property.FindPropertyRelative("details");
            pageProperty = property.FindPropertyRelative("page");
            conditionsProperty = property.FindPropertyRelative("sceneConditions");
            actionsProperty = property.FindPropertyRelative("sceneActions");
            paramedEventProperty = property.FindPropertyRelative("sceneParameteredEvents");
            uEventProperty = property.FindPropertyRelative("unityEvent");

            EditorGUI.BeginProperty(position, label, property);

            Rect r = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            property.isExpanded = EditorGUI.Foldout(r, property.isExpanded, property.isExpanded ? "" : idProperty.stringValue);
            r.y += EditorGUIUtility.singleLineHeight * 0.25f;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                EditorGUI.PropertyField(r, idProperty);
                r.y += EditorGUI.GetPropertyHeight(idProperty);
                
                EditorGUI.PropertyField(r, detailsProperty, true);
                r.y += EditorGUI.GetPropertyHeight(detailsProperty);

                r.y += EditorGUIUtility.singleLineHeight * 0.5f;
                r.height = EditorGUIUtility.singleLineHeight * 2.2f;
                pageProperty.intValue = GUI.SelectionGrid(r, pageProperty.intValue, pageNames, 2);

                r.y += EditorGUIUtility.singleLineHeight * 2.5f;
                r.height = EditorGUIUtility.singleLineHeight;

                height = r.y - position.y;

                DrawCurrentPage(r, property, pageProperty.intValue);

                EditorGUI.indentLevel--;
            }

            height += EditorGUIUtility.singleLineHeight * 0.5f;
            property.FindPropertyRelative("propertyHeight").floatValue = height;
            EditorGUI.EndProperty();
        }

        float minHeight = EditorGUIUtility.singleLineHeight * 5f;
        private void DrawCurrentPage(Rect r, SerializedProperty property, int pageIndex)
        {
            switch (pageIndex)
            {
                case 0:
                    {
                        EditorGUI.PropertyField(r, conditionsProperty, true);
                        height += EditorGUI.GetPropertyHeight(conditionsProperty);
                        //sceneConditionsList.DoList(r);
                        //height += Mathf.Max(sceneConditionsList.GetHeight(), minHeight);
                        break;
                    }
                case 1:
                    {
                        EditorGUI.PropertyField(r, actionsProperty, true);
                        height += EditorGUI.GetPropertyHeight(actionsProperty);
                        //sceneActionsList.DoList(r);
                        //height += Mathf.Max(sceneActionsList.GetHeight(), minHeight);
                        break;
                    }
                case 2:
                    {
                        EditorGUI.PropertyField(r, uEventProperty, true);
                        height += EditorGUI.GetPropertyHeight(uEventProperty);
                        break;
                    }
                case 3:
                    {
                        EditorGUI.PropertyField(r, paramedEventProperty, true);
                        height += EditorGUI.GetPropertyHeight(paramedEventProperty);
                        //sceneParamedList.DoList(r);
                        //height += Mathf.Max(sceneParamedList.GetHeight(), minHeight);
                        break;
                    }
            }
        }

        private ReorderableList CreateSceneParameteredEventsList(SerializedProperty property)
        {
            return new ReorderableList(property.serializedObject, paramedEventProperty, true, true, true, true)
            {
                drawElementBackgroundCallback = (rect, index, active, focused) =>
                {
                    if (active)
                    {
                        if (focused) GUI.backgroundColor = selectionBlue;
                        GUI.Box(rect, GUIContent.none, GUI.skin.button);
                        if (focused) GUI.backgroundColor = Color.white;
                    }
                },
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "Scene Parametered Events");
                },
                drawElementCallback = (rect, index, active, focused) =>
                {
                    EditorGUI.PropertyField(rect, paramedEventProperty.GetArrayElementAtIndex(index), true);
                },
                elementHeightCallback = index =>
                {
                    return EditorGUI.GetPropertyHeight(paramedEventProperty.GetArrayElementAtIndex(index));
                },
                elementHeight = EditorGUIUtility.singleLineHeight * 2.6f
            };
        }
        private ReorderableList CreateSceneActionsList(SerializedProperty property)
        {
            return new ReorderableList(property.serializedObject, actionsProperty, true, true, true, true)
            {
                drawElementBackgroundCallback = (rect, index, active, focused) =>
                {
                    if (active)
                    {
                        if (focused) GUI.backgroundColor = selectionBlue;
                        GUI.Box(rect, GUIContent.none, GUI.skin.button);
                        if (focused) GUI.backgroundColor = Color.white;
                    }
                },
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "Scene Actions");
                },
                drawElementCallback = (rect, index, active, focused) =>
                {
                    EditorGUI.PropertyField(rect, actionsProperty.GetArrayElementAtIndex(index), true);
                },
                elementHeightCallback = index =>
                {
                    return EditorGUI.GetPropertyHeight(actionsProperty.GetArrayElementAtIndex(index));
                },
                elementHeight = EditorGUIUtility.singleLineHeight * 2.6f
            };
        }
        private ReorderableList CreateSceneConditionsList(SerializedProperty property)
        {
            return new ReorderableList(property.serializedObject, conditionsProperty, true, true, true, true)
            {
                drawElementBackgroundCallback = (rect, index, active, focused) =>
                {
                    if (active)
                    {
                        if (focused) GUI.backgroundColor = selectionBlue;
                        GUI.Box(rect, GUIContent.none, GUI.skin.button);
                        if (focused) GUI.backgroundColor = Color.white;
                    }
                },
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "Scene Conditions");
                },
                drawElementCallback = (rect, index, active, focused) =>
                {
                    EditorGUI.PropertyField(rect, conditionsProperty.GetArrayElementAtIndex(index), true);
                },
                elementHeightCallback = index =>
                {
                    return EditorGUI.GetPropertyHeight(conditionsProperty.GetArrayElementAtIndex(index));
                },
                elementHeight = EditorGUIUtility.singleLineHeight * 2.6f
            };
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.isExpanded ? property.FindPropertyRelative("propertyHeight").floatValue 
                : EditorGUI.GetPropertyHeight(property, true);
        }
    }
}
