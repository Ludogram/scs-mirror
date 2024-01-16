using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    public class SceneStateWindow : EditorWindow
    {
        [MenuItem("SCS/Window/Scene State")]
        private static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(SceneStateWindow));
        }


        private SceneVariablesSO sceneVariablesSO;

        // Scene Dependency
        private ReorderableList dependantsList;
        private int currentUID;
        private bool rebuildDependencies = false;

        // Scene Vars
        private Vector2 currentPosition = Vector2.zero;

        private bool filterByType;
        private SceneVarType typeFilter;

        private bool displayStatics = true;
        private bool displayEvents;
        private bool displayRandoms;


        private void OnEnable()
        {
            sceneVariablesSO = EditorHelper.GetCurrentSceneVariablesSO();
            rebuildDependencies = true;
        }

        private void OnGUI()
        {
            currentPosition = EditorGUILayout.BeginScrollView(currentPosition);
            EditorGUILayout.BeginVertical();

            // ----- Scene Dependencies -----
            if (sceneVariablesSO != null)
            {
                EditorGUI.DropShadowLabel(EditorGUILayout.GetControlRect(false, 20f), "Scene Dependencies");
                EditorGUILayout.Space(10f);

                // Choose Scene Var
                List<SceneVar> sceneVarList = sceneVariablesSO.SceneVars;
                if (sceneVarList.IsValid())
                {
                    int sceneVarIndexSave = sceneVarList.GetIndexByUniqueID(currentUID);
                    if (sceneVarIndexSave == -1) sceneVarIndexSave = 0;

                    // SceneVar choice popup
                    int sceneVarIndex = EditorGUILayout.Popup(sceneVarIndexSave, sceneVarList.VarStrings().ToArray());
                    if (sceneVarList.GetUniqueIDByIndex(sceneVarIndex) == 0) sceneVarIndex = sceneVarIndexSave;
                    int newUID = sceneVarList.GetUniqueIDByIndex(sceneVarIndex);
                    if (rebuildDependencies || newUID != currentUID)
                    {
                        currentUID = newUID;
                        rebuildDependencies = false;
                        if (currentUID != 0)
                        {
                            CreateDependantsList(currentUID);
                        }
                    }

                    // Display List
                    if (currentUID != 0)
                    {
                        dependantsList?.DoLayoutList();
                    }
                }
            }

            if (Application.isPlaying)
            {
                // ----- Runtime Scene Vars -----
                EditorGUI.DropShadowLabel(EditorGUILayout.GetControlRect(false, 20f), "Runtime Scene Vars");

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Filters");
                // Type
                EditorGUILayout.BeginHorizontal();
                filterByType = EditorGUILayout.ToggleLeft("Filter by type", filterByType);
                if (filterByType)
                {
                    typeFilter = (SceneVarType)EditorGUILayout.EnumPopup("Type Filter", typeFilter);
                }
                EditorGUILayout.EndHorizontal();
                // Statics
                displayStatics = EditorGUILayout.ToggleLeft("Display Statics", displayStatics);
                // Events
                displayEvents = EditorGUILayout.ToggleLeft("Display Events", displayEvents);
                // Randoms
                displayRandoms = EditorGUILayout.ToggleLeft("Display Randoms", displayRandoms);

                EditorGUILayout.Space();

                SceneVar sceneVar;
                foreach (var pair in SceneState.GetCurrentSceneVars())
                {
                    sceneVar = pair.Value;
                    if ((!filterByType || sceneVar.type == typeFilter)
                        && (displayStatics || !sceneVar.IsStatic)
                        && (displayEvents || sceneVar.type != SceneVarType.EVENT)
                        && (displayRandoms || (!sceneVar.IsRandom && !sceneVar.IsLinkRandom)))
                    {
                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField(pair.Value.RuntimeCompleteString());

                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void CreateDependantsList(int UID)
        {
            List<BaseSceneObject> dependants = SceneDependency.GetSceneVarDependants(UID);

            if (dependants.IsValid())
            {
                dependantsList = new ReorderableList(dependants, typeof(BaseSceneObject), false, true, false, false)
                {
                    drawHeaderCallback = rect =>
                    {
                        EditorGUI.LabelField(rect, "Dependants");
                    },

                    drawElementCallback = (rect, index, active, focused) =>
                    {
                        if (dependants[index] != null)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUI.ObjectField(new Rect(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight), dependants[index].name, dependants[index], typeof(BaseSceneObject), true);
                            EditorGUI.EndDisabledGroup();
                            EditorGUI.indentLevel--;
                        }
                        else
                        {
                            rebuildDependencies = true;
                        }
                    },

                    elementHeight = EditorGUIUtility.singleLineHeight * 1.1f,
                };
            }
            else dependantsList = null;
        }
    }
}
