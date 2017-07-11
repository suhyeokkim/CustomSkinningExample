//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(CharacterData))]
//public class CharacterDataEditor : Editor
//{
//    CharacterData targetAs { get { return target as CharacterData; } }

//    Example.TextureArray.Texture2DArrayManager tex2DArrayManager;

//    public override void OnInspectorGUI()
//    {
//        CharacterData data = targetAs;

//        base.OnInspectorGUI();

//        EditorGUILayout.Space();

//        if (GUILayout.Button("Refesh mesh with uv2"))
//        {
//            CleanAndRefresh(
//                (mesh) =>
//                {
//                    Vector3[] vertices = null;
//                    int[] indices = null;

//                    VertexMapper.GetVertices(ref vertices, data.GetBodyPoses(), data.GetBodySizes());
//                    VertexMapper.GetIndices(ref indices);

//                    mesh.vertices = vertices;
//                    mesh.triangles = indices;

//                    Vector2[] uvs = null;
//                    UVMapper.GetUV(ref uvs, data.GetUVPoses(), data.GetUVSizes());
//                    mesh.uv = uvs;

//                    mesh.name = string.Format("{0}Mesh", data.name);
//                }
//                );
//        }


//        EditorGUILayout.Space();

//        tex2DArrayManager = 
//            EditorGUILayout.ObjectField("Tex2DArrayManager", tex2DArrayManager, typeof(Example.TextureArray.Texture2DArrayManager), true) as
//            Example.TextureArray.Texture2DArrayManager;

//        EditorGUILayout.BeginHorizontal();
        
//        if (GUILayout.Button("Refesh mesh with uv3"))
//        {
//            if (tex2DArrayManager == null)
//            {
//                Debug.LogErrorFormat("Cannot find Texture2DArrayManager asset.");
//                return;
//            }

//            int index = Array.FindIndex(tex2DArrayManager.textureInputArray, (otherTex) => otherTex.Equals(data.texture));

//            CleanAndRefresh(
//               (mesh) =>
//               {
//                   Vector3[] vertices = null;
//                   int[] indices = null;

//                   VertexMapper.GetVertices(ref vertices, data.GetBodyPoses(), data.GetBodySizes());
//                   VertexMapper.GetIndices(ref indices);

//                   mesh.vertices = vertices;
//                   mesh.triangles = indices;

//                   Vector3[] uvs = null;
//                   UVMapper.GetUV(ref uvs, data.GetUVPoses(), data.GetUVSizes(), index);
//                   mesh.SetUVs(0, new List<Vector3>(uvs));

//                   mesh.name = string.Format("{0}MeshWithTI", data.name);
//               }
//               );
//        }
        
//        if (GUILayout.Button("Refesh mesh with uv4"))
//        {
//            if (tex2DArrayManager == null)
//            {
//                Debug.LogErrorFormat("Cannot find Texture2DArrayManager asset.");
//                return;
//            }

//            int index = Array.FindIndex(tex2DArrayManager.textureInputArray, (otherTex) => otherTex.Equals(data.texture));

//            CleanAndRefresh(
//               (mesh) =>
//               {
//                   Vector3[] vertices = null;
//                   int[] indices = null;

//                   VertexMapper.GetVertices(ref vertices, data.GetBodyPoses(), data.GetBodySizes());
//                   VertexMapper.GetIndices(ref indices);

//                   mesh.vertices = vertices;
//                   mesh.triangles = indices;

//                   Vector4[] uvs = null;
//                   UVMapper.GetUV(ref uvs, data.GetUVPoses(), data.GetUVSizes(), index);
//                   mesh.SetUVs(0, new List<Vector4>(uvs));

//                   mesh.name = string.Format("{0}MeshWithTI&BI", data.name);
//               }
//               );
//        }

//        EditorGUILayout.EndHorizontal();

//        EditorGUILayout.Space();

//    }

//    public void CleanAndRefresh(Action<Mesh> addData)
//    {
//        CharacterData data = targetAs;

//        if (data.meshData != null)
//            DestroyImmediate(data.meshData, true);

//        Mesh mesh = new Mesh();

//        if (addData != null)
//            addData(mesh);

//        AssetDatabase.AddObjectToAsset(mesh, data);
//        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(data));
//        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

//        data.meshData = mesh;
//    }
//}
