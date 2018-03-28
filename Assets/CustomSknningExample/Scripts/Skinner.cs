namespace CustomSkinningExample
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

    public enum SkinningBlend
    {
        Linear,
        DualQuaternion,
        OptimizedCenterOfRotation,
    }

    public enum SkinningCompute
    {
        Auto,
        ForceGPGPU,
        ForceCPU,
    }

    public class Skinner : MonoBehaviour
    {
        [Header("Select process type")] [Space]
        [SerializeField]
        private SkinningBlend blend;
        [SerializeField]
        private SkinningCompute compute;
        [SerializeField]
        private bool tension;

        public SkinningBlend skinningBlend { get { return blend; } set { blend = value; ReloadAdaptor(compute, blend, tension); } }
        public SkinningCompute skinningCompute { get { return compute; } set { compute = value; ReloadAdaptor(compute, blend, tension); } }
        public bool calculateTension { get { return tension; } set { tension = value; ReloadAdaptor(compute, blend, tension); } }

        [Header("Render data")] [Space]
        public RenderChunk chunk;
        public ComputeShader computeShader;
        public Material material;

        [Header("Debug")] [Space]
        [SerializeField]
        private bool tensionDebug;

        public bool debugTension { get { return tensionDebug; } set { tensionDebug = value; if (tensionDebug) Shader.EnableKeyword("TENSION_DEBUG"); else Shader.DisableKeyword("TENSION_DEBUG"); } }

        public bool validate { get { bool check = chunk != null && computeShader != null && material != null; if (check && adapter == null) LoadAdaptor(compute, blend, tension); return check; } }

        public IRenderAdapter adapter;
        public IDisposable disposable;

        public void LoadAdaptor(SkinningCompute compute, SkinningBlend blend, bool tension)
        {
            switch (compute)
            {
                case SkinningCompute.Auto:
                    if (SystemInfo.supportsComputeShaders)
                        LoadAdaptor(SkinningCompute.ForceGPGPU, blend, tension);
                    else
                        LoadAdaptor(SkinningCompute.ForceCPU, blend, tension);
                    break;
                case SkinningCompute.ForceGPGPU:
                    {
                        ComputeShaderSkinningAdapter computeAdapter = new ComputeShaderSkinningAdapter(blend, computeShader, chunk, chunk.GetBones(transform.parent), material, tension);

                        adapter = computeAdapter as IRenderAdapter;
                        disposable = computeAdapter as IDisposable;
                    }
                    break;
                case SkinningCompute.ForceCPU:
                    {
                        DefaultSkinningAdapter defaultAdapter = new DefaultSkinningAdapter(blend, chunk, chunk.GetBones(transform.parent), material);

                        adapter = defaultAdapter as IRenderAdapter;
                        disposable = defaultAdapter as IDisposable;
                    }
                    break;
            }
        }

        public void ReloadAdaptor(SkinningCompute compute, SkinningBlend blend, bool tension)
        {
            if (disposable != null)
                disposable.Dispose();

            adapter = null;
            disposable = null;

            LoadAdaptor(compute, blend, tension);
        }

        public void RefreshDebug()
        {
            if (tensionDebug)
                Shader.EnableKeyword("TENSION_DEBUG");
            else
                Shader.DisableKeyword("TENSION_DEBUG");
        }

        #region Unity Messages

        private void OnEnable()
        {
            LoadAdaptor(compute, blend, tension);
        }

        private void OnDisable()
        {
            if (validate)
            {
                if (disposable != null) disposable.Dispose();
            }
        }

        private void OnRenderObject()
        {
            if (validate)
            {
                adapter.OnRenderObject();
            }
        }

        private void Update()
        {
            if (validate)
            {
                adapter.Update();
            }
        }

        #endregion
    }
}
