namespace Example.VariousSkinning
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public struct MeshDataInfo
    {
        public Vector4 position;
        public Vector4 normal;
        public Vector2 uv;

        public static int ByteLength
        {
            get
            {
                return System.Runtime.InteropServices.Marshal.SizeOf(new MeshDataInfo());
            }
        }
    };

    [System.Serializable]
    public struct CustomBoneWeight
    {
        public float weight0;
        public float weight1;
        public float weight2;
        public float weight3;
        public int boneIndex0;
        public int boneIndex1;
        public int boneIndex2;
        public int boneIndex3;

        public CustomBoneWeight(BoneWeight weight)
        {
            weight0 = weight.weight0;
            weight1 = weight.weight1;
            weight2 = weight.weight2;
            weight3 = weight.weight3;

            boneIndex0 = weight.boneIndex0;
            boneIndex1 = weight.boneIndex1;
            boneIndex2 = weight.boneIndex2;
            boneIndex3 = weight.boneIndex3;
        }

    }

    [CreateAssetMenu]
    public class RenderChunk : ScriptableObject
    {
#if UNITY_EDITOR
        public Mesh mesh;
#endif

        public int vertexCount;
        public MeshDataInfo[] meshData;
        public int[] indices;
        public uint[] indexCounts;
        public CustomBoneWeight[] boneWeights;
    }

    [System.Serializable]
    public struct RuntimeRenderChunk
    {
        public ComputeShader computeShader;

        [NonSerialized]
        public Transform[] bones;
        [NonSerialized]
        public Matrix4x4[] restPoseBoneInverseMatrix;

        public void SetBoneInverseTransform()
        {
            if (bones == null)
            {
                Debug.LogError("Need bones transforms..");
                return;
            }
            
            restPoseBoneInverseMatrix = new Matrix4x4[bones.Length];

            for (int i = 0; i < restPoseBoneInverseMatrix.Length; i++)
                restPoseBoneInverseMatrix[i] = bones[i].worldToLocalMatrix;
        }
    }
}