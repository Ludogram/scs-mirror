using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Dhs5.SceneCreation
{
    [CustomPropertyDrawer(typeof(SceneLoopCondition))]
    public class SceneLoopConditionEditor : PropertyDrawer
    {
        private float propertyOffset;
        
        private SerializedProperty loopProperty;
        private SerializedProperty conditionTypeProperty;
        private SerializedProperty timeToWaitProperty;
        private SerializedProperty iterationNumberProperty;
        private SerializedProperty sceneConditionsProperty;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            propertyOffset = 0;
            
            loopProperty = property.FindPropertyRelative("loop");
            conditionTypeProperty = property.FindPropertyRelative("conditionType");
            timeToWaitProperty = property.FindPropertyRelative("timeToWait");
            iterationNumberProperty = property.FindPropertyRelative("iterationNumber");
            sceneConditionsProperty = property.FindPropertyRelative("sceneConditions");

            EditorGUI.BeginProperty(position, label, property);

            GUI.Box(new Rect(
                position.x - 3f, position.y - 1f, position.width + 3f, GetPropertyHeight(property, label) + 2f),
                GUIContent.none, GUI.skin.window);

            loopProperty.boolValue = EditorGUI.ToggleLeft(new Rect(
                position.x + 3f, position.y, 15f, EditorGUIUtility.singleLineHeight),
                GUIContent.none, loopProperty.boolValue);

            if (!loopProperty.boolValue)
            {
                EditorGUI.LabelField(new Rect(
                position.x + 35f, position.y, position.width - 35f, EditorGUIUtility.singleLineHeight),
                label);

                property.isExpanded = false;
                EditorGUI.EndProperty();
                return;
            }

            property.isExpanded = EditorGUI.Foldout(new Rect(
                position.x + 35f, position.y, position.width - 35f, EditorGUIUtility.singleLineHeight),
                property.isExpanded, label, true);
            propertyOffset += EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                // Condition type choice
                Rect typePosition = new Rect(position.x, position.y + propertyOffset, position.width - 5f,
                    EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(typePosition, conditionTypeProperty);
                propertyOffset += EditorGUIUtility.singleLineHeight;
                
                // Param
                Rect paramPosition = new Rect(position.x, position.y + propertyOffset, position.width - 5f,
                    EditorGUIUtility.singleLineHeight);
                switch ((SceneLoopCondition.LoopConditionType)conditionTypeProperty.enumValueIndex)
                {
                    case SceneLoopCondition.LoopConditionType.TIMED:
                        EditorGUI.PropertyField(paramPosition, timeToWaitProperty);
                        propertyOffset += EditorGUIUtility.singleLineHeight * 1.5f;
                        break;
                    case SceneLoopCondition.LoopConditionType.SCENE:
                        EditorGUI.PropertyField(paramPosition, sceneConditionsProperty);
                        propertyOffset += EditorGUI.GetPropertyHeight(sceneConditionsProperty) + EditorGUIUtility.singleLineHeight * 0.15f;
                        break;
                    case SceneLoopCondition.LoopConditionType.ITERATION:
                        EditorGUI.PropertyField(paramPosition, iterationNumberProperty);
                        propertyOffset += EditorGUIUtility.singleLineHeight * 1.5f;
                        break;
                }

                EditorGUI.indentLevel--;
            }

            // End
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            conditionTypeProperty = property.FindPropertyRelative("conditionType");
            sceneConditionsProperty = property.FindPropertyRelative("sceneConditions");
            return property.isExpanded ? 
                conditionTypeProperty.enumValueIndex == 0 ? 
                EditorGUI.GetPropertyHeight(sceneConditionsProperty) + EditorGUIUtility.singleLineHeight * 2.4f : 
                    EditorGUIUtility.singleLineHeight * 3.8f
                : EditorGUIUtility.singleLineHeight * 1.1f;
        }
    }
}
