using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

namespace Dhs5.SceneCreation
{
    [CustomEditor(typeof(IntersceneVariablesSO))]
    public class IntersceneVariablesSOEditor : Editor
    {
        IntersceneVariablesSO intersceneVariablesSO;

        private void OnEnable()
        {
            intersceneVariablesSO = target as IntersceneVariablesSO;

            //intersceneVariablesSO.OnEditorEnable();

            CreateSceneVarList("sceneVars", "Scene Variables");
            //CreateComplexSceneVarList("complexSceneVars", "Complex Scene Variables");
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.BeginVertical();

            sceneVarList.DoLayoutList();
            //EditorGUILayout.Space(10f);
            //complexSceneVarList.DoLayoutList();

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            UnityEditor.EditorUtility.SetDirty(target);
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
                    EditorGUI.BeginDisabledGroup(intersceneVariablesSO.IsDisabledAtIndex(index));
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
                    intersceneVariablesSO.TryRemoveSceneVarAtIndex(list.index);
                },

                onCanRemoveCallback = list =>
                {
                    return intersceneVariablesSO.CanRemoveAtIndex(list.index);
                },
                
                elementHeightCallback = index => EditorGUI.GetPropertyHeight(textList.GetArrayElementAtIndex(index)),
            };

            void AddOfType(object type)
            {
                if (type is SceneVarType t)
                {
                    intersceneVariablesSO.AddSceneVarOfType(t);
                }
                else
                {
                    Debug.LogError("Error in adding of type " + type);
                }
            }
        }
        
        /*
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
                    intersceneVariablesSO.TryRemoveComplexSceneVarAtIndex(list.index);
                },
                
                elementHeightCallback = index => EditorGUI.GetPropertyHeight(textList.GetArrayElementAtIndex(index)),
            };

            void AddOfType(object type)
            {
                if (type is ComplexSceneVarType t)
                {
                    intersceneVariablesSO.AddComplexSceneVarOfType(t);
                }
                else
                {
                    Debug.LogError("Error in adding of type " + type);
                }
            }
        }
        */
    }
}
