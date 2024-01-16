using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dhs5.SceneCreation
{
    [CustomPropertyDrawer(typeof(BaseSceneListener.Condition))]
    public class ListenerConditionEditor : PropertyDrawer
    {
        SerializedProperty hasConditionProp;
        SerializedProperty layerCheckProp;
        SerializedProperty layerMaskProp;
        SerializedProperty tagCheckProp;
        SerializedProperty tagMaskProp;
        SerializedProperty sceneConditionsProp;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            hasConditionProp = property.FindPropertyRelative("hasCondition");
            layerCheckProp = property.FindPropertyRelative("layerCheck");
            tagCheckProp = property.FindPropertyRelative("tagCheck");
            sceneConditionsProp = property.FindPropertyRelative("sceneConditions");

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.indentLevel++;

            GUI.Box(new Rect(
                position.x, position.y - 1f, position.width, GetPropertyHeight(property, label) + 2f),
                GUIContent.none, GUI.skin.window);

            hasConditionProp.boolValue = EditorGUI.ToggleLeft(new Rect(
                position.x - 10f, position.y, 30f, EditorGUIUtility.singleLineHeight),
                GUIContent.none, hasConditionProp.boolValue);

            if (!hasConditionProp.boolValue)
            {
                EditorGUI.LabelField(new Rect(
                position.x + 30f, position.y, position.width - 30f, EditorGUIUtility.singleLineHeight),
                label);

                property.isExpanded = false;
                EditorGUI.indentLevel--;
                EditorGUI.EndProperty();
                return;
            }

            property.isExpanded = EditorGUI.Foldout(new Rect(
                position.x + 30f, position.y, position.width - 30f, EditorGUIUtility.singleLineHeight),
                property.isExpanded, label, true);

            if (property.isExpanded)
            {
                // Layer
                Rect layerRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, 105f, EditorGUIUtility.singleLineHeight);
                layerCheckProp.boolValue = EditorGUI.ToggleLeft(layerRect, "Layer Check", layerCheckProp.boolValue);

                if (layerCheckProp.boolValue)
                {
                    layerMaskProp = property.FindPropertyRelative("layerMask");
                    layerRect.x += position.width * 0.40f;
                    layerRect.width = position.width * 0.6f - 5f;
                    EditorGUI.PropertyField(layerRect, layerMaskProp, GUIContent.none);
                }

                // Tag
                Rect tagRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 2.2f, 95f, EditorGUIUtility.singleLineHeight);
                tagCheckProp.boolValue = EditorGUI.ToggleLeft(tagRect, "Tag Check", tagCheckProp.boolValue);

                if (tagCheckProp.boolValue)
                {
                    tagMaskProp = property.FindPropertyRelative("tagMask");
                    tagRect.x += position.width * 0.40f;
                    tagRect.width = position.width * 0.6f - 5f;
                    EditorGUI.PropertyField(tagRect, tagMaskProp, GUIContent.none);
                }

                // Scene Conditions
                Rect condRect = new Rect(position.x + 14f, position.y + EditorGUIUtility.singleLineHeight * 3.4f, position.width - 22f, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(condRect, sceneConditionsProp);
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.isExpanded ?
                EditorGUIUtility.singleLineHeight * 3.7f
                + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("sceneConditions")) :
                EditorGUIUtility.singleLineHeight * 1.1f;
        }
    }
}
