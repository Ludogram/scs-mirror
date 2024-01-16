using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Dhs5.SceneCreation
{
    [CustomPropertyDrawer(typeof(BalancingVar))]
    public class BalancingVarEditor : PropertyDrawer
    {
        SerializedProperty idProperty;
        SerializedProperty typeProperty;
        SerializedProperty staticProperty;
        SerializedProperty randomProperty;
        SerializedProperty linkProperty;
        SerializedProperty overrideProperty;

        private SerializedProperty hasMinProperty;
        private SerializedProperty hasMaxProperty;
        private SerializedProperty minIntProperty;
        private SerializedProperty maxIntProperty;
        private SerializedProperty minfloatProperty;
        private SerializedProperty maxfloatProperty;

        float propertyHeight;
        float propertyOffset;

        SceneVarType type;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //uniqueIDProperty = property.FindPropertyRelative("uniqueID");
            idProperty = property.FindPropertyRelative("ID");
            typeProperty = property.FindPropertyRelative("type");
            staticProperty = property.FindPropertyRelative("isStatic");
            randomProperty = property.FindPropertyRelative("isRandom");
            linkProperty = property.FindPropertyRelative("isLink");
            overrideProperty = property.FindPropertyRelative("overrideVar");

            type = (SceneVarType)typeProperty.enumValueIndex;

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginDisabledGroup(type == SceneVarType.EVENT || linkProperty.boolValue);
            Rect overrideRect = new Rect(position.x + 10, position.y, position.width * 0.8f - 10, EditorGUIUtility.singleLineHeight);
            overrideProperty.boolValue = EditorGUI.ToggleLeft(overrideRect, "Override : " + idProperty.stringValue, overrideProperty.boolValue);
            EditorGUI.EndDisabledGroup();

            // Unique ID
            Rect typeRect = new Rect(position.x + position.width * 0.81f, position.y, position.width * 0.19f, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(typeRect, linkProperty.boolValue ? "LINK" : type.ToString());
            propertyHeight = EditorGUIUtility.singleLineHeight;
            propertyOffset = EditorGUIUtility.singleLineHeight;

            if (overrideProperty.boolValue)
            {
                Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, "");
                if (property.isExpanded)
                {
                    if (!randomProperty.boolValue)
                    {
                        Rect valueLabelRect = new Rect(position.x, position.y + propertyOffset, 75, EditorGUIUtility.singleLineHeight);
                        EditorGUI.LabelField(valueLabelRect, (staticProperty.boolValue ? "" : "Initial ") + "Value");
                        Rect valueRect = new Rect(position.x + 75, position.y + propertyOffset, position.width * 0.75f - 75, EditorGUIUtility.singleLineHeight);

                        switch (type)
                        {
                            case SceneVarType.BOOL:
                                EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("boolValue"), new GUIContent(""));
                                break;
                            case SceneVarType.INT:
                                EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("intValue"), new GUIContent(""));
                                break;
                            case SceneVarType.FLOAT:
                                EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("floatValue"), new GUIContent(""));
                                break;
                            case SceneVarType.STRING:
                                EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("stringValue"), new GUIContent(""));
                                break;
                        }

                        // Static
                        EditorGUI.BeginDisabledGroup(true);
                        Rect staticRect = new Rect(position.x + position.width * 0.76f, position.y + propertyOffset, position.width * 0.24f,
                            EditorGUIUtility.singleLineHeight);
                        staticProperty.boolValue = EditorGUI.ToggleLeft(staticRect, "Static", staticProperty.boolValue);
                        propertyOffset += EditorGUIUtility.singleLineHeight * 1.2f;
                        propertyHeight += EditorGUIUtility.singleLineHeight * 1.2f;
                        EditorGUI.EndDisabledGroup();
                    }

                    if (!staticProperty.boolValue && (type == SceneVarType.INT || type == SceneVarType.FLOAT))
                    {
                        hasMinProperty = property.FindPropertyRelative("hasMin");
                        hasMaxProperty = property.FindPropertyRelative("hasMax");

                        EditorGUI.BeginDisabledGroup(true);
                        Rect hasMinRect = new Rect(position.x, position.y + propertyOffset, position.width * 0.11f, EditorGUIUtility.singleLineHeight);
                        Rect hasMaxRect = new Rect(position.x + position.width * 0.38f, position.y + propertyOffset, position.width * 0.12f, EditorGUIUtility.singleLineHeight);
                        hasMinProperty.boolValue = EditorGUI.ToggleLeft(hasMinRect, new GUIContent("Min", "Min inclusive"), hasMinProperty.boolValue);
                        hasMaxProperty.boolValue = EditorGUI.ToggleLeft(hasMaxRect, new GUIContent("Max", "Max inclusive"), hasMaxProperty.boolValue);
                        EditorGUI.EndDisabledGroup();

                        if (hasMinProperty.boolValue)
                        {
                            Rect minRect = new Rect(position.x + position.width * 0.12f, position.y + propertyOffset, position.width * 0.24f, EditorGUIUtility.singleLineHeight);

                            if (type == SceneVarType.INT)
                            {
                                minIntProperty = property.FindPropertyRelative("minInt");
                                EditorGUI.PropertyField(minRect, minIntProperty, new GUIContent(""));
                            }
                            else if (type == SceneVarType.FLOAT)
                            {
                                minfloatProperty = property.FindPropertyRelative("minFloat");
                                EditorGUI.PropertyField(minRect, minfloatProperty, new GUIContent(""));
                            }
                        }
                        if (hasMaxProperty.boolValue)
                        {
                            Rect maxRect = new Rect(position.x + position.width * 0.51f, position.y + propertyOffset, position.width * 0.24f, EditorGUIUtility.singleLineHeight);

                            if (type == SceneVarType.INT)
                            {
                                maxIntProperty = property.FindPropertyRelative("maxInt");
                                EditorGUI.PropertyField(maxRect, maxIntProperty, new GUIContent(""));
                                if (hasMinProperty.boolValue && (maxIntProperty.intValue <= minIntProperty.intValue))
                                {
                                    maxIntProperty.intValue = minIntProperty.intValue + 1;
                                }
                            }
                            else if (type == SceneVarType.FLOAT)
                            {
                                maxfloatProperty = property.FindPropertyRelative("maxFloat");
                                EditorGUI.PropertyField(maxRect, maxfloatProperty, new GUIContent(""));
                                if (hasMinProperty.boolValue && (maxfloatProperty.floatValue <= minfloatProperty.floatValue))
                                {
                                    maxfloatProperty.floatValue = minfloatProperty.floatValue + 0.1f;
                                }
                            }
                        }
                        // Random
                        EditorGUI.BeginDisabledGroup(true);
                        Rect randomRect = new Rect(position.x + position.width * 0.76f, position.y + propertyOffset, position.width * 0.24f,
                            EditorGUIUtility.singleLineHeight);
                        randomProperty.boolValue = EditorGUI.ToggleLeft(randomRect, "Random", randomProperty.boolValue);
                        EditorGUI.EndDisabledGroup();

                        if (randomProperty.boolValue)
                        {
                            hasMinProperty.boolValue = true;
                            hasMaxProperty.boolValue = true;
                        }

                        propertyOffset += EditorGUIUtility.singleLineHeight;
                        propertyHeight += EditorGUIUtility.singleLineHeight;
                    }

                    propertyHeight += EditorGUIUtility.singleLineHeight * 0.2f;
                }
            }

            EditorGUI.EndProperty();

            property.FindPropertyRelative("propertyHeight").floatValue = propertyHeight;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.FindPropertyRelative("propertyHeight").floatValue;
        }
    }
}
