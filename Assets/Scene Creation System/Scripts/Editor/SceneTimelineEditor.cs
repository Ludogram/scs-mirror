using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Dhs5.SceneCreation
{
    [CustomPropertyDrawer(typeof(SceneTimeline))]
    public class SceneTimelineEditor : PropertyDrawer
    {
        private float propertyOffset;
        
        private SerializedProperty idProperty;
        private SerializedProperty conditionProperty;
        private SerializedProperty timelineObjectsProperty;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            propertyOffset = 0;
            
            idProperty = property.FindPropertyRelative("ID");
            conditionProperty = property.FindPropertyRelative("endLoopCondition");
            timelineObjectsProperty = property.FindPropertyRelative("steps");

            EditorGUI.BeginProperty(position, label, property);

            Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutPosition, property.isExpanded, property.isExpanded ? "" : idProperty.stringValue);
            propertyOffset += EditorGUIUtility.singleLineHeight * 0.25f;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                Rect idPosition = new Rect(position.x, position.y + propertyOffset, position.width - 15, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(idPosition, idProperty);
                propertyOffset += EditorGUIUtility.singleLineHeight * 1.2f;

                Rect conditionPosition = new Rect(position.x, position.y + propertyOffset, position.width,
                    EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(conditionPosition, conditionProperty, new GUIContent("Loop End-condition"));
                propertyOffset += EditorGUI.GetPropertyHeight(conditionProperty);

                Rect timelineObjPosition = new Rect(position.x, position.y + propertyOffset, position.width,
                    EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(timelineObjPosition, timelineObjectsProperty);
                propertyOffset += EditorGUI.GetPropertyHeight(timelineObjectsProperty);

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            idProperty = property.FindPropertyRelative("ID");
            conditionProperty = property.FindPropertyRelative("endLoopCondition");
            timelineObjectsProperty = property.FindPropertyRelative("steps");

            return property.isExpanded ? 
                EditorGUIUtility.singleLineHeight * 2.65f + EditorGUI.GetPropertyHeight(timelineObjectsProperty)
                    + EditorGUI.GetPropertyHeight(conditionProperty)
                    : EditorGUIUtility.singleLineHeight * 1.3f;
        }
    }
}
