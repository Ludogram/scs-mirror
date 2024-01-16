using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dhs5.SceneCreation
{
    [Serializable]
    public class SelectList<T> : IList<T>
    {
        [SerializeField] private List<T> list;

        [SerializeField] private int currentElement;
        [SerializeField] private float propertyHeight;

        public T this[int index] { get => list[index]; set => list[index] = value; }

        public int Count => list.Count;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            list.Add(item);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            list.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SelectList<>))]
    public class SelectListDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.FindPropertyRelative("propertyHeight").floatValue;
        }


        SerializedProperty listProp;
        SerializedProperty elemProp;
        SerializedProperty currentProp;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            listProp = property.FindPropertyRelative("list");
            elemProp = property.FindPropertyRelative("currentElement");

            EditorGUI.BeginProperty(position, label, property);

            Rect r = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            property.isExpanded = EditorGUI.Foldout(r, property.isExpanded, label);
            r.y += EditorGUIUtility.singleLineHeight;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                if (GUI.Button(new Rect(r.x + (r.width - 52f), r.y, 25f, r.height), 
                    EditorGUIUtility.IconContent("d_Toolbar Plus")))
                {
                    listProp.InsertArrayElementAtIndex(listProp.arraySize);
                    elemProp.intValue = listProp.arraySize - 1;
                }
                if (GUI.Button(new Rect(r.x + (r.width - 25f), r.y, 25f, r.height),
                    EditorGUIUtility.IconContent("d_Toolbar Minus")))
                {
                    listProp.DeleteArrayElementAtIndex(elemProp.intValue);
                    elemProp.intValue = Mathf.Clamp(elemProp.intValue, 0, listProp.arraySize - 1);
                }

                if (listProp.arraySize == 0)
                {
                    EditorGUI.LabelField(r, "List empty");
                    EditorGUI.EndProperty();
                    return;
                }

                (string[] options, int[] values) = GetListDisplayOptions(listProp);
                elemProp.intValue = EditorGUI.IntPopup(new Rect(r.x, r.y, r.width - 55f, r.height),
                    elemProp.intValue, options, values);

                r.y += EditorGUIUtility.singleLineHeight * 1.25f;

                EditorGUI.indentLevel++;
                currentProp = listProp.GetArrayElementAtIndex(elemProp.intValue);
                EditorGUI.PropertyField(r, currentProp, true);
                r.y += EditorGUI.GetPropertyHeight(currentProp);
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }

            property.FindPropertyRelative("propertyHeight").floatValue = r.y - position.y;
            EditorGUI.EndProperty();
        }

        private (string[], int[]) GetListDisplayOptions(SerializedProperty property)
        {
            string[] options = new string[property.arraySize];
            int[] values = new int[property.arraySize];
            for (int i = 0; i < options.Length; i++)
            {
                options[i] = "Element " + i;
                values[i] = i;
            }
            return (options, values);
        }
    }
#endif
}
