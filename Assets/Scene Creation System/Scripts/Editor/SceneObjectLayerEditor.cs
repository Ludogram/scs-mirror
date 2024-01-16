using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    [CustomPropertyDrawer(typeof(SceneObjectLayer))]
    public class SceneObjectLayerEditor : PropertyDrawer
    {
        SerializedProperty valueProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            valueProperty = property.FindPropertyRelative("_value");

            EditorGUI.BeginProperty(position, label, property);

            //valueProperty.intValue = EditorGUI.IntPopup(position, label.text, valueProperty.intValue, 
            //    SceneObjectLayer.Layers.ToArray(), SceneObjectLayer.LayerValues.ToArray());

            int currentValue = valueProperty.intValue;

            if (EditorGUI.DropdownButton(position, new GUIContent(SceneObjectLayer.LayerToName(currentValue)), FocusType.Passive))
            {
                List<string> list = SceneObjectLayer.Layers;
                List<int> values = SceneObjectLayer.LayerValues;
                GenericMenu menu = new();
                for (int i = 0; i < list.Count; i++)
                {
                    menu.AddItem(new GUIContent(list[i]), values[i] == currentValue, Choose, values[i]);
                }
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Add Layer"), false, EditorHelper.GetSceneObjectLayerDatabase);
                menu.ShowAsContext();
            }

            void Choose(object index)
            {
                valueProperty.intValue = (int)index;
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }
    }
}
