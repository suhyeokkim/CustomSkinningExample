namespace Example.VariousSkinning
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public static class SkinningComputeFactoray
    {
        public static IDisposableCompute CreateComputeBy(SkinningMethod method, RenderChunk chunk, RuntimeRenderChunk runtimeChunk, Func<ComputeBuffer> getMeshDataBuffer, Func<ComputeBuffer> getMeshDataStream)
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
    public class ComputeShaderAdapter : IUpdate, IRenderer, IDisposable
    {
        public SkinningMethod method;

        public IDisposableCompute compute;
        public IDisposableRenderer renderer;

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

        public void Dispose()
        {
            meshDataBuffer.Dispose();
            meshDataStream.Dispose();

            compute.Dispose();
            renderer.Dispose();
        }
    }

    public interface ICompute : IDisposable { void Compute(); }
    public interface IDisposableCompute : ICompute, IDisposable { }

    /// <summary>
    /// Compute realtime skinning.
    /// This class implement LinearBlendSkinning, called LBS.
    /// 
    /// This class implementation is depend on ComputeShader.
    /// if you change computeShader source, Have to change this class implementation.
    /// 
    /// boneWeightPerVertexBuffer : source bone index, weight from UnityEngine.Mesh
    /// boneCurrentPoseMatrixBuffer : current pose transformation matrix from UnityEngine.Transform
    /// boneRestPoseMatrixBuffer : rest pose inverse transformation matrix from RuntimeRenderChunk.restPoseBoneInverseMatrix
    /// getMeshDataBuffer : get data from outside, source vetices, normals, uvs(compatibility for renderer)
    /// getMeshDataStream : get data from outside, converted vertices, normals, uvs(compatibility for renderer)
    /// </summary>
    public class LinearBlendSkinningCompute : IDisposableCompute
    {
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

        public void Dispose()
        {
            boneCurrentPoseMatrixBuffer.Dispose();
            boneRestPoseMatrixBuffer.Dispose();
            boneWeightPerVertexBuffer.Dispose();
        }
    }

    public static class QuaternionExtension
    {
        public static Quaternion AddQuaternion(Quaternion q1, Quaternion q2)
        {
            return new Quaternion(q1.x + q2.x, q1.y + q2.y, q1.z + q2.z, q1.w + q2.w);
        }
        public static Quaternion Multiply(Quaternion q, float f)
        {
            return new Quaternion(q.x * f, q.y * f, q.z * f, q.w * f);
        }
    }

    /// <summary>
    /// dual quaternion data class
    /// </summary>
    public struct DualQuaternion
    {
        public Quaternion real;
        public Quaternion dual;

        public static DualQuaternion identity = new DualQuaternion(Quaternion.identity, Vector3.zero);

        public DualQuaternion(Quaternion real, Quaternion dual)
        {
            this.real = real;
            this.dual = dual;
        }

        public DualQuaternion(Quaternion rotation, Vector3 position)
        {
            real = rotation;

            dual = (rotation * new Quaternion(position.x * 0.5f, position.y * 0.5f, position.z * 0.5f, 0));
        }
            
        public static DualQuaternion operator *(DualQuaternion dq1, DualQuaternion dq2)
        {
            // WIP
            return new DualQuaternion(dq1.real * dq2.real, QuaternionExtension.AddQuaternion(dq1.real * dq2.dual, dq1.dual * dq2.real));
        }

        public static Vector3 operator *(DualQuaternion dq, Vector3 pos)
        {
            Quaternion t = Quaternion.Inverse(dq.real) * new Quaternion(dq.dual.x * 2f, dq.dual.y * 2f, dq.dual.z * 2f, dq.dual.w * 2f);            return dq.real * pos + new Vector3(t.x, t.y, t.z);
        }
    }

    /// <summary>
    /// dual quaternion translation extension 
    /// </summary>
    public static class DQExtension
    {
        // WIP
        public static DualQuaternion GetLocalToWorldDQ(this Transform transform)
        {
            DualQuaternion realDQ = new DualQuaternion();
            Transform iterate = transform;

            while (transform != null)
            {
                Vector3 pos = transform.localPosition;
                Quaternion quat = transform.localRotation;

                DualQuaternion dq = new DualQuaternion(quat, pos);

                if (transform == iterate)
                    realDQ = dq;
                else
                    realDQ = dq * realDQ;

                transform = transform.parent;
            }

            return realDQ;
        }

        // WIP
        public static DualQuaternion GetWorldToLocalDQ(this Transform transform)
        {
            DualQuaternion realDQ = new DualQuaternion();
            Transform iterate = transform;

            while (transform != null)
            {
                Vector3 pos = transform.localPosition * -1;
                Quaternion quat = Quaternion.Inverse(transform.localRotation);

                DualQuaternion dq = new DualQuaternion(quat, pos);

                if (transform == iterate)
                    realDQ = dq;
                else
                    realDQ = realDQ * dq;

                transform = transform.parent;
            }

            return realDQ;
        }
    }


    /// <summary>
    /// Compute realtime skinning.
    /// This class will implement DualQuaternionBlendSkinning, called DQS, DQBS.
    /// 
    /// This class implementation is depend on ComputeShader.
    /// if you change computeShader source, Have to change this class implementation.
    /// 
    /// boneRestPoseDQBuffer : source bone index, weight from UnityEngine.Mesh
    /// boneCurrentPoseDQBuffer : current pose transformation dual quaternion from UnityEngine.Transform
    /// boneRestPoseMatrixBuffer : rest pose inverse transformation matrix from RuntimeRenderChunk.restPoseBoneInverseMatrix
    /// 
    /// getMeshDataBuffer : get data from outside, source vetices, normals, uvs(compatibility for renderer)
    /// getMeshDataStream : get data from outside, converted vertices, normals, uvs(compatibility for renderer)
    /// </summary>
    public class DualQuaternionBlendSkinningCompute : IDisposableCompute
    {
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
            computeShader.SetBuffer(kernelIndex, "restPoseDQBuffer", boneRestPoseDQBuffer);

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

        public void Dispose()
        {
            boneCurrentPoseDQBuffer.Dispose();
            boneRestPoseDQBuffer.Dispose();
            boneWeightPerVertexBuffer.Dispose();
        }
    }

    /// <summary>
    /// Render skinned data
    /// 
    /// This class implementation is depend on shader which noticed material.
    /// if you change shader source, Have to change this class implementation.
    /// 
    /// indexBuffer : Index Buffer from UnityEngine.Mesh
    /// indexCountBuffer : end index which is each SubMesh in indexArray for texture index(Texture2DArray)
    /// 
    /// getMeshDataStream : get data from outside, converted vertices, normals, uvs
    /// </summary>
    public class ComputeShaderRenderer : IDisposableRenderer
    {
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

        public void Dispose()
        {
            indexBuffer.Dispose();
            indexCountBuffer.Dispose();
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