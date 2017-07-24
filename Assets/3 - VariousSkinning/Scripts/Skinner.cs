namespace Example.VariousSkinning
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using UnityEngine;

    public interface IRenderer { void OnRenderObject(); }
    public interface IUpdate { void Update(); }

    public interface IRenderAdapter : IUpdate, IRenderer { }

    public interface IDisposableRenderer : IDisposable, IRenderer { }

    public enum SkinningMethod
    {
        Linear,
        DualQuaternion,
        OptimizedCenterOfRotation,
    }

    public enum SkinningSelector
    {
        Auto,
        ForceGPGPU,
        ForceCPU,
    }

    public class Skinner : MonoBehaviour
    {
        public SkinningMethod method;
        public SkinningSelector selector;

        public RenderChunk chunk;

        public ComputeShader computeShader;
        public Material material;

        public IRenderAdapter adapter;
        public IDisposable disposable;

        void Awake()
        {
            SelectSkinning(selector);
        }

        public void SelectSkinning(SkinningSelector selector)
        {
            switch (selector)
            {
                case SkinningSelector.Auto:
                    AutoSelectSkinning();
                    break;
                case SkinningSelector.ForceGPGPU:
                    {
                        ComputeShaderSkinningAdapter computeAdapter = new ComputeShaderSkinningAdapter(method, computeShader, chunk, chunk.GetBones(transform.parent), material);

                        adapter = computeAdapter as IRenderAdapter;
                        disposable = computeAdapter as IDisposable;
                    }
                    break;
                case SkinningSelector.ForceCPU:
                    {
                        DefaultSkinningAdapter defaultAdapter = new DefaultSkinningAdapter(method, chunk, chunk.GetBones(transform.parent), material);

                        adapter = defaultAdapter as IRenderAdapter;
                        disposable = defaultAdapter as IDisposable;
                    }
                    break;
            }
        }

        public void AutoSelectSkinning()
        {
            if (SystemInfo.supportsComputeShaders)
            {
                ComputeShaderSkinningAdapter computeAdapter = new ComputeShaderSkinningAdapter(method, computeShader, chunk, chunk.GetBones(transform.parent), material);

                adapter = computeAdapter as IRenderAdapter;
                disposable = computeAdapter as IDisposable;
            }
            else // last fallback
            {
                DefaultSkinningAdapter defaultAdapter = new DefaultSkinningAdapter(method, chunk, chunk.GetBones(transform.parent), material);

                adapter = defaultAdapter as IRenderAdapter;
                disposable = defaultAdapter as IDisposable;
            }
        }

        private void OnRenderObject()
        {
            adapter.OnRenderObject();
        }

        private void Update()
        {
            adapter.Update();
        }

        private void OnDestroy()
        {
            if (disposable != null) disposable.Dispose();
        }
    }
}
