namespace Example.VariousSkinning.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System;
    using System.Runtime.InteropServices;

    [CustomEditor(typeof(RenderChunk))]
    public class RenderChunkEditor : Editor
    {
        private RenderChunk targetAs { get { return target as RenderChunk; } }

        private string overrideTitle = "Override UnityEngine.Mesh to RenderChunk Data";
        private string fillTitle = "Fill RenderChunk Data";

        private SerializedProperty meshProperty;

        private void OnEnable()
        {
            meshProperty = serializedObject.FindProperty("mesh");
        }

        public override void OnInspectorGUI()
        {
            RenderChunk chunk = targetAs;

            EditorGUILayout.PropertyField(meshProperty);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Origin data");
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("VertexCount", chunk.mesh.vertexCount.ToString());
            EditorGUILayout.LabelField("IndexCount", chunk.mesh.triangles.Length.ToString());
            EditorGUILayout.LabelField("BoneWeightCount", chunk.mesh.boneWeights != null ? chunk.mesh.boneWeights.Length.ToString() : "null");

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Converted Data");
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("VertexCount", chunk.meshData != null? chunk.meshData.Length.ToString(): "null");
            EditorGUILayout.LabelField("IndexCount", chunk.indices != null ? chunk.indices.Length.ToString() : "null");
            EditorGUILayout.LabelField("BoneWeightCount", chunk.boneWeights != null ? chunk.boneWeights.Length.ToString() : "null");

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            
            if (GUILayout.Button(overrideTitle))
            {
                OverrideData();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button(fillTitle))
            {
                FillData();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        public void OverrideData()
        {
            RenderChunk chunk = targetAs;

            Mesh mesh = chunk.mesh;
            long time = DateTime.Now.Ticks;

            mesh.RecalculateNormals();

            chunk.vertexCount = mesh.vertexCount;

            EditorUtility.DisplayProgressBar(overrideTitle, "Get Mesh Data", 0f);

            MeshDataInfo[] meshData = new MeshDataInfo[mesh.vertexCount];

            // This code very long time
            for (int i = 0; i < meshData.Length; i++)
            {
                meshData[i].position = mesh.vertices[i];
                meshData[i].normal = mesh.normals[i];
                meshData[i].uv = mesh.uv[i];
            }

            chunk.meshData = meshData;

            EditorUtility.DisplayProgressBar(overrideTitle, "Get Index Data", 0.33f);

            chunk.indices = new int[mesh.triangles.Length];
            Array.Copy(mesh.triangles, chunk.indices, chunk.indices.Length);

            EditorUtility.DisplayProgressBar(overrideTitle, "Get Index Count Data", 0.66f);

            chunk.indexCounts = new uint[mesh.subMeshCount];
            for (int i = 0; i < chunk.indexCounts.Length; i++)
                chunk.indexCounts[i] = mesh.GetIndexStart(i) + mesh.GetIndexCount(i);

            EditorUtility.DisplayProgressBar(overrideTitle, "Get boneWeight Data", 1f);

            chunk.boneWeights = new CustomBoneWeight[mesh.boneWeights.Length];
            Array.Copy(mesh.boneWeights, chunk.boneWeights, chunk.boneWeights.Length);

            EditorUtility.ClearProgressBar();

            float measuredTime = (float)((DateTime.Now.Ticks - time) / TimeSpan.TicksPerMillisecond) / 1000;
            Debug.Log("Measured time : " + measuredTime);
        }

        public void FillData()
        {
            RenderChunk chunk = targetAs;
            Mesh mesh = chunk.mesh;

            chunk.vertexCount = mesh.vertexCount;

            if (chunk.meshData == null || chunk.meshData.Length != mesh.vertexCount)
            {
                EditorUtility.DisplayProgressBar(fillTitle, "Get Mesh Data", 0f);

                chunk.meshData = new MeshDataInfo[mesh.vertexCount];
                for (int i = 0; i < chunk.meshData.Length; i++)
                    chunk.meshData[i] = new MeshDataInfo() { position = mesh.vertices[i], normal = mesh.normals[i], uv = mesh.uv[i] };
            }

            if (chunk.indices == null || chunk.indices.Length != mesh.triangles.Length)
            {
                EditorUtility.DisplayProgressBar(fillTitle, "Get Index Data", 0.33f);

                chunk.indices = new int[mesh.triangles.Length];
                Array.Copy(mesh.triangles, chunk.indices, chunk.indices.Length);
            }

            EditorUtility.DisplayProgressBar(fillTitle, "Get Index Count Data", 0.66f);

            chunk.indexCounts = new uint[mesh.subMeshCount];
            for (int i = 0; i < chunk.indexCounts.Length; i++)
                chunk.indexCounts[i] = mesh.GetIndexCount(i);

            if (chunk.boneWeights == null || chunk.boneWeights.Length != mesh.boneWeights.Length)
            {
                EditorUtility.DisplayProgressBar(overrideTitle, "Get boneWeight Data", 1f);

                chunk.boneWeights = new CustomBoneWeight[mesh.boneWeights.Length];
                Array.Copy(mesh.boneWeights, chunk.boneWeights, chunk.boneWeights.Length);
            }

            EditorUtility.ClearProgressBar();
        }
    }
}