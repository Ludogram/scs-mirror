using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    [CustomPropertyDrawer(typeof(SceneObjectTag))]
    public class SceneObjectTagEditor : PropertyDrawer
    {
        SerializedProperty valueProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            valueProperty = property.FindPropertyRelative("_value");

            EditorGUI.BeginProperty(position, label, property);

            List<string> list = SceneObjectTag.Tags;
            if (list.IsValid())
            {
                string[] options = list.ToArray();
                valueProperty.intValue = EditorGUI.MaskField(position, label, valueProperty.intValue, options);
            }
            else
            {
                EditorGUI.LabelField(position, label, new GUIContent("No tags available"));
            }

            EditorGUI.EndProperty();
        }
    }
}
