using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Dhs5.SceneCreation
{
    [CustomPropertyDrawer(typeof(SceneVarTween))]
    public class SceneVarTweenEditor : PropertyDrawer
    {
        SerializedProperty sceneVariablesSO;
        SerializedObject sceneVariablesObj;
        SceneVariablesSO sceneVarContainer;

        private SerializedProperty sceneVarUniqueIDP;

        private SerializedProperty canBeStaticP;
        private SerializedProperty isStaticP;
        
        private SerializedProperty canBeInactiveP;
        private SerializedProperty isActiveP;

        int sceneVarIndex = 0;
        int sceneVarIndexSave = 0;

        float propertyOffset;
        float propertyHeight;

        bool emptyLabel;

        int prev;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            emptyLabel = string.IsNullOrEmpty(label.text);

            sceneVarIndex = 0;

            propertyOffset = 0f;
            propertyHeight = 0f;

            canBeStaticP = property.FindPropertyRelative("canBeStatic");
            isStaticP = property.FindPropertyRelative("isStatic");

            canBeInactiveP = property.FindPropertyRelative("canBeInactive");
            isActiveP = property.FindPropertyRelative("isActive");

            sceneVarUniqueIDP = property.FindPropertyRelative("sceneVarUniqueID");

            EditorGUI.BeginProperty(position, label, property);

            sceneVariablesSO = property.FindPropertyRelative("sceneVariablesSO");
            if (sceneVariablesSO.objectReferenceValue == null)
            {
                EditorGUI.LabelField(position, "SceneVariablesSO is not assigned !");
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

            SceneVarType type = (SceneVarType)property.FindPropertyRelative("type").enumValueIndex;
            List<SceneVar> sceneVarList = property.FindPropertyRelative("anyVar").boolValue ?
                 sceneVarContainer.SceneVars : 
                 sceneVarContainer.GetListByType(type, type == SceneVarType.INT);
            // Clean list of dependency cycles
            if (!property.FindPropertyRelative("anyVar").boolValue)
            {
                int forbiddenUID = property.FindPropertyRelative("forbiddenUID").intValue;
                if (forbiddenUID != -1)
                {
                    sceneVarList = sceneVarContainer.CleanListOfCycleDependencies(sceneVarList, forbiddenUID);
                }
            }

            // Test if list empty
            if (!sceneVarList.IsValid())
            {
                // Label
                Rect labelPosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 0.25f, 115f, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(labelPosition, label);

                if (!canBeStaticP.boolValue)
                {
                    EditorGUI.LabelField(
                        new Rect(position.x + 130f, position.y + EditorGUIUtility.singleLineHeight * 0.25f,
                        position.width - 130f, EditorGUIUtility.singleLineHeight),
                        "No SceneVar available");

                    sceneVarUniqueIDP.intValue = -1;

                    EditorGUI.EndProperty();
                    return;
                }

                // SceneVar choice popup
                Rect popupPosition = new Rect(position.x + (emptyLabel ? 0 : 120f), position.y + EditorGUIUtility.singleLineHeight * 0.25f,
                    position.width - (emptyLabel ? 0f : 120f), EditorGUIUtility.singleLineHeight);
                string typeStr = "";
                switch (type)
                {
                    case SceneVarType.BOOL:
                        typeStr = "bool";
                        break;
                    case SceneVarType.INT:
                        typeStr = "int";
                        break;
                    case SceneVarType.FLOAT:
                        typeStr = "float";
                        break;
                    case SceneVarType.STRING:
                        typeStr = "string";
                        break;
                    default:
                        EditorGUI.EndProperty();
                        return;
                }
                EditorGUI.PropertyField(popupPosition, property.FindPropertyRelative(typeStr + "Value"), new GUIContent(""));
                isStaticP.boolValue = true;

                EditorGUI.EndProperty();
                return;
            }

            if (canBeInactiveP.boolValue)
            {
                float xOffset = 20f;
                Rect isActiveRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 0.25f,
                    xOffset, EditorGUIUtility.singleLineHeight);
                isActiveP.boolValue = EditorGUI.ToggleLeft(isActiveRect, GUIContent.none, isActiveP.boolValue);
                position.x += xOffset;
                position.width -= xOffset;

                if (!isActiveP.boolValue)
                {
                    EditorGUI.LabelField(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 0.25f, 115f, EditorGUIUtility.singleLineHeight)
                        , label);
                    EditorGUI.EndProperty();
                    return;
                }
            }

            sceneVarIndexSave = sceneVarList.GetIndexByUniqueID(sceneVarUniqueIDP.intValue);
            if (sceneVarIndexSave == -1) sceneVarIndexSave = 0;

            propertyOffset += EditorGUIUtility.singleLineHeight * 0.25f;
            propertyHeight += EditorGUIUtility.singleLineHeight * 0.25f;

            if (!canBeStaticP.boolValue)
            {
                // Label
                Rect labelPosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 0.25f, 115f, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(labelPosition, label);

                // SceneVar choice popup
                Rect popupPosition = new Rect(position.x + (emptyLabel ? 0 : 120f), position.y + propertyOffset, position.width - 60f - (emptyLabel ? 0f : 120f), EditorGUIUtility.singleLineHeight);
                sceneVarIndex = EditorGUI.Popup(popupPosition, sceneVarIndexSave, sceneVarList.VarStrings().ToArray());
                if (sceneVarList.GetUniqueIDByIndex(sceneVarIndex) == 0) sceneVarIndex = sceneVarIndexSave;
                sceneVarUniqueIDP.intValue = sceneVarList.GetUniqueIDByIndex(sceneVarIndex);

                prev = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                // Label
                Rect typePosition = new Rect(position.x + (position.width - 55f), position.y + EditorGUIUtility.singleLineHeight * 0.25f, 55f, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(typePosition, sceneVarContainer[sceneVarUniqueIDP.intValue].type.ToString());

                EditorGUI.indentLevel = prev;
            }
            else
            {
                // Label
                Rect labelPosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 0.25f, 115f, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(labelPosition, label);

                // SceneVar choice popup
                Rect popupPosition = new Rect(position.x + (emptyLabel ? 0 : 120f), position.y + propertyOffset, position.width - 120f - (emptyLabel ? 0f : 120f), EditorGUIUtility.singleLineHeight);
                if (!isStaticP.boolValue)
                {
                    sceneVarIndex = EditorGUI.Popup(popupPosition, sceneVarIndexSave, sceneVarList.VarStrings().ToArray());
                    if (sceneVarList.GetUniqueIDByIndex(sceneVarIndex) == 0) sceneVarIndex = sceneVarIndexSave;
                    sceneVarUniqueIDP.intValue = sceneVarList.GetUniqueIDByIndex(sceneVarIndex);
                }
                else
                {
                    string typeStr;
                    switch (type)
                    {
                        case SceneVarType.BOOL:
                            typeStr = "bool";
                            break;
                        case SceneVarType.INT:
                            typeStr = "int";
                            break;
                        case SceneVarType.FLOAT:
                            typeStr = "float";
                            break;
                        case SceneVarType.STRING:
                            typeStr = "string";
                            break;
                        default:
                            EditorGUI.EndProperty();
                            return;
                    }
                    EditorGUI.PropertyField(popupPosition, property.FindPropertyRelative(typeStr + "Value"), new GUIContent(""));
                }

                prev = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                // Label
                Rect typePosition = new Rect(position.x + (position.width - 115f), position.y + EditorGUIUtility.singleLineHeight * 0.25f, 55f, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(typePosition, type.ToString());

                // Static toggle
                Rect staticRect = new Rect(position.x + (position.width - 60f), position.y + EditorGUIUtility.singleLineHeight * 0.25f, 60f, EditorGUIUtility.singleLineHeight);
                isStaticP.boolValue = EditorGUI.ToggleLeft(staticRect, "Static", isStaticP.boolValue);

                EditorGUI.indentLevel = prev;
            }

            // End
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 1.5f;// property.FindPropertyRelative("propertyHeight").floatValue;
        }
    }
}
