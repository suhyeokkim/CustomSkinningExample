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

            public ComputeShader computeShader;

            public ComputeBuffer vertexBuffer;
            public ComputeBuffer vertexStream;
            public ComputeBuffer boneWeightPerVertexBuffer;

            public ComputeBuffer bonePositionBuffer;
            public ComputeBuffer boneRotationBuffer;
            public ComputeBuffer boneRestPoseMatrixBuffer;

            public ComputeBuffer boneCacluatedBuffer;
            public ComputeBuffer boneTransformMatrixBuffer;

            public Transform[] bones;

            public Vector3[] bonePositionArray;
            public Quaternion[] boneRotationArray;

            public int kernelIndex;
            public int vertexCount;

            public ComputeShaderRenderer(RenderChunk chunk, RuntimeRenderChunk chunk2)
            {
                this.chunk = chunk;

                bones = chunk2.bones;
                computeShader = chunk2.computeShader;

                kernelIndex = computeShader.FindKernel("TransfromVertex");

                bonePositionArray = new Vector3[chunk2.bones.Length];
                boneRotationArray = new Quaternion[chunk2.bones.Length];

                for (int i = 0; i < chunk2.bones.Length; i++)
                {
                    bonePositionArray[i] = chunk2.bones[i].position;
                    boneRotationArray[i] = chunk2.bones[i].rotation;
                }

                Mesh mesh = chunk.mesh;

                vertexBuffer = new ComputeBuffer(mesh.vertices.Length, Marshal.SizeOf(typeof(Vector3)));
                vertexBuffer.GetData(mesh.vertices);

                vertexCount = mesh.vertexCount;

                vertexStream = new ComputeBuffer(mesh.vertices.Length, Marshal.SizeOf(typeof(Vector3)));

                boneWeightPerVertexBuffer = new ComputeBuffer(mesh.boneWeights.Length, Marshal.SizeOf(typeof(BoneWeight)));
                boneWeightPerVertexBuffer.GetData(mesh.boneWeights);

                bonePositionBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(Vector3)));
                bonePositionBuffer.GetData(bonePositionArray);

                boneRotationBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(Quaternion)));
                boneRotationBuffer.GetData(boneRotationArray);

                boneRestPoseMatrixBuffer = new ComputeBuffer(mesh.bindposes.Length, Marshal.SizeOf(typeof(Matrix4x4)));
                boneRestPoseMatrixBuffer.GetData(mesh.bindposes);

                boneCacluatedBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(bool)));
                boneTransformMatrixBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(Matrix4x4)));

                computeShader.SetInt("vertexCount", vertexCount);

                computeShader.SetBuffer(kernelIndex, "bonePositionBuffer", bonePositionBuffer);
                computeShader.SetBuffer(kernelIndex, "boneRotationBuffer", boneRotationBuffer);

                computeShader.SetBuffer(kernelIndex, "calculatedMatrix", boneCacluatedBuffer);
                computeShader.SetBuffer(kernelIndex, "boneMatrixBuffer", boneTransformMatrixBuffer);

                computeShader.SetBuffer(kernelIndex, "vertexBuffer", vertexBuffer);
                computeShader.SetBuffer(kernelIndex, "vertexStream", vertexStream);
                computeShader.SetBuffer(kernelIndex, "boneInfoBuffer", boneWeightPerVertexBuffer);
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
                bonePositionBuffer.SetData(bonePositionArray);
                boneRotationBuffer.SetData(boneRotationArray);

                computeShader.Dispatch(kernelIndex, vertexCount / 512 + 1, 1, 1);
            }
        }
    }
}
