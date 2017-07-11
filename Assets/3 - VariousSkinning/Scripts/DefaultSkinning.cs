namespace Example.VariousSkinning
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

    public class DefaultSkinningAdapter : IRenderAdapter
    {
        public SkinningMethod method;

        public ICompute compute;
        public IRenderer renderer;

        public DefaultSkinningAdapter(SkinningMethod method, RenderChunk chunk, Transform[] bones, Material material)
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
