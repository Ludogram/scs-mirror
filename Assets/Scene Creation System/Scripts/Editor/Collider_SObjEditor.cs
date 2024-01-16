using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    [CustomEditor(typeof(Collider_SObj), true)]
    public class Collider_SObjEditor : BaseSceneObjectEditor
    {
        Collider_SObj collider;

        protected override void OnEnable()
        {
            base.OnEnable();

            collider = (Collider_SObj)baseSceneObject;
        }

        protected override void DrawDefault()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("collider"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("layerMask"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("infiniteUse"));

            // Infinite Use
            EditorGUI.indentLevel++;

            if (!collider.InfiniteUse)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("useNumber"));
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("interactions"));

            if (collider.DoesCollisionEnter)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onCollisionEnter"), true);
            }
            if (collider.DoesCollisionStay)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onCollisionStay"), true);
            }
            if (collider.DoesCollisionExit)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onCollisionExit"), true);
            }
            if (collider.DoesTriggerEnter)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onTriggerEnter"), true);
            }
            if (collider.DoesTriggerStay)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onTriggerStay"), true);
            }
            if (collider.DoesTriggerExit)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onTriggerExit"), true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
