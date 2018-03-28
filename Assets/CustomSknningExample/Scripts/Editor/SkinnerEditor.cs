namespace CustomSkinningExample.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(Skinner))]
    public class SkinnerEditor : Editor
    {
        public Skinner targetAs { get { return target as Skinner; } }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(target as MonoBehaviour), typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();

            {
                SerializedProperty
                    blendType = serializedObject.FindProperty("blend"),
                    computeType = serializedObject.FindProperty("compute"),
                    tension = serializedObject.FindProperty("tension");

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(blendType);
                EditorGUILayout.PropertyField(computeType);
                EditorGUILayout.PropertyField(tension);

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                    if (EditorApplication.isPlaying)
                        targetAs.ReloadAdaptor(targetAs.skinningCompute, targetAs.skinningBlend, targetAs.calculateTension);
                }
            }

            {
                SerializedProperty
                   renderChunk = serializedObject.FindProperty("chunk"),
                   computeShader = serializedObject.FindProperty("computeShader"),
                   material = serializedObject.FindProperty("material");

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(renderChunk);
                EditorGUILayout.PropertyField(computeShader);
                EditorGUILayout.PropertyField(material);

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                }

                if (
                    renderChunk.objectReferenceValue == null || 
                    computeShader.objectReferenceValue == null || 
                    material.objectReferenceValue == null
                    )
                    EditorGUILayout.HelpBox("Missing properties..", MessageType.Error);
                else
                    EditorGUILayout.HelpBox("Properties has been prepared!", MessageType.Info);
            }

            {
                SerializedProperty tensionDbg = serializedObject.FindProperty("tensionDebug");

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(tensionDbg);

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();

                    targetAs.RefreshDebug();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
