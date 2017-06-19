namespace Example.VariousSkinning
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public interface IRenderer
    {
        void Update();
        void OnRenderObject();
    }

    public class DQSkinner : MonoBehaviour
    {
        public RenderChunk chunk;
        public RuntimeRenderChunk chunk2;

        public IRenderer realRenderer;

        void Awake()
        {
            
            if (SystemInfo.supportsComputeShaders)
            {
                realRenderer = new ComputeShaderRenderer();
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

        public class ComputeShaderRenderer : IRenderer
        {
            public void OnRenderObject()
            {
            }

            public void Update()
            {
            }
        }
    }
}
