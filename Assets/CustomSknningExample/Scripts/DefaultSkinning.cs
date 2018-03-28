namespace CustomSkinningExample
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// JUST CREATE
    /// </summary>

    public interface ICompute { void Compute(); }

    public static class DefaultSkinningFactoray
    {
        public static ICompute CreateComputeBy(SkinningBlend method, RenderChunk chunk)
        {
            switch (method)
            {
                case SkinningBlend.Linear:
                    return new LinearBlendSkinningCompute();
                case SkinningBlend.DualQuaternion:
                    return new DualQuaternionBlendSkinningCompute();
                default:
                    return null;
            }
        }
    }

    public class DefaultSkinningAdapter : IRenderAdapter
    {
        public SkinningBlend method;

        public ICompute compute;
        public IRenderer renderer;

        public DefaultSkinningAdapter(SkinningBlend method, RenderChunk chunk, Transform[] bones, Material material)
        {
        }

        public void Update()
        {
        }

        public void OnRenderObject()
        {
        }
    }

    public class LinearBlendSkinningCompute : ICompute
    {
        public void Compute()
        {
        }
    }

    public class DualQuaternionBlendSkinningCompute : ICompute
    {
        public void Compute()
        {
        }
    }

    public class DefaultSkinningRenderer
    {
    }
}
