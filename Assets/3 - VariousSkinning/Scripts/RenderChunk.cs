namespace Example.VariousSkinning
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu]
    public class RenderChunk : ScriptableObject
    {
        public Mesh mesh;
        public Material[] materials;
    }

    [System.Serializable]
    public struct RuntimeRenderChunk
    {
        public ComputeShader computeShader;

        public Transform rootBone;
        [NonSerialized]
        public Transform[] bones;
    }
}