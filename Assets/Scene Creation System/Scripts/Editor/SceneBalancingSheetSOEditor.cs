using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using NUnit.Framework;
using UnityEditorInternal;

namespace Dhs5.SceneCreation
{
    [CustomEditor(typeof(SceneBalancingSheetSO))]
    public class SceneBalancingSheetSOEditor : Editor
    {
        SceneBalancingSheetSO sceneBalancingSheetSO;

        private void OnEnable()
        {
            sceneBalancingSheetSO = target as SceneBalancingSheetSO;

            if (sceneBalancingSheetSO.sceneVariablesSO == null) return;

            sceneBalancingSheetSO.ApplyTemplate();

            CreateReorderableList("balancingVars", sceneBalancingSheetSO.balancingVars, "Scene Variables overrides");
        }

        bool sceneVarNull = false;
        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            sceneVarNull = sceneBalancingSheetSO.sceneVariablesSO == null;

            EditorGUILayout.BeginVertical();

            if (sceneVarNull)
            {
                EditorGUILayout.HelpBox("Can't find the corresponding SceneVariablesSO !", MessageType.Error);
                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
                EditorGUILayout.EndVertical();
                serializedObject.ApplyModifiedProperties();
                return;
            }

            list?.DoLayoutList();
            
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            UnityEditor.EditorUtility.SetDirty(target);
        }

        #region Helper functions

        ReorderableList list;
        private void CreateReorderableList(string listPropertyName, List<BalancingVar> balancingVars, string displayName)
        {
            SerializedProperty textList = serializedObject.FindProperty(listPropertyName);
            list = new ReorderableList(serializedObject, textList, false, true, false, false)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, displayName);
                },

                drawElementCallback = (rect, index, active, focused) =>
                {
                    var element = textList.GetArrayElementAtIndex(index);

                    EditorGUI.indentLevel++;
                    // Disable here
                    EditorGUI.PropertyField(rect, element, new GUIContent(balancingVars[index].ID), true);
                    // 
                    EditorGUI.indentLevel--;
                },

                elementHeightCallback = index =>
                {
                    var element = textList.GetArrayElementAtIndex(index);

                    return EditorGUI.GetPropertyHeight(element);
                }
            };
        }
        #endregion
    }
}
