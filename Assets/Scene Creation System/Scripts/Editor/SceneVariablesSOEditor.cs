using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using NUnit.Framework;
using UnityEditorInternal;
using System;

namespace Dhs5.SceneCreation
{
    [CustomEditor(typeof(SceneVariablesSO))]
    public class SceneVariablesSOEditor : Editor
    {
        SceneVariablesSO sceneVariablesSO;

        //bool detailFoldoutOpen;
        string[] pageNames = new string[] { "Scene Vars", "Complex Vars", "Global Vars", "Balancing" };

        private void OnEnable()
        {
            sceneVariablesSO = target as SceneVariablesSO;

            sceneVariablesSO.OnEditorEnable();

            CreateBalancingSheetList("sceneBalancingSheets", "Balancing Sheets");
            CreateSceneVarList("sceneVars", "Scene Variables");
            CreateComplexSceneVarList("complexSceneVars", "Complex Scene Variables");
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            serializedObject.Update();

            Rect r = EditorGUILayout.BeginVertical();
            GUI.Box(r, GUIContent.none, GUI.skin.window);

            EditorGUILayout.Space(3f);
            EditorGUI.indentLevel++;
            
            //detailFoldoutOpen = EditorGUILayout.Foldout(detailFoldoutOpen, "Scene Creation Settings", true);
            //if (detailFoldoutOpen)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("intersceneVariablesSO"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("sceneObjectSettings"));
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(3f);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(15f);

            SerializedProperty pageProp = serializedObject.FindProperty("currentPage");
            pageProp.enumValueIndex = GUILayout.Toolbar(pageProp.enumValueIndex, pageNames);

            switch (pageProp.enumValueIndex)
            {
                case 0:
                    sceneVarList.DoLayoutList();
                    break;
                case 1:
                    complexSceneVarList.DoLayoutList();
                    break;
                case 2:
                    DisplayGlobalVars();
                    break;
                case 3:
                    balancingSheetList.DoLayoutList();
                    EditorGUILayout.Space(10f);
                    DisplaySelectedBalancingSheet();
                    break;
            }
            
            //EditorGUILayout.Space(10f);
            //
            //EditorGUILayout.Space(10f);
            

            serializedObject.ApplyModifiedProperties();
            UnityEditor.EditorUtility.SetDirty(target);
        }
        private void DisplaySelectedBalancingSheet()
        {
            SceneBalancingSheetSO bs = sceneVariablesSO.GetCurrentlySelectedBalancingSheet();
            if (bs == null) return;

            EditorGUILayout.LabelField(bs.name, EditorStyles.boldLabel);
            EditorGUILayout.Space(5f);
            Editor editor = Editor.CreateEditor(bs);
            editor.OnInspectorGUI();
        }
        private void DisplayGlobalVars()
        {
            if (sceneVariablesSO.IntersceneVariables == null) return;

            Editor editor = Editor.CreateEditor(sceneVariablesSO.IntersceneVariables);
            editor.OnInspectorGUI();
        }

        ReorderableList sceneVarList;
        private void CreateSceneVarList(string listPropertyName, string displayName)
        {
            serializedObject.Update();
            SerializedProperty textList = serializedObject.FindProperty(listPropertyName);

            sceneVarList = new ReorderableList(serializedObject, textList, true, true, true, true)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, displayName);
                },

                drawElementCallback = (rect, index, active, focused) =>
                {
                    var element = textList.GetArrayElementAtIndex(index);

                    EditorGUI.indentLevel++;
                    EditorGUI.BeginDisabledGroup(sceneVariablesSO.IsDisabledAtIndex(index));
                    EditorGUI.PropertyField(rect, element, true);
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel--;
                },

                onAddDropdownCallback = (rect, list) =>
                {
                    var menu = new GenericMenu();
                    SceneVarType type;
                    foreach (var value in Enum.GetValues(typeof(SceneVarType)))
                    {
                        type = (SceneVarType)value;
                        menu.AddItem(new GUIContent(type.ToString()), false, AddOfType, type);
                    }
                    menu.ShowAsContext();
                },

                onRemoveCallback = list =>
                {
                    sceneVariablesSO.TryRemoveSceneVarAtIndex(list.index);
                },

                onCanRemoveCallback = list =>
                {
                    return sceneVariablesSO.CanRemoveAtIndex(list.index);
                },
                
                elementHeightCallback = index => EditorGUI.GetPropertyHeight(textList.GetArrayElementAtIndex(index)),
            };

            void AddOfType(object type)
            {
                if (type is SceneVarType t)
                {
                    sceneVariablesSO.AddSceneVarOfType(t);
                }
                else
                {
                    Debug.LogError("Error in adding of type " + type);
                }
            }
        }
        
        ReorderableList complexSceneVarList;
        private void CreateComplexSceneVarList(string listPropertyName, string displayName)
        {
            serializedObject.Update();
            SerializedProperty textList = serializedObject.FindProperty(listPropertyName);

            complexSceneVarList = new ReorderableList(serializedObject, textList, true, true, true, true)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, displayName);
                },

                drawElementCallback = (rect, index, active, focused) =>
                {
                    var element = textList.GetArrayElementAtIndex(index);

                    EditorGUI.indentLevel++;
                    EditorGUI.PropertyField(rect, element, true);
                    EditorGUI.indentLevel--;
                },

                onAddDropdownCallback = (rect, list) =>
                {
                    var menu = new GenericMenu();
                    ComplexSceneVarType type;
                    foreach (var value in Enum.GetValues(typeof(ComplexSceneVarType)))
                    {
                        type = (ComplexSceneVarType)value;
                        menu.AddItem(new GUIContent(type.ToString()), false, AddOfType, type);
                    }
                    menu.ShowAsContext();
                },

                onRemoveCallback = list =>
                {
                    sceneVariablesSO.TryRemoveComplexSceneVarAtIndex(list.index);
                },
                
                elementHeightCallback = index => EditorGUI.GetPropertyHeight(textList.GetArrayElementAtIndex(index)),
            };

            void AddOfType(object type)
            {
                if (type is ComplexSceneVarType t)
                {
                    sceneVariablesSO.AddComplexSceneVarOfType(t);
                }
                else
                {
                    Debug.LogError("Error in adding of type " + type);
                }
            }
        }
        
        ReorderableList balancingSheetList;
        private void CreateBalancingSheetList(string listPropertyName, string displayName)
        {
            serializedObject.Update();
            SerializedProperty textList = serializedObject.FindProperty(listPropertyName);

            balancingSheetList = new ReorderableList(serializedObject, textList, true, true, true, true)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, displayName);
                },

                drawElementCallback = (rect, index, active, focused) =>
                {
                    var element = textList.GetArrayElementAtIndex(index);

                    EditorGUI.indentLevel++;
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.PropertyField(rect, element, new GUIContent("Sheet " + (index + 1)), true);
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel--;
                },

                onAddCallback = list =>
                {
                    sceneVariablesSO.CreateNewBalancingSheet();
                },

                onRemoveCallback = list =>
                {
                    sceneVariablesSO.TryRemoveBalancingSheetAtIndex(list.index);
                },

                onSelectCallback = list =>
                {
                    serializedObject.FindProperty("currentBalancingSheetIndex").intValue = list.index;
                },

                elementHeight = EditorGUIUtility.singleLineHeight
            };
        }
    }
}
