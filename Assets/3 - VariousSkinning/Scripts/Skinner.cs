namespace Example.VariousSkinning
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public interface IRenderer
    {
        void Update();
        void OnRenderObject();
    }

    public enum SkinningMethod
    {
        LinearBlend,
        DualQuaternion,
    }

    public class Skinner : MonoBehaviour
    {
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public RenderChunk chunk;
        public RuntimeRenderChunk chunk2;
        public Material material;

        public IRenderer realRenderer;

        void Awake()
        {
            chunk2.bones = skinnedMeshRenderer.bones;

            chunk2.SetBoneInverseTransform();

            if (SystemInfo.supportsComputeShaders)
            {
                realRenderer = new ComputeShaderRenderer(chunk, chunk2, material);
            }
        }

        private void OnRenderObject()
        {
            realRenderer.OnRenderObject();
        }

        private void Update()
        {
            realRenderer.Update();
        }
    }
}
