namespace Example.VariousSkinning
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public interface IRenderer
    {
        void OnRenderObject();
    }

    public interface IUpdate
    {
        void Update();
    }

    public enum SkinningMethod
    {
        LinearBlend,
        DualQuaternion,
    }

    public class Skinner : MonoBehaviour
    {
        public SkinningMethod skinning;

        public SkinnedMeshRenderer skinnedMeshRenderer;
        public RenderChunk chunk;
        public RuntimeRenderChunk chunk2;
        public Material material;

        public IRenderer implRenderer;
        public IUpdate implCompute;

        void Awake()
        {
            chunk2.bones = skinnedMeshRenderer.bones;

            chunk2.SetBoneInverseTransform();

            if (SystemInfo.supportsComputeShaders)
            {
                ComputeShaderAdapter adapter = new ComputeShaderAdapter(skinning, chunk, chunk2, material);

                implRenderer = adapter as IRenderer;
                implCompute = adapter as IUpdate;
            }
        }

        private void OnRenderObject()
        {
            implRenderer.OnRenderObject();
        }

        private void Update()
        {
            if (implCompute != null)
                implCompute.Update();
        }
    }
}
