using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Dhs5.SceneCreation
{
    [CustomPropertyDrawer(typeof(SceneTimedCondition))]
    public class SceneTimedConditionEditor : PropertyDrawer
    {
        private float propertyOffset;
        
        private SerializedProperty conditionTypeProperty;
        private SerializedProperty timeToWaitProperty;
        private SerializedProperty eventVarProperty;
        private SerializedProperty sceneConditionsProperty;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            propertyOffset = 0;
            
            conditionTypeProperty = property.FindPropertyRelative("conditionType");
            timeToWaitProperty = property.FindPropertyRelative("timeToWait");
            eventVarProperty = property.FindPropertyRelative("eventVar");
            sceneConditionsProperty = property.FindPropertyRelative("sceneConditions");

            EditorGUI.BeginProperty(position, label, property);

            GUI.Box(new Rect(
                position.x - 3f, position.y - 1f, position.width + 3f, GetPropertyHeight(property, label) + 2f),
                GUIContent.none, GUI.skin.window);

            Rect foldoutPosition = new Rect(position.x + 13f, position.y, position.width - 13f, 
                EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutPosition, property.isExpanded, label, true);
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
                switch ((SceneTimedCondition.TimedConditionType)conditionTypeProperty.enumValueIndex)
                {
                    case SceneTimedCondition.TimedConditionType.WAIT_FOR_TIME:
                        EditorGUI.PropertyField(paramPosition, timeToWaitProperty);
                        propertyOffset += EditorGUIUtility.singleLineHeight * 1.5f;
                        break;
                    case SceneTimedCondition.TimedConditionType.WAIT_UNTIL_SCENE_CONDITION:
                        EditorGUI.PropertyField(paramPosition, sceneConditionsProperty);
                        propertyOffset += EditorGUI.GetPropertyHeight(sceneConditionsProperty) + EditorGUIUtility.singleLineHeight * 0.15f;
                        break;
                    case SceneTimedCondition.TimedConditionType.WAIT_WHILE_SCENE_CONDITION:
                        EditorGUI.PropertyField(paramPosition, sceneConditionsProperty);
                        propertyOffset += EditorGUI.GetPropertyHeight(sceneConditionsProperty) + EditorGUIUtility.singleLineHeight * 0.15f;
                        break;
                    case SceneTimedCondition.TimedConditionType.WAIT_FOR_EVENT:
                        EditorGUI.PropertyField(paramPosition, eventVarProperty);
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
                conditionTypeProperty.enumValueIndex == 0 ? EditorGUIUtility.singleLineHeight * 3.8f :
                    EditorGUI.GetPropertyHeight(sceneConditionsProperty) + EditorGUIUtility.singleLineHeight * 2.4f
                    : EditorGUIUtility.singleLineHeight * 1.1f;
        }
    }
}
