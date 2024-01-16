using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Dhs5.SceneCreation
{
    [CustomPropertyDrawer(typeof(TimelineObject))]
    public class TimelineObjectEditor : PropertyDrawer
    {
        private float propertyOffset;
        
        private SerializedProperty idProperty;
        private SerializedProperty startConditionProperty;
        private SerializedProperty endConditionProperty;
        private SerializedProperty eventsProperty;
        private SerializedProperty propertyHeight;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            propertyOffset = 0;
            
            idProperty = property.FindPropertyRelative("ID");
            startConditionProperty = property.FindPropertyRelative("startCondition");
            endConditionProperty = property.FindPropertyRelative("endLoopCondition");
            eventsProperty = property.FindPropertyRelative("sceneEvents");
            propertyHeight = property.FindPropertyRelative("propertyHeight");

            EditorGUI.BeginProperty(position, label, property);

            //Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            //property.isExpanded = EditorGUI.Foldout(foldoutPosition, property.isExpanded, label.text.Replace("Element", "Step"));
            //propertyOffset += EditorGUIUtility.singleLineHeight;
            //if (property.isExpanded)

            EditorGUI.PropertyField(
                new Rect(position.x, position.y + propertyOffset, position.width, EditorGUIUtility.singleLineHeight)
                , idProperty);
            propertyOffset += EditorGUI.GetPropertyHeight(idProperty) + 5f;

            Rect startConditionPosition = new Rect(position.x, position.y + propertyOffset, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(startConditionPosition, startConditionProperty, new GUIContent("Trigger condition"));
            propertyOffset += EditorGUI.GetPropertyHeight(startConditionProperty) + 5f;

            Rect conditionPosition = new Rect(position.x, position.y + propertyOffset, position.width,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(conditionPosition, endConditionProperty, new GUIContent("Loop End-condition"));
            propertyOffset += EditorGUI.GetPropertyHeight(endConditionProperty) + 5f;

            Rect sceneEventsPosition = new Rect(position.x, position.y + propertyOffset, position.width - 2f,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(sceneEventsPosition, eventsProperty);
            propertyOffset += EditorGUI.GetPropertyHeight(eventsProperty);

            propertyHeight.floatValue = propertyOffset;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.FindPropertyRelative("propertyHeight").floatValue;
        }
    }
}
