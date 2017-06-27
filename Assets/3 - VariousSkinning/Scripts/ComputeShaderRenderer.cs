namespace Example.VariousSkinning
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public static class SkinningComputeFactoray
    {
        public static ICompute CreateComputeBy(SkinningMethod method, RenderChunk chunk, RuntimeRenderChunk runtimeChunk, Func<ComputeBuffer> getMeshDataBuffer, Func<ComputeBuffer> getMeshDataStream)
        {
            switch (method)
            {
                case SkinningMethod.LinearBlend:
                    return new LinearBlendSkinningCompute(chunk, runtimeChunk, getMeshDataBuffer, getMeshDataStream);
                case SkinningMethod.DualQuaternion:
                    return new DualQuaternionBlendSkinningCompute(chunk, runtimeChunk, getMeshDataBuffer, getMeshDataStream);
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// ComputeShader based skinning adapter
    /// </summary>
    public class ComputeShaderAdapter : IUpdate, IRenderer
    {
        public SkinningMethod method;

        public ICompute compute;
        public IRenderer renderer;

        public ComputeBuffer meshDataBuffer;
        public ComputeBuffer meshDataStream;

        public ComputeShaderAdapter(SkinningMethod method, RenderChunk chunk, RuntimeRenderChunk runtimeChunk, Material material)
        {
            this.method = method;

            meshDataBuffer = new ComputeBuffer(chunk.vertexCount, Marshal.SizeOf(typeof(MeshDataInfo)));
            meshDataBuffer.SetData(chunk.meshData);

            meshDataStream = new ComputeBuffer(chunk.vertexCount, Marshal.SizeOf(typeof(MeshDataInfo)));
            meshDataStream.SetData(chunk.meshData);

            compute = SkinningComputeFactoray.CreateComputeBy(method, chunk, runtimeChunk, () => { return meshDataBuffer; }, () => { return meshDataStream; });
            renderer = new ComputeShaderRenderer(chunk, material, () => { return Input.GetKey(KeyCode.Space) ? meshDataBuffer : meshDataStream; });
        }

        public void Update()
        {
            compute.Compute();
        }

        public void OnRenderObject()
        {
            renderer.OnRenderObject();
        }
    }

    public interface ICompute
    {
        void Compute();
    }

    /// <summary>
    /// Compute realtime skinning.
    /// This class implement LinearBlendSkinning, called LBS.
    /// 
    /// This class implementation is depend on ComputeShader.
    /// if you change computeShader source, Have to change this class implementation.
    /// </summary>
    public class LinearBlendSkinningCompute : ICompute
    {
        /*
         * boneWeightPerVertexBuffer : source bone index, weight from UnityEngine.Mesh
         * 
         * boneCurrentPoseMatrixBuffer : current pose transformation matrix from UnityEngine.Transform
         * boneRestPoseMatrixBuffer : rest pose inverse transformation matrix from RuntimeRenderChunk.restPoseBoneInverseMatrix
         * 
         * getMeshDataBuffer : get data from outside, source vetices, normals, uvs(compatibility for renderer)
         * getMeshDataStream : get data from outside, converted vertices, normals, uvs(compatibility for renderer)
         */
        public int vertexCount;

        public uint maxThreadSizeX;
        public uint maxThreadSizeY;
        public uint maxThreadSizeZ;

        public int kernelIndex;
        public ComputeShader computeShader;

        public ComputeBuffer boneCurrentPoseMatrixBuffer;
        public ComputeBuffer boneRestPoseMatrixBuffer;

        public ComputeBuffer boneWeightPerVertexBuffer;

        /*
         * data caching..
         */
        public Transform[] bones;
        public Matrix4x4[] currentPoseMatrixArray;

        public LinearBlendSkinningCompute(RenderChunk chunk, RuntimeRenderChunk runtimeChunk, Func<ComputeBuffer> getMeshDataBuffer, Func<ComputeBuffer> getMeshDataStream)
        {
            computeShader = runtimeChunk.computeShader;
            kernelIndex = computeShader.FindKernel("LinearBlendCompute");
            computeShader.GetKernelThreadGroupSizes(kernelIndex, out maxThreadSizeX, out maxThreadSizeY, out maxThreadSizeZ);

            vertexCount = chunk.vertexCount;

            bones = runtimeChunk.bones;
            currentPoseMatrixArray = new Matrix4x4[bones.Length];

            boneRestPoseMatrixBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(Matrix4x4)));
            boneCurrentPoseMatrixBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(Matrix4x4)));
            boneWeightPerVertexBuffer = new ComputeBuffer(chunk.vertexCount, Marshal.SizeOf(typeof(CustomBoneWeight)));

            boneRestPoseMatrixBuffer.SetData(runtimeChunk.restPoseBoneInverseMatrix);
            boneWeightPerVertexBuffer.SetData(chunk.boneWeights);

            computeShader.SetInt("vertexCount", vertexCount);

            computeShader.SetBuffer(kernelIndex, "currentPoseMatrixBuffer", boneCurrentPoseMatrixBuffer);
            computeShader.SetBuffer(kernelIndex, "restPoseMatrixBuffer", boneRestPoseMatrixBuffer);

            computeShader.SetBuffer(kernelIndex, "boneInfoBuffer", boneWeightPerVertexBuffer);

            computeShader.SetBuffer(kernelIndex, "meshBuffer", getMeshDataBuffer());
            computeShader.SetBuffer(kernelIndex, "meshStream", getMeshDataStream());
        }

        public void Compute()
        {
            for (int i = 0; i < currentPoseMatrixArray.Length; i++)
                currentPoseMatrixArray[i] = bones[i].localToWorldMatrix;

            boneCurrentPoseMatrixBuffer.SetData(currentPoseMatrixArray);

            computeShader.Dispatch(kernelIndex, (int)(vertexCount / (long)maxThreadSizeX + 1), 1, 1);
        }
    }

    /// <summary>
    /// dual quaternion data class
    /// </summary>
    public struct DualQuaternion
    {
        public Quaternion rotation;
        public Quaternion translate;
    }

    /// <summary>
    /// dual quaternion translation extension 
    /// </summary>
    public static class DQExtension
    {
        public static DualQuaternion GetLocalToWorldDQ(this Transform transform)
        {
            DualQuaternion dq = new DualQuaternion();
            Vector3 pos = transform.position;
            Quaternion quat = transform.rotation;

            dq.rotation = quat;

            dq.translate[0] = -0.5f * (pos[0] * quat[1] + pos[1] * quat[2] + pos[2] * quat[3]);
            dq.translate[1] = 0.5f * (pos[0] * quat[0] + pos[1] * quat[3] - pos[2] * quat[2]);
            dq.translate[2] = 0.5f * (-pos[0] * quat[3] + pos[1] * quat[0] + pos[2] * quat[1]);
            dq.translate[3] = 0.5f * (pos[0] * quat[2] - pos[1] * quat[1] + pos[2] * quat[0]);

            return dq;
        }

        public static DualQuaternion GetWorldToLocalDQ(this Transform transform)
        {
            DualQuaternion dq = new DualQuaternion();
            return dq;
        }
    }

    /// <summary>
    /// Compute realtime skinning.
    /// This class will implement DualQuaternionBlendSkinning, called DQS, DQBS.
    /// 
    /// This class implementation is depend on ComputeShader.
    /// if you change computeShader source, Have to change this class implementation.
    /// </summary>
    public class DualQuaternionBlendSkinningCompute : ICompute
    {
        /*
         * boneWeightPerVertexBuffer : source bone index, weight from UnityEngine.Mesh
         * 
         * boneCurrentPoseMatrixBuffer : current pose transformation matrix from UnityEngine.Transform
         * boneRestPoseMatrixBuffer : rest pose inverse transformation matrix from RuntimeRenderChunk.restPoseBoneInverseMatrix
         * 
         * getMeshDataBuffer : get data from outside, source vetices, normals, uvs(compatibility for renderer)
         * getMeshDataStream : get data from outside, converted vertices, normals, uvs(compatibility for renderer)
         */
        public int vertexCount;

        public uint maxThreadSizeX;
        public uint maxThreadSizeY;
        public uint maxThreadSizeZ;

        public int kernelIndex;
        public ComputeShader computeShader;

        public ComputeBuffer boneCurrentPoseDQBuffer;
        public ComputeBuffer boneRestPoseDQBuffer;

        public ComputeBuffer boneWeightPerVertexBuffer;

        /*
         * data caching..
         */
        public Transform[] bones;
        public DualQuaternion[] currentPoseDQArray;

        public DualQuaternionBlendSkinningCompute(RenderChunk chunk, RuntimeRenderChunk runtimeChunk, Func<ComputeBuffer> getMeshDataBuffer, Func<ComputeBuffer> getMeshDataStream)
        {
            computeShader = runtimeChunk.computeShader;
            kernelIndex = computeShader.FindKernel("DualQuaternionBlendCompute");
            computeShader.GetKernelThreadGroupSizes(kernelIndex, out maxThreadSizeX, out maxThreadSizeY, out maxThreadSizeZ);

            vertexCount = chunk.vertexCount;

            bones = runtimeChunk.bones;
            currentPoseDQArray = new DualQuaternion[bones.Length];

            boneRestPoseDQBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(DualQuaternion)));
            boneCurrentPoseDQBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(DualQuaternion)));
            boneWeightPerVertexBuffer = new ComputeBuffer(chunk.vertexCount, Marshal.SizeOf(typeof(CustomBoneWeight)));
            
            boneRestPoseDQBuffer.SetData(runtimeChunk.restPoseBoneInverseDQ);
            boneWeightPerVertexBuffer.SetData(chunk.boneWeights);

            computeShader.SetInt("vertexCount", vertexCount);

            computeShader.SetBuffer(kernelIndex, "currentPoseDQBuffer", boneCurrentPoseDQBuffer);
            computeShader.SetBuffer(kernelIndex, "restPoseMatrixBuffer", boneRestPoseDQBuffer);
            computeShader.SetBuffer(kernelIndex, "boneInfoBuffer", boneWeightPerVertexBuffer);

            computeShader.SetBuffer(kernelIndex, "meshBuffer", getMeshDataBuffer());
            computeShader.SetBuffer(kernelIndex, "meshStream", getMeshDataStream());
        }

        public void Compute()
        {
            for (int i = 0; i < currentPoseDQArray.Length; i++)
            {
                currentPoseDQArray[i] = bones[i].GetLocalToWorldDQ();
            }
            
            boneCurrentPoseDQBuffer.SetData(currentPoseDQArray);

            computeShader.Dispatch(kernelIndex, (int)(vertexCount / (long)maxThreadSizeX + 1), 1, 1);
        }
    }

    /// <summary>
    /// Render skinned data
    /// 
    /// This class implementation is depend on shader which noticed material.
    /// if you change shader source, Have to change this class implementation.
    /// </summary>
    public class ComputeShaderRenderer : IRenderer
    {
        /*
         * indexBuffer : Index Buffer from UnityEngine.Mesh
         * indexCountBuffer : end index which is each SubMesh in indexArray for texture index(Texture2DArray)
         * 
         * getMeshDataStream : get data from outside, converted vertices, normals, uvs
         */
        public Material material;

        public ComputeBuffer indexBuffer;
        public ComputeBuffer indexCountBuffer;

        public Func<ComputeBuffer> getMeshDataStream;

        public ComputeShaderRenderer(RenderChunk chunk, Material material, Func<ComputeBuffer> getMeshDataStream)
        {
            this.material = material;

            indexBuffer = new ComputeBuffer(chunk.indices.Length, sizeof(int));
            indexCountBuffer = new ComputeBuffer(chunk.indexCounts.Length, sizeof(int));

            indexBuffer.SetData(chunk.indices);
            indexCountBuffer.SetData(chunk.indexCounts);

            this.getMeshDataStream = getMeshDataStream;
        }

        public void OnRenderObject()
        {
            material.SetPass(0);

            material.SetBuffer("triangles", indexBuffer);
            material.SetBuffer("triCountPerTextureIndex", indexCountBuffer);
            material.SetBuffer("vertices", getMeshDataStream());

            Graphics.DrawProcedural(MeshTopology.Triangles, indexBuffer.count);
        }
    }
}