namespace Example.VariousSkinning
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public class ComputeShaderRenderer : IRenderer
    {
        /* 
         * Mesh 데이터 래핑
         */
        public RenderChunk chunk;

        /*
         * 스키닝 데이터 계산 용 데이터
         * meshDataBuffer 는 참조 용도
         * 실제로 그릴때 쓰는 데이터는 meshDataStream
         */
        public ComputeShader computeShader;

        public ComputeBuffer boneCurrentPoseMatrixBuffer;
        public ComputeBuffer boneRestPoseMatrixBuffer;

        public ComputeBuffer boneWeightPerVertexBuffer;

        public ComputeBuffer meshDataBuffer;

        /*
         * 스키닝 계산용 버퍼 및 캐시
         */
        public Transform[] bones;
        public Matrix4x4[] boneMatrixArray;

        /*
         * 쉐이더에서 그릴때 쓰는 데이터들
         * 인덱스 버퍼, 서브메쉬 구별용 인덱스 범위 데이터 버퍼(텍스쳐 인덱스)
         */
        public Material material;

        public ComputeBuffer indexBuffer;
        public ComputeBuffer indexCountBuffer;

        /*
         * Skinning 연산 -> 그릴 때 참조하는 mesh data
         */
        public ComputeBuffer meshDataStream;

        public int kernelIndex;

        public ComputeShaderRenderer(RenderChunk chunk, RuntimeRenderChunk chunk2, Material material)
        {
            this.chunk = chunk;
            this.material = material;

            computeShader = chunk2.computeShader;
            kernelIndex = computeShader.FindKernel("TransfromVertex");

            bones = chunk2.bones;
            boneMatrixArray = new Matrix4x4[bones.Length];

            boneWeightPerVertexBuffer = new ComputeBuffer(chunk.vertexCount, Marshal.SizeOf(typeof(BoneWeight)));
            boneWeightPerVertexBuffer.SetData(chunk.boneWeights);

            boneRestPoseMatrixBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(Matrix4x4)));
            boneRestPoseMatrixBuffer.SetData(chunk2.restPoseBoneInverseMatrix);

            boneCurrentPoseMatrixBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(Matrix4x4)));

            meshDataBuffer = new ComputeBuffer(chunk.vertexCount, Marshal.SizeOf(typeof(MeshDataInfo)));
            meshDataBuffer.SetData(chunk.meshData);

            indexBuffer = new ComputeBuffer(chunk.indices.Length, sizeof(int));
            indexBuffer.SetData(chunk.indices);

            indexCountBuffer = new ComputeBuffer(chunk.indexCounts.Length, sizeof(int));
            indexCountBuffer.SetData(chunk.indexCounts);

            meshDataStream = new ComputeBuffer(chunk.vertexCount, Marshal.SizeOf(typeof(MeshDataInfo)));

            computeShader.SetInt("vertexCount", chunk.vertexCount);

            computeShader.SetBuffer(kernelIndex, "currentPoseMatrixBuffer", boneCurrentPoseMatrixBuffer);
            computeShader.SetBuffer(kernelIndex, "restPoseMatrixBuffer", boneRestPoseMatrixBuffer);

            computeShader.SetBuffer(kernelIndex, "meshBuffer", meshDataBuffer);
            computeShader.SetBuffer(kernelIndex, "boneInfoBuffer", boneWeightPerVertexBuffer);

            computeShader.SetBuffer(kernelIndex, "meshStream", meshDataStream);
        }

        public void OnRenderObject()
        {
            material.SetPass(0);

            material.SetBuffer("triangles", indexBuffer);
            material.SetBuffer("triCountPerTextureIndex", indexCountBuffer);
            material.SetBuffer("vertices", meshDataStream);

            Graphics.DrawProcedural(MeshTopology.Triangles, indexBuffer.count);
        }

        public void Update()
        {
            for (int i = 0; i < boneMatrixArray.Length; i++)
                boneMatrixArray[i] = bones[i].localToWorldMatrix;

            boneCurrentPoseMatrixBuffer.SetData(boneMatrixArray);

            computeShader.Dispatch(kernelIndex, chunk.vertexCount / 512 + 1, 1, 1);
        }
    }
}