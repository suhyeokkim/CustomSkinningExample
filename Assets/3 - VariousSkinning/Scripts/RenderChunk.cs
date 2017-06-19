namespace Example.VariousSkinning
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu]
    public class RenderChunk : ScriptableObject
    {
        [Header("Input data for skinning")]
        public Mesh mesh;
        public Material[] materials;
    }

    [System.Serializable]
    public struct RuntimeRenderChunk
    {
        public Transform rootBone;
        public ComputeShader computeShader;
    }
}