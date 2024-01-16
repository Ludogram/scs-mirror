using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SubclassPicker : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SubclassPicker))]
public class SubclassPickerDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property);
    }

    IEnumerable GetClasses(Type baseType)
    {
        return Assembly.GetAssembly(baseType).GetTypes().Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t));
    }
    public List<System.Type> GetAllDerivedTypes(System.Type aType)
    {
        System.AppDomain aAppDomain = System.AppDomain.CurrentDomain;
        var result = new List<System.Type>();
        var assemblies = aAppDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.IsClass && !type.IsAbstract && aType.IsAssignableFrom(type))
                    result.Add(type);
            }
        }
        return result;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Type t = fieldInfo.FieldType;
        if (typeof(IEnumerable).IsAssignableFrom(t))
        {
            t = fieldInfo.FieldType.GetGenericArguments()[0];
        }
        string typeName = property.managedReferenceValue?.GetType().Name ?? "Not set";

        Rect dropdownRect = position;
        dropdownRect.x += EditorGUIUtility.labelWidth + 2;
        dropdownRect.width -= EditorGUIUtility.labelWidth + 2;
        dropdownRect.height = EditorGUIUtility.singleLineHeight;
        if (EditorGUI.DropdownButton(dropdownRect, new(typeName), FocusType.Keyboard))
        {
            GenericMenu menu = new GenericMenu();

            // null
            menu.AddItem(new GUIContent("None"), property.managedReferenceValue == null, () =>
            {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });

            // inherited types
            foreach (Type type in GetAllDerivedTypes(t))// GetClasses(t))
            {
                menu.AddItem(new GUIContent(type.Name), typeName == type.Name, () =>
                {
                    property.managedReferenceValue = type.GetConstructor(Type.EmptyTypes).Invoke(null);
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        }
        EditorGUI.PropertyField(position, property, label, true);
    }
}
#endif

