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

        /*
         * Test variable
         */
        bool logSimilarityToggle;
        int cmpIndex1 = 0, cmpIndex2 = 1;
        float simKernel = 1;

        private void OnEnable()
        {
            rendererProperty = serializedObject.FindProperty("builtInRenderer");
        }

        public override void OnInspectorGUI()
        {
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

            EditorGUILayout.LabelField("Origin data");
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

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Converted Data");
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Root Bone Name", chunk.rootBoneName != null ? chunk.rootBoneName : "null");

            EditorGUI.BeginDisabledGroup(chunk.rootBoneName == null);

            if (GUILayout.Button("Clear"))
            {
                chunk.rootBoneName = null;

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Indexed Bone Name Array", chunk.indexedBoneNameArray != null ? chunk.indexedBoneNameArray.Length.ToString() : "null");

            EditorGUI.BeginDisabledGroup(chunk.indexedBoneNameArray == null);

            if (GUILayout.Button("Clear"))
            {
                chunk.indexedBoneNameArray = null;

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bones Matrix", chunk.inverseRestPoseMatrixArray != null ? chunk.inverseRestPoseMatrixArray.Length.ToString() : "null");

            EditorGUI.BeginDisabledGroup(chunk.inverseRestPoseMatrixArray == null);

            if (GUILayout.Button("Clear"))
            {
                chunk.inverseRestPoseMatrixArray = null;

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bones DQ", chunk.inverseRestPoseDQArray != null ? chunk.inverseRestPoseDQArray.Length.ToString() : "null");

            EditorGUI.BeginDisabledGroup(chunk.inverseRestPoseDQArray == null);

            if (GUILayout.Button("Clear"))
            {
                chunk.inverseRestPoseDQArray = null;

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Vertex Count", chunk.meshData != null? chunk.meshData.Length.ToString(): "null");

            EditorGUI.BeginDisabledGroup(chunk.meshData == null);

            if (GUILayout.Button("Clear"))
            {
                chunk.meshData = null;

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("1 bone vertex count", chunk.boneVertexCount.x.ToString());
            EditorGUILayout.LabelField("2 bone vertex count", chunk.boneVertexCount.y.ToString());
            EditorGUILayout.LabelField("3 bone vertex count", chunk.boneVertexCount.z.ToString());
            EditorGUILayout.LabelField("4 bone vertex count", chunk.boneVertexCount.w.ToString());

            EditorGUI.indentLevel--;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Index Count", chunk.indices != null ? chunk.indices.Length.ToString() : "null");

            EditorGUI.BeginDisabledGroup(chunk.indices == null);

            if (GUILayout.Button("Clear"))
            {
                chunk.indices = null;

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Each bone vertex count");
            EditorGUILayout.LabelField(string.Format("1: {0}. 2 : {1}. 3 : {2}. 4 : {3}", chunk.boneVertexCount.x, chunk.boneVertexCount.y, chunk.boneVertexCount.z, chunk.boneVertexCount.w));

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(mesh == null);
            
            if (GUILayout.Button(addTitle))
            {
                AddData();

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
            }

            if (GUILayout.Button(overrideTitle))
            {
                OverrideData();

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
            }

            if (GUILayout.Button(fillTitle))
            {
                FillData();

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Clear Data"))
            {
                targetAs.Clear();

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
            }

            EditorGUI.EndDisabledGroup();

            logSimilarityToggle = EditorGUILayout.Foldout(logSimilarityToggle, "Log similarity");

            if (logSimilarityToggle)
            {
                EditorGUI.indentLevel++;

                cmpIndex1 = EditorGUILayout.IntSlider("Compare Index 1", cmpIndex1, 0, chunk.meshData.Length - 1);
                cmpIndex2 = EditorGUILayout.IntSlider("Compare Index 2", cmpIndex2, 0, chunk.meshData.Length - 1);
                simKernel = EditorGUILayout.FloatField("Similarity Kernel", simKernel);

                EditorGUILayout.Space();

                if (GUILayout.Button("log calculate"))
                {
                    Debug.Log(chunk.meshData[cmpIndex1] + ", " + chunk.meshData[cmpIndex2]);
                    Debug.Log(chunk.GetSimliarity(cmpIndex1, cmpIndex2, simKernel));
                }

                EditorGUI.indentLevel--;
            }
        }

        public void AddData()
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

        public void OverrideData()
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

        public void FillData()
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