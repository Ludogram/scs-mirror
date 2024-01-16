using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Dhs5.SceneCreation
{
    [CustomPropertyDrawer(typeof(SceneAction))]
    public class SceneActionEditor : PropertyDrawer
    {        
        SerializedProperty sceneVariablesSO;
        SerializedObject sceneVariablesObj;
        SceneVariablesSO sceneVarContainer;

        private SerializedProperty sceneVarUniqueID1P;
        //private SerializedProperty sceneVarUniqueID2P;

        int sceneVarIndex1 = 0;
        //int sceneVarIndex2 = 0;

        GUIContent empty = new GUIContent("");
        GUIStyle style;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool secondParam = true;
            string operationDescription = "";

            style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            sceneVarIndex1 = 0;
            //sceneVarIndex2 = 0;
            
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

            // SceneVar 1
            List<SceneVar> sceneVarList1 = sceneVarContainer.Modifyables;
            // Test if list empty
            if (sceneVarList1 == null || sceneVarList1.Count == 0)
            {
                EditorGUI.LabelField(position, "No SceneVar usuable !");
                EditorGUI.EndProperty();
                return;
            }

            sceneVarUniqueID1P = property.FindPropertyRelative("var1UniqueID");
            int sceneVarIndexSave1 = sceneVarList1.GetIndexByUniqueID(sceneVarUniqueID1P.intValue);
            if (sceneVarIndexSave1 == -1) sceneVarIndexSave1 = 0;
            // SceneVar1 choice popup
            //Rect popup1Position = new Rect(position.x, position.y, position.width * 0.35f, EditorGUIUtility.singleLineHeight);
            Rect popup1Position = new Rect(position.x, position.y + 3f, position.width - 120f, EditorGUIUtility.singleLineHeight);
            sceneVarIndex1 = EditorGUI.Popup(popup1Position, sceneVarIndexSave1, sceneVarList1.VarStrings().ToArray());
            if (sceneVarList1.GetUniqueIDByIndex(sceneVarIndex1) == 0) sceneVarIndex1 = sceneVarIndexSave1;
            sceneVarUniqueID1P.intValue = sceneVarList1.GetUniqueIDByIndex(sceneVarIndex1);

            // Operation creation
            //Rect opPosition = new Rect(position.x + position.width * 0.36f, position.y, position.width * 0.28f, EditorGUIUtility.singleLineHeight);
            Rect opPosition = new Rect(position.x + (position.width - 115f), position.y + 3f, 115f, EditorGUIUtility.singleLineHeight);
            SceneVarType type = sceneVarContainer[sceneVarUniqueID1P.intValue].type;
            property.FindPropertyRelative("var2Type").enumValueIndex = (int)type;
            switch (type)
            {
                case SceneVarType.BOOL:
                    SerializedProperty boolOpProperty = property.FindPropertyRelative("boolOP");
                    EditorGUI.PropertyField(opPosition, boolOpProperty, GUIContent.none);
                    BoolOperation boolOp = (BoolOperation)boolOpProperty.enumValueIndex;
                    operationDescription = boolOp.Description();
                    secondParam = boolOp.HasSecondParameter();
                    break;
                case SceneVarType.INT:
                    SerializedProperty intOpProperty = property.FindPropertyRelative("intOP");
                    EditorGUI.PropertyField(opPosition, intOpProperty, GUIContent.none);
                    IntOperation intOp = (IntOperation)intOpProperty.enumValueIndex;
                    secondParam = intOp.HasSecondParameter();
                    operationDescription = intOp.Description();
                    break;
                case SceneVarType.FLOAT:
                    SerializedProperty floatOpProperty = property.FindPropertyRelative("floatOP");
                    EditorGUI.PropertyField(opPosition, floatOpProperty, GUIContent.none);
                    FloatOperation floatOp = (FloatOperation)floatOpProperty.enumValueIndex;
                    secondParam = floatOp.HasSecondParameter();
                    operationDescription = floatOp.Description();
                    break;
                case SceneVarType.STRING:
                    EditorGUI.PropertyField(opPosition, property.FindPropertyRelative("stringOP"), GUIContent.none);
                    operationDescription = ((StringOperation)property.FindPropertyRelative("stringOP").enumValueIndex).Description();
                    break;
                case SceneVarType.EVENT:
                    EditorGUI.LabelField(opPosition, "Trigger");
                    EditorGUI.EndProperty();
                    return;
            }

            if (secondParam)
            {
                Rect var2Position = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 1.7f, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(var2Position, property.FindPropertyRelative("SceneVar2"), empty);
            }

            // SceneVar 2
            //List<SceneVar> sceneVarList2 = sceneVarContainer.GetListByType(type);
            //sceneVarUniqueID2P = property.FindPropertyRelative("var2UniqueID");
            //int sceneVarIndexSave2 = sceneVarContainer.GetIndexByUniqueID(sceneVarList2 ,sceneVarUniqueID2P.intValue);
            //if (sceneVarIndexSave2 == -1) sceneVarIndexSave2 = 0;
            //// SceneVar1 choice popup
            //Rect popup2Position = new Rect(position.x + position.width * 0.65f, position.y, position.width * 0.35f, EditorGUIUtility.singleLineHeight);
            //sceneVarIndex2 = EditorGUI.Popup(popup2Position, sceneVarIndexSave2, sceneVarContainer.VarStrings(sceneVarList2).ToArray());
            //if (sceneVarContainer.GetUniqueIDByIndex(sceneVarList2, sceneVarIndex2) == 0) sceneVarIndex2 = sceneVarIndexSave2;
            //sceneVarUniqueID2P.intValue = sceneVarContainer.GetUniqueIDByIndex(sceneVarList2, sceneVarIndex2);

            // Description labels
            //Rect label1Position = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width * 0.35f, EditorGUIUtility.singleLineHeight);
            Rect labelPosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width * 0.72f, EditorGUIUtility.singleLineHeight);
            //Rect label3Position = new Rect(position.x + position.width * 0.65f, position.y + EditorGUIUtility.singleLineHeight, position.width * 0.35f, EditorGUIUtility.singleLineHeight);
            //EditorGUI.LabelField(label1Position, type.ToString(), EditorStyles.miniLabel);
            EditorGUI.LabelField(labelPosition, operationDescription, style);
            //EditorGUI.LabelField(label3Position, sceneVarList2[sceneVarIndex2].type.ToString(), EditorStyles.miniLabel);

            // End
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SceneVarType type = (SceneVarType)property.FindPropertyRelative("var2Type").enumValueIndex;
            if (type == SceneVarType.EVENT) 
                return EditorGUIUtility.singleLineHeight * 1.3f;
            if (type == SceneVarType.BOOL && (BoolOperation)property.FindPropertyRelative("boolOP").enumValueIndex != BoolOperation.SET)
                return EditorGUIUtility.singleLineHeight * 1.8f;
            if (type == SceneVarType.INT)
            {
                IntOperation intOp = (IntOperation)property.FindPropertyRelative("intOP").enumValueIndex;
                if (intOp == IntOperation.TO_MIN || intOp == IntOperation.TO_MAX || intOp == IntOperation.TO_NULL || intOp == IntOperation.INCREMENT || intOp == IntOperation.DECREMENT) return EditorGUIUtility.singleLineHeight * 1.8f;
            }
            else if (type == SceneVarType.FLOAT)
            {
                FloatOperation floatOp = (FloatOperation)property.FindPropertyRelative("floatOP").enumValueIndex;
                if (floatOp == FloatOperation.TO_MIN || floatOp == FloatOperation.TO_MAX || floatOp == FloatOperation.TO_NULL || floatOp == FloatOperation.INCREMENT || floatOp == FloatOperation.DECREMENT) return EditorGUIUtility.singleLineHeight * 1.8f;
            }

            return EditorGUIUtility.singleLineHeight * 3f;
        }
    }
}
