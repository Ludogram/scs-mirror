using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Dhs5.SceneCreation
{
    [CustomPropertyDrawer(typeof(SceneSpecificListener))]
    public class SceneSpecificListenerEditor : PropertyDrawer
    {
        private float propertyHeight;
        private float propertyOffset;
        
        SerializedProperty sceneVariablesSO;
        SerializedObject sceneVariablesObj;
        SceneVariablesSO sceneVarContainer;

        private SerializedProperty hasConditionP;
        private SerializedProperty conditionP;
        private SerializedProperty sceneVarUniqueIDP;

        int sceneVarIndex = 0;
        SceneVar sceneVar;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            propertyHeight = 0;
            sceneVarIndex = 0;
            
            EditorGUI.BeginProperty(position, label, property);

            sceneVariablesSO = property.FindPropertyRelative("sceneVariablesSO");
            if (sceneVariablesSO.objectReferenceValue == null)
            {
                EditorGUI.LabelField(position, "SceneVariablesSO is missing !");
                EditorGUI.EndProperty();
                return;
            }
            // Get the SceneVariablesSO
            sceneVariablesObj = new SerializedObject(sceneVariablesSO.objectReferenceValue);
            sceneVarContainer = sceneVariablesObj.targetObject as SceneVariablesSO;
            if (sceneVarContainer == null)
            {
                EditorGUI.LabelField(position, "SceneVariablesSO is null !");
                EditorGUI.EndProperty();
                return;
            }

            List<SceneVar> sceneVarList = sceneVarContainer.Listenables;
            // Test if list empty
            if (sceneVarList == null || sceneVarList.Count == 0)
            {
                EditorGUI.LabelField(position, "No SceneVar listenable !");
                EditorGUI.EndProperty();
                property.FindPropertyRelative("propertyHeight").floatValue = EditorGUIUtility.singleLineHeight;
                return;
            }

            sceneVarUniqueIDP = property.FindPropertyRelative("varUniqueID");
            int sceneVarIndexSave = sceneVarList.GetIndexByUniqueID(sceneVarUniqueIDP.intValue);
            if (sceneVarIndexSave == -1) sceneVarIndexSave = 0;
            sceneVar = sceneVarList[sceneVarIndexSave];

            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, property.isExpanded ? "" : sceneVar.ID + " : " + sceneVar.type);
            if (property.isExpanded)
            {
                // SceneVar choice popup
                Rect popupPosition = new Rect(position.x + 15, position.y, position.width * 0.6f - 15, EditorGUIUtility.singleLineHeight);
                sceneVarIndex = EditorGUI.Popup(popupPosition, sceneVarIndexSave, sceneVarList.VarStrings().ToArray());
                if (sceneVarList.GetUniqueIDByIndex(sceneVarIndex) == 0) sceneVarIndex = sceneVarIndexSave;
                sceneVarUniqueIDP.intValue = sceneVarList.GetUniqueIDByIndex(sceneVarIndex);

                // Type label
                Rect typePosition = new Rect(position.x + position.width * 0.65f, position.y, position.width * 0.3f, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(typePosition, sceneVar.type.ToString());
                propertyOffset = EditorGUIUtility.singleLineHeight * 1.2f;
                propertyHeight += EditorGUIUtility.singleLineHeight * 1.2f;

                // Condition creation
                conditionP = property.FindPropertyRelative("condition");
                Rect compPosition = new Rect(position.x, position.y + propertyOffset, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(compPosition, conditionP, true);
                propertyOffset += EditorGUI.GetPropertyHeight(conditionP) + 3f;
                propertyHeight += EditorGUI.GetPropertyHeight(conditionP) + 3f;

                propertyHeight += EditorGUIUtility.singleLineHeight * 0.3f;
                propertyOffset += EditorGUIUtility.singleLineHeight * 0.3f;
                // Debug toggle
                Rect debugTogglePosition = new(position.x, position.y + propertyOffset, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(debugTogglePosition, property.FindPropertyRelative("debug"));
                propertyHeight += EditorGUIUtility.singleLineHeight * 0.85f;
            }
            // End
            EditorGUI.EndProperty();

            property.FindPropertyRelative("propertyHeight").floatValue = propertyHeight;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.isExpanded ? property.FindPropertyRelative("propertyHeight").floatValue : EditorGUIUtility.singleLineHeight;
        }
    }
}
