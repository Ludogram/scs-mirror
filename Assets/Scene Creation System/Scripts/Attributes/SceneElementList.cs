using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Dhs5.SceneCreation
{
    [Serializable]
    public class SceneElementList<T> : IList<T>
    {
        [SerializeField] private List<T> list;

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
    [CustomPropertyDrawer(typeof(SceneElementList<>))]
    public class SceneElementListEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return listDico.ContainsKey(property.propertyPath) ?
                listDico[property.propertyPath] != null ? listDico[property.propertyPath].GetHeight() 
                : EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight;
        }


        Dictionary<string, ReorderableList> listDico = new();
        ReorderableList list;
        SerializedProperty listProp;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            listProp = property.FindPropertyRelative("list");

            if (!listDico.ContainsKey(property.propertyPath) || listDico[property.propertyPath] == null)
                listDico[property.propertyPath] = CreateSceneElementList(property, listProp, label);

            list = listDico[property.propertyPath];

            EditorGUI.BeginProperty(position, label, property);

            list.DoList(position);

            EditorGUI.EndProperty();
        }

        Color selectionBlue = SceneCreationSettings.instance.EditorColors.selectionBlue;
        private ReorderableList CreateSceneElementList(SerializedProperty property, SerializedProperty listProperty, GUIContent label)
        {
            return new ReorderableList(property.serializedObject, listProperty, true, true, true, true)
            {
                drawElementBackgroundCallback = (rect, index, active, focused) =>
                {
                    if (active)
                    {
                        if (focused) GUI.backgroundColor = selectionBlue;
                        GUI.Box(rect, GUIContent.none, GUI.skin.button);
                        if (focused) GUI.backgroundColor = Color.white;
                    }
                },
                drawHeaderCallback = rect =>
                {
                    var prev = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;
                    EditorGUI.LabelField(rect, label);
                    EditorGUI.indentLevel = prev;
                },
                drawElementCallback = (rect, index, active, focused) =>
                {
                    EditorGUI.PropertyField(rect, listProperty.GetArrayElementAtIndex(index), true);
                },
                elementHeightCallback = index =>
                {
                    return EditorGUI.GetPropertyHeight(listProperty.GetArrayElementAtIndex(index));
                },
                elementHeight = EditorGUIUtility.singleLineHeight * 2.6f
            };
        }
    }
#endif
}
