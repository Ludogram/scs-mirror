using Codice.CM.SEIDInfo;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace Dhs5.SceneCreation
{
    [CustomPropertyDrawer(typeof(SceneTimelineList))]
    public class SceneTimelineListEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.FindPropertyRelative("propertyHeight").floatValue;
        }


        SerializedProperty listProp;

        SerializedProperty timelineIndexProp;
        SerializedProperty stepIndexProp;
        
        SerializedProperty currentTimelineProp;
        SerializedProperty stepsProp;
        SerializedProperty currentStepProp;


        SerializedProperty idProp;
        SerializedProperty loopEndProp;

        int currentTimelineIndex;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            listProp = property.FindPropertyRelative("list");

            timelineIndexProp = property.FindPropertyRelative("currentTimeline");
            stepIndexProp = property.FindPropertyRelative("currentStep");
            

            EditorGUI.BeginProperty(position, label, property);

            GUI.Box(new Rect(
                position.x - 3f, position.y - 1f, position.width + 3f, GetPropertyHeight(property, label) + 2f),
                GUIContent.none, GUI.skin.window);

            Rect r = new Rect(position.x, position.y, position.width - 3f, EditorGUIUtility.singleLineHeight);

            Indent(13f);
            property.isExpanded = EditorGUI.Foldout(r, property.isExpanded, label);
            r.y += EditorGUIUtility.singleLineHeight * 1.25f;

            if (property.isExpanded)
            {
                //Indent(10f);

                EditorGUI.LabelField(r, "Timeline", EditorStyles.boldLabel);
                r.y += EditorGUIUtility.singleLineHeight * 1.25f;

                Indent(10f);

                // Display Timeline Choice
                if (GUI.Button(new Rect(r.x + (r.width - 52f), r.y, 25f, r.height),
                    EditorGUIUtility.IconContent("d_Toolbar Plus", "Add a timeline")))
                {
                    AddTimeline();
                }

                if (listProp.arraySize == 0)
                {
                    EditorGUI.LabelField(r, "No Timeline yet");
                    r.y += EditorGUIUtility.singleLineHeight * 1.25f;
                    Indent(-10f);
                    End();
                    return;
                }

                if (GUI.Button(new Rect(r.x + (r.width - 25f), r.y, 25f, r.height),
                    EditorGUIUtility.IconContent("d_Toolbar Minus", "Remove this timeline")))
                {
                    RemoveTimeline(timelineIndexProp.intValue);
                }

                (string[] options, int[] values) = GetTimelineDisplayOptions(listProp);
                timelineIndexProp.intValue = EditorGUI.IntPopup(new Rect(r.x, r.y, r.width - 55f, r.height),
                    timelineIndexProp.intValue, options, values);
                if (currentTimelineIndex != timelineIndexProp.intValue)
                {
                    stepIndexProp.intValue = 0;
                    currentTimelineIndex = timelineIndexProp.intValue;
                }

                currentTimelineProp = listProp.GetArrayElementAtIndex(timelineIndexProp.intValue);
                stepsProp = currentTimelineProp.FindPropertyRelative("steps");

                r.y += EditorGUIUtility.singleLineHeight * 1.25f;

                // Display Timeline Params
                Indent(2f);
                idProp = currentTimelineProp.FindPropertyRelative("ID");
                loopEndProp = currentTimelineProp.FindPropertyRelative("endLoopCondition");

                EditorGUI.PropertyField(r, idProp);
                r.y += EditorGUI.GetPropertyHeight(idProp);

                r.y += 3f;
                EditorGUI.PropertyField(r, loopEndProp);
                r.y += EditorGUI.GetPropertyHeight(loopEndProp);

                r.y += EditorGUIUtility.singleLineHeight * 0.5f;

                // Separator
                EditorGUI.DrawRect(new Rect(position.x + 1f, r.y, position.width - 3f, 1f), Color.grey);

                r.y += EditorGUIUtility.singleLineHeight * 0.5f;

                // Display Step Choice
                Indent(-12f);

                EditorGUI.LabelField(r, "Step", EditorStyles.boldLabel);
                r.y += EditorGUIUtility.singleLineHeight * 1.25f;

                Indent(10f);

                if (stepIndexProp.intValue > 0 && 
                    GUI.Button(new Rect(r.x + (r.width - 106f), r.y, 25f, r.height),
                    EditorGUIUtility.IconContent("HoverBar_Up", "Move this step up")))
                {
                    MoveStep(true, stepIndexProp.intValue);
                }
                if (stepIndexProp.intValue < stepsProp.arraySize - 1 && 
                    GUI.Button(new Rect(r.x + (r.width - 79f), r.y, 25f, r.height),
                    EditorGUIUtility.IconContent("HoverBar_Down", "Move this step down")))
                {
                    MoveStep(false, stepIndexProp.intValue);
                }
                
                if (GUI.Button(new Rect(r.x + (r.width - 52f), r.y, 25f, r.height),
                    EditorGUIUtility.IconContent("d_Toolbar Plus", "Add a step")))
                {
                    AddStep();
                }

                if (stepsProp.arraySize == 0)
                {
                    EditorGUI.LabelField(r, "No Step yet");
                    r.y += EditorGUIUtility.singleLineHeight * 1.25f;
                    Indent(-10f);
                    End();
                    return;
                }

                if (GUI.Button(new Rect(r.x + (r.width - 25f), r.y, 25f, r.height),
                    EditorGUIUtility.IconContent("d_Toolbar Minus", "Remove this step")))
                {
                    RemoveStep(stepIndexProp.intValue);
                    if (stepsProp.arraySize < 1)
                    {
                        Indent(-10f);
                        End();
                        return;
                    }
                }

                (string[] steps, int[] stepValues) = GetStepsDisplayOptions(stepsProp);
                stepIndexProp.intValue = EditorGUI.IntPopup(new Rect(r.x, r.y, r.width - 110f, r.height),
                    stepIndexProp.intValue, steps, stepValues);

                currentStepProp = stepsProp.GetArrayElementAtIndex(stepIndexProp.intValue);

                r.y += EditorGUIUtility.singleLineHeight * 1.5f;

                // Display Step
                Indent(2f);
                EditorGUI.PropertyField(r, currentStepProp, true);
                r.y += EditorGUI.GetPropertyHeight(currentStepProp);
                r.y += 5f;
                Indent(-10f);

                Indent(-10f);
            }

            End();

            void End()
            {
                property.FindPropertyRelative("propertyHeight").floatValue = r.y - position.y;
                EditorGUI.EndProperty();
            }
            void Indent(float value)
            {
                r.x += value;
                r.width -= value;
            }
        }

        private void AddTimeline()
        {
            listProp.InsertArrayElementAtIndex(listProp.arraySize);
            listProp.GetArrayElementAtIndex(listProp.arraySize - 1).FindPropertyRelative("ID").stringValue = "Timeline " + listProp.arraySize;
            listProp.GetArrayElementAtIndex(listProp.arraySize - 1).FindPropertyRelative("steps").ClearArray();
            timelineIndexProp.intValue = listProp.arraySize - 1;
            stepIndexProp.intValue = 0;
        }
        private void RemoveTimeline(int index)
        {
            listProp.DeleteArrayElementAtIndex(index);
            timelineIndexProp.intValue = Mathf.Clamp(index, 0, listProp.arraySize - 1);
            stepIndexProp.intValue = 0;
        }

        private (string[], int[]) GetTimelineDisplayOptions(SerializedProperty property)
        {
            string id;
            string[] options = new string[property.arraySize];
            int[] values = new int[property.arraySize];
            for (int i = 0; i < options.Length; i++)
            {
                id = property.GetArrayElementAtIndex(i).FindPropertyRelative("ID").stringValue;
                options[i] = "Timeline " + i + ": " + id;
                values[i] = i;
            }
            return (options, values);
        }

        private void AddStep()
        {
            stepsProp.InsertArrayElementAtIndex(stepsProp.arraySize);
            stepIndexProp.intValue = stepsProp.arraySize - 1;
        }
        private void RemoveStep(int index)
        {
            stepsProp.DeleteArrayElementAtIndex(index);
            stepIndexProp.intValue = Mathf.Clamp(index, 0, stepsProp.arraySize - 1);
        }
        private void MoveStep(bool up, int index)
        {
            int newIndex = up ? index - 1 : index + 1;
            stepsProp.MoveArrayElement(index, newIndex);
            stepIndexProp.intValue = newIndex;
        }
        private (string[], int[]) GetStepsDisplayOptions(SerializedProperty property)
        {
            string id;
            string[] options = new string[property.arraySize];
            int[] values = new int[property.arraySize];
            for (int i = 0; i < options.Length; i++)
            {
                id = property.GetArrayElementAtIndex(i).FindPropertyRelative("ID").stringValue;
                options[i] = "Step " + i + ": " + id;
                values[i] = i;
            }
            return (options, values);
        }
    }
}
