using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dhs5.SceneCreation
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnly : PropertyAttribute
    {
        public string param;
        public bool inverse;

        public ReadOnly() { param = null; }
        public ReadOnly(string _param) { param = _param; }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ReadOnly))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        ReadOnly readOnly;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool disable = true;

            readOnly = attribute as ReadOnly;

            string param = readOnly.param;
            if (param != null)
            {
                disable = property.serializedObject.FindProperty(param).boolValue;
                if (readOnly.inverse) disable = !disable;
            }

            EditorGUI.BeginDisabledGroup(disable);
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndDisabledGroup();
        }
    }
#endif
}
