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

    public class Skinner : MonoBehaviour
    {
        public RenderChunk chunk;
        public RuntimeRenderChunk chunk2;

        public IRenderer realRenderer;

        void Awake()
        {
            chunk2.bones = new Transform[1];
            GetBones(chunk2.rootBone, 0, ref chunk2.bones);

            if (SystemInfo.supportsComputeShaders)
            {
                realRenderer = new ComputeShaderRenderer(chunk, chunk2);
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

        private int GetBones(Transform bone, int boneIndex, ref Transform[] boneArray)
        {
            boneArray[boneIndex++] = bone;

            if (bone.childCount > 0)
            {
                Array.Resize(ref boneArray, boneArray.Length + bone.childCount);

                for (int i = 0; i < bone.childCount; i++)
                {
                    boneIndex = GetBones(bone.GetChild(i), boneIndex, ref boneArray);
                }
            }
            
            return boneIndex;
        }

        public class ComputeShaderRenderer : IRenderer
        {
            public RenderChunk chunk;
            public RuntimeRenderChunk chunk2;

            public ComputeShaderRenderer(RenderChunk chunk, RuntimeRenderChunk chunk2)
            {
                this.chunk = chunk;
                this.chunk2 = chunk2;
            }

            public void OnRenderObject()
            {
                for (int i = 0; i < chunk.materials.Length; i++)
                {
                    Material material = chunk.materials[i];
                    material.SetPass(i);
                }

                Graphics.DrawProceduralIndirect(MeshTopology.Triangles, null);
            }

            public void Update()
            {
            }
        }
    }
}
