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
        RenderChunk targetAs { get { return target as RenderChunk; } }

        const string getboneTitle = "Get Bone Data";

        const string overrideTitle = "Override UnityEngine.Mesh to RenderChunk Data";
        const string addTitle = "Add UnityEngine.Mesh to RenderChunk Data";
        const string fillTitle = "Fill RenderChunk Data from UnityEngine.Mesh";

        SerializedProperty meshProperty;
        SerializedProperty rendererProperty;

        bool
            originDataToggle = true,
            convertedDataToggle = true,
            eachBoneVertexDataToggle = true;

        /*
         * Test variable
         */
        bool logSimilarityToggle;
        int cmpIndex1 = 0, cmpIndex2 = 1;
        float simKernel = 1;

        bool centerOfRotationDataToggle = true;
        float distanceThreshold = 1;

        bool clusterDataToggle = false;
        float similarityKernel = 0.5f;
        float similarityThreshold = 0.05f;

        private void OnEnable()
        {
            rendererProperty = serializedObject.FindProperty("builtInRenderer");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject(targetAs), typeof(RenderChunk), false);
            EditorGUILayout.ObjectField("Editor Script", MonoScript.FromScriptableObject(this), typeof(RenderChunkEditor), false);

            EditorGUI.EndDisabledGroup();

            serializedObject.Update();

            EditorGUILayout.Space();

            RenderChunk chunk = targetAs;
            
            EditorGUILayout.PropertyField(rendererProperty);

            SkinnedMeshRenderer renderer = rendererProperty.objectReferenceValue as SkinnedMeshRenderer;

            if (renderer != null)
            {
                bool isAsset = AssetDatabase.Contains(renderer);

                if (isAsset)
                {
                    EditorGUILayout.ObjectField("Shared Mesh", renderer.sharedMesh, typeof(UnityEngine.Mesh), false);
                }
                else
                {
                    EditorGUILayout.HelpBox(string.Format("{0} not exist in AssetDatabase. Reselect renderer asset in model data.", renderer.gameObject.name), MessageType.Error);
                }
            }
            
            serializedObject.ApplyModifiedProperties();

            Mesh mesh = renderer != null? renderer.sharedMesh: null;

            EditorGUI.BeginDisabledGroup(mesh == null);

            EditorGUILayout.Space();

            originDataToggle = EditorGUILayout.Foldout(originDataToggle, "Origin Data");

            if (originDataToggle)
            {
                EditorGUILayout.Space();

                EditorGUI.indentLevel++;

                if (mesh != null)
                {
                    EditorGUILayout.LabelField("Bones", renderer.bones != null ? renderer.bones.Length.ToString() : "null");
                    EditorGUILayout.LabelField("Vertex Count", mesh.vertexCount.ToString());
                    EditorGUILayout.LabelField("Index Count", mesh.triangles.Length.ToString());
                    EditorGUILayout.LabelField("Bone Weight Count", mesh.boneWeights != null ? mesh.boneWeights.Length.ToString() : "null");
                }
                else
                {
                    EditorGUILayout.HelpBox("Mesh is null.. please insert data above", MessageType.Error);
                }

                EditorGUI.indentLevel--;

                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.Space();

            convertedDataToggle = EditorGUILayout.Foldout(convertedDataToggle, "Converted Data");

            if (convertedDataToggle)
            {
                EditorGUILayout.Space();

                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Root Bone Name", chunk.rootBoneName != null ? chunk.rootBoneName : "null");

                EditorGUI.BeginDisabledGroup(chunk.rootBoneName == null);

                if (GUILayout.Button("Clear"))
                {
                    Undo.RecordObject(chunk, "Root Bone Name Clear");

                    chunk.rootBoneName = null;

                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Each Bones Data", chunk.inverseRestPoseMatrixArray != null ? chunk.inverseRestPoseMatrixArray.Length.ToString() : "null");

                EditorGUI.BeginDisabledGroup(chunk.inverseRestPoseMatrixArray == null);

                if (GUILayout.Button("Clear"))
                {
                    Undo.RecordObject(chunk, "Bone Transform Data Clear");

                    chunk.indexedBoneNameArray = null;

                    chunk.inverseRestPoseMatrixArray = null;
                    chunk.inverseRestPoseDQArray = null;
                    chunk.inverseRestPoseRotationArray = null;

                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Vertex Count", chunk.meshData != null ? chunk.meshData.Length.ToString() : "null");

                EditorGUI.BeginDisabledGroup(chunk.meshData == null);

                if (GUILayout.Button("Clear"))
                {
                    Undo.RecordObject(chunk, "Mesh Data Clear");

                    chunk.meshData = null;

                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField("1 Bone", chunk.boneVertexCount.x.ToString());
                EditorGUILayout.LabelField("2 Bone", chunk.boneVertexCount.y.ToString());
                EditorGUILayout.LabelField("3 Bone", chunk.boneVertexCount.z.ToString());
                EditorGUILayout.LabelField("4 Bone", chunk.boneVertexCount.w.ToString());

                EditorGUI.indentLevel--;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Index Count", chunk.indices != null ? chunk.indices.Length.ToString() : "null");

                EditorGUI.BeginDisabledGroup(chunk.indices == null);

                if (GUILayout.Button("Clear"))
                {
                    Undo.RecordObject(chunk, "Index Clear");

                    chunk.indices = null;

                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

                EditorGUI.BeginDisabledGroup(mesh == null);

                if (GUILayout.Button(addTitle))
                {
                    Undo.RecordObject(chunk, addTitle);

                    AddMeshData();

                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                }

                if (GUILayout.Button(overrideTitle))
                {
                    Undo.RecordObject(chunk, overrideTitle);

                    OverrideMeshData();

                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                }

                if (GUILayout.Button(fillTitle))
                {
                    Undo.RecordObject(chunk, fillTitle);

                    FillMeshData();

                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("Clear Converted Mesh Data"))
                {
                    Undo.RecordObject(chunk, "Clear Converted Mesh Data");

                    targetAs.Clear();

                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                }

                EditorGUILayout.Space();

                centerOfRotationDataToggle = EditorGUILayout.Foldout(centerOfRotationDataToggle, "Center Of Rotation");

                if (centerOfRotationDataToggle)
                {
                    EditorGUILayout.Space();

                    EditorGUI.indentLevel++;

                    SerializedProperty clusterArrayProperty = serializedObject.FindProperty("clusterArray");
                    EditorGUILayout.PropertyField(clusterArrayProperty, true);

                    EditorGUILayout.LabelField("Clustered Vertex Index Count", chunk.clusteredVertexIndexArray != null ? chunk.clusteredVertexIndexArray.Length.ToString() : "null");
                    EditorGUILayout.LabelField("Clustered Index Index Count", chunk.clusteredTriangleIndexArray != null ? chunk.clusteredTriangleIndexArray.Length.ToString() : "null");

                    EditorGUILayout.Space();

                    distanceThreshold = EditorGUILayout.DelayedFloatField("Distance Threshold", distanceThreshold);

                    EditorGUILayout.Space();

                    if (GUILayout.Button("Calculate Center Of Cluster"))
                    {
                        Undo.RecordObject(chunk, "Calculate Center Of Cluster");

                        IEnumerator<int> enumer = chunk.CalculateCluster(distanceThreshold);
                        DateTime startTime = DateTime.Now;

                        while (enumer.MoveNext())
                            EditorUtility.DisplayProgressBar("Calculate Center Of Cluster", enumer.Current.ToString(), (float)enumer.Current / chunk.vertexCount);

                        EditorUtility.ClearProgressBar();

                        Debug.Log(String.Format("Time : {0}", ((double)(DateTime.Now - startTime).Ticks / TimeSpan.TicksPerSecond)));

                        EditorUtility.SetDirty(target);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                    }

                    EditorGUILayout.Space();

                    SerializedProperty calculateThreadNumberProperty = serializedObject.FindProperty("calculateThreadNumber");
                    EditorGUILayout.PropertyField(calculateThreadNumberProperty);

                    SerializedProperty similarityKernelProperty = serializedObject.FindProperty("similarityKernel");
                    EditorGUILayout.PropertyField(similarityKernelProperty);

                    SerializedProperty similarityThresholdProperty = serializedObject.FindProperty("similarityThreshold");
                    EditorGUILayout.PropertyField(similarityThresholdProperty);

                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Center Of Rotation", chunk.centerOfRotationPositionArray != null ? chunk.centerOfRotationPositionArray.Length.ToString() : "null");

                    EditorGUILayout.Space();

                    if (GUILayout.Button("Log vertices"))
                    {
                        System.Text.StringBuilder builder = new System.Text.StringBuilder();

                        Array.ForEach(chunk.centerOfRotationPositionArray, (cor) => { builder.Append(cor.ToString("F4")).Append("\n"); });

                        Debug.Log(builder.ToString());
                    }
                    
                    if (GUILayout.Button("Calculate Center Of Rotation Position"))
                    {
                        Undo.RecordObject(chunk, "Calculate Center Of Rotation Position");

                        RenderChunkHandler.CoRProcessThreadState[] stateArray = chunk.CalculateCenterOfRotation(chunk.calculateThreadNumber, similarityKernel, similarityThreshold);

                        if (stateArray != null)
                        {
                            while (!Array.TrueForAll(stateArray, (state) => state.done || state.fail))
                            {
                                int sumedProcessCount = 0;
                                for (int i = 0; i < stateArray.Length; i++)
                                    sumedProcessCount += stateArray[i].processCount;
                                
                                EditorUtility.DisplayProgressBar(
                                        "Calculate Center Of Rotation Position", 
                                        String.Format("Processed vertex number : {0}", sumedProcessCount), 
                                        (float)sumedProcessCount / chunk.meshData.Length
                                    );
                            }

                            int emptyCount = 0;
                            for (int i = 0; i < stateArray.Length; i++)
                                emptyCount += stateArray[i].emptyCount;

                            Debug.LogFormat("Calculated similarity count : {0}, empty similarity count : {1}", chunk.meshData.Length - emptyCount, emptyCount);

                            EditorUtility.ClearProgressBar();
                            
                            EditorUtility.SetDirty(target);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                        }
                    }
                    
                    EditorGUI.indentLevel--;
                }

                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.Space();

            logSimilarityToggle = EditorGUILayout.Foldout(logSimilarityToggle, "Log Similarity and Weight Distance");

            if (logSimilarityToggle)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.Space();

                cmpIndex1 = EditorGUILayout.IntSlider("Compare Index 1", cmpIndex1, 0, chunk.meshData.Length - 1);
                cmpIndex2 = EditorGUILayout.IntSlider("Compare Index 2", cmpIndex2, 0, chunk.meshData.Length - 1);
                simKernel = EditorGUILayout.FloatField("Similarity Kernel", simKernel);

                EditorGUILayout.Space();

                EditorGUILayout.LabelField
                (
                    "Vertex Weight 1",
                    String.Format
                    (
                        "{0}, {1}",
                        chunk.meshData[cmpIndex1].index, chunk.meshData[cmpIndex1].weight
                    )
                );
                EditorGUILayout.LabelField
                (
                    "Vertex Weight 2",
                    String.Format
                    (
                        "{0}, {1}",
                        chunk.meshData[cmpIndex2].index, chunk.meshData[cmpIndex2].weight
                    )
                );

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Similarity", chunk.GetSimliarity(cmpIndex1, cmpIndex2, simKernel).ToString());
                EditorGUILayout.LabelField("Weight distance", chunk.meshData[cmpIndex1].GetWeightDistance(chunk.meshData[cmpIndex2]).ToString());

                EditorGUILayout.Space();

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        public void AddMeshData()
        {
            long time = DateTime.Now.Ticks;

            try
            {
                RenderChunk chunk = targetAs;

                Mesh mesh = chunk.builtInRenderer.sharedMesh;

                mesh.RecalculateNormals();

                EditorUtility.DisplayProgressBar(addTitle, "Get Bone Data", 0.0f);
                chunk.builtInRenderer.SetBoneData(chunk);

                EditorUtility.DisplayProgressBar(addTitle, "Get Index Data", 0.25f);
                mesh.SetIndices(chunk);

                EditorUtility.DisplayProgressBar(addTitle, "Get Mesh Data", 0.5f);

                if (EditorUtility.DisplayDialog(addTitle, "override mesh.vertcies, mesh.triangles to RenderChunk.MeshData\nthis process has been long time, are sure this process?", "Yes", "No"))
                    mesh.SetMeshData(chunk);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                float measuredTime = (float)((DateTime.Now.Ticks - time) / TimeSpan.TicksPerMillisecond) / 1000;
                Debug.Log("Measured time : " + measuredTime);
            }

            EditorUtility.ClearProgressBar();
        }

        public void OverrideMeshData()
        {
            long time = DateTime.Now.Ticks;

            try
            {
                RenderChunk chunk = targetAs;

                Mesh mesh = chunk.builtInRenderer.sharedMesh;

                mesh.RecalculateNormals();

                EditorUtility.DisplayProgressBar(addTitle, "Get Bone Data", 0.0f);
                chunk.builtInRenderer.SetBoneData(chunk);

                EditorUtility.DisplayProgressBar(overrideTitle, "Get Index Data", 0.25f);
                mesh.SetIndices(chunk);

                EditorUtility.DisplayProgressBar(overrideTitle, "Get Mesh Data", 0.5f);
                mesh.SetMeshData(chunk);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                float measuredTime = (float)((DateTime.Now.Ticks - time) / TimeSpan.TicksPerMillisecond) / 1000;
                Debug.Log("Measured time : " + measuredTime);
            }

            EditorUtility.ClearProgressBar();
        }

        public void FillMeshData()
        {
            long time = DateTime.Now.Ticks;

            try
            {
                RenderChunk chunk = targetAs;
                Mesh mesh = chunk.builtInRenderer.sharedMesh;

                EditorUtility.DisplayProgressBar(addTitle, "Get Bone Data", 0.0f);
                chunk.builtInRenderer.SetBoneData(chunk);

                if (chunk.indices == null || chunk.indices.Length != mesh.triangles.Length)
                {
                    EditorUtility.DisplayProgressBar(fillTitle, "Get Index Data", 0.25f);

                    mesh.SetIndices(chunk);
                }

                if (chunk.meshData == null || chunk.meshData.Length != mesh.vertexCount)
                    if (EditorUtility.DisplayDialog(fillTitle, "override mesh.vertcies, mesh.triangles to RenderChunk.MeshData\nthis process has been long time, are sure this process?", "Yes", "No"))
                    {
                        EditorUtility.DisplayProgressBar(fillTitle, "Get Mesh Data", 0.5f);

                        mesh.SetMeshData(chunk);
                    }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                float measuredTime = (float)((DateTime.Now.Ticks - time) / TimeSpan.TicksPerMillisecond) / 1000;
                Debug.Log("Measured time : " + measuredTime);
            }

            EditorUtility.ClearProgressBar();
        }
    }
}