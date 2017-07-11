namespace Example.VariousSkinning
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// JUST CREATE
    /// </summary>

    public static class VTFSkinningComputerFactory
    {
        public static ICompute CreateComputeBy(SkinningMethod method, RenderChunk chunk)
        {
            switch (method)
            {
                case SkinningMethod.Linear:
                    return new LinearBlendSkinningCompute();
                case SkinningMethod.DualQuaternion:
                    return new DualQuaternionBlendSkinningCompute();
                default:
                    return null;
            }
        }
    }

    public class VTFSkinningAdapter : IRenderAdapter
    {
        public SkinningMethod method;

        public ICompute compute;
        public IRenderer renderer;

        public VTFSkinningAdapter(SkinningMethod method, RenderChunk chunk, Material material)
        {
        }

        public void Update()
        {
        }

        public void OnRenderObject()
        {
        }
    }

    public class VTFSkinningRenderer
    {
    }
}
