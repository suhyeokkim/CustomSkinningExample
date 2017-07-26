namespace Example.VariousSkinning
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using UnityEngine;

    public interface IDispatch { void Dispatch(); }
    public interface IDisposableDispatch : IDispatch, IDisposable { }

    public static class ComputeShaderSkinningDispatcherFactory
    {
        public static IDisposableDispatch CreateComputeBy(SkinningMethod method, ComputeShader computeShader, RenderChunk chunk, Transform[] bones, Func<ComputeBuffer> getMeshDataBuffer, Func<ComputeBuffer> getMeshDataStream)
        {
            switch (method)
            {
                case SkinningMethod.Linear:
                    return new LinearBlendSkinningDispatcher(computeShader, chunk, bones, getMeshDataBuffer, getMeshDataStream);
                case SkinningMethod.DualQuaternion:
                    return new DualQuaternionBlendSkinningDispatcher(computeShader, chunk, bones, getMeshDataBuffer, getMeshDataStream);
                case SkinningMethod.OptimizedCenterOfRotation:
                    return new OptimizedCenterOfRotationSkinningDispatcher(computeShader, chunk, bones, getMeshDataBuffer, getMeshDataStream);
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// ComputeShader based skinning adapter
    /// </summary>
    public class ComputeShaderSkinningAdapter : IRenderAdapter, IDisposable
    {
        public SkinningMethod method;

        public IDispatch sourceDispatcher;
        public IDisposableDispatch dispatchcer;
        public IDisposableRenderer renderer;

        public ComputeBuffer meshDataBuffer;
        public ComputeBuffer meshDataStream;

        public ComputeShaderSkinningAdapter(SkinningMethod method, ComputeShader computeShader, RenderChunk chunk, Transform[] bones, Material material)
        {
            this.method = method;

            meshDataBuffer = new ComputeBuffer(chunk.vertexCount, Marshal.SizeOf(typeof(MeshDataInfo)));
            meshDataBuffer.SetData(chunk.meshData);

            meshDataStream = new ComputeBuffer(chunk.vertexCount, Marshal.SizeOf(typeof(RenderData)));
            meshDataStream.SetData(chunk.meshData);

            sourceDispatcher = new DataToDataDispatcher(computeShader, () => { return meshDataBuffer; }, () => { return meshDataStream; });
            dispatchcer = ComputeShaderSkinningDispatcherFactory.CreateComputeBy(method, computeShader, chunk, bones, () => { return meshDataBuffer; }, () => { return meshDataStream; });

            renderer = new ComputeShaderRenderer(chunk, material, () => { return meshDataStream; });
        }
        
        public void Update()
        {
            if (Input.GetKey(KeyCode.Space))
            {
                sourceDispatcher.Dispatch();
            }
            else
                dispatchcer.Dispatch();
        }

        public void OnRenderObject()
        {
            renderer.OnRenderObject();
        }

        public void Dispose()
        {
            meshDataBuffer.Dispose();
            meshDataStream.Dispose();

            dispatchcer.Dispose();
            renderer.Dispose();
        }
    }

    public class DataToDataDispatcher : IDispatch
    {
        public ComputeShader computeShader;

        public int vertexCount;
        public int kernelIndex;

        public uint maxThreadSizeX;

        // useless..
        public uint maxThreadSizeY;
        public uint maxThreadSizeZ;

        public DataToDataDispatcher(ComputeShader computeShader, Func<ComputeBuffer> getMeshDataBuffer, Func<ComputeBuffer> getMeshDataStream)
        {
            this.computeShader = computeShader;

            kernelIndex = computeShader.FindKernel("DataToDataCompute");
            computeShader.GetKernelThreadGroupSizes(kernelIndex, out maxThreadSizeX, out maxThreadSizeY, out maxThreadSizeZ);

            ComputeBuffer buffer = getMeshDataBuffer();

            vertexCount = buffer.count;

            computeShader.SetBuffer(kernelIndex, "meshBuffer", buffer);
            computeShader.SetBuffer(kernelIndex, "meshStream", getMeshDataStream());
        }

        public void Dispatch()
        {
            computeShader.Dispatch(kernelIndex, (int)(vertexCount / (long)maxThreadSizeX + 1), 1, 1);
        }
    }

    /// <summary>
    /// Compute realtime skinning.
    /// This class implement LinearBlendSkinning, called LBS.
    /// 
    /// This class implementation is depend on ComputeShader.
    /// if you change computeShader source, Have to change this class implementation.
    /// 
    /// boneCurrentPoseMatrixBuffer : current pose transformation matrix from UnityEngine.Transform
    /// boneRestPoseMatrixBuffer : rest pose inverse transformation matrix from RenderChunk.restPoseBoneInverseMatrix
    /// 
    /// getMeshDataBuffer : get data from outside, source vetices, normals, uvs(compatibility for renderer)
    /// getMeshDataStream : get data from outside, converted vertices, normals, uvs(compatibility for renderer)
    /// </summary>
    public class LinearBlendSkinningDispatcher : IDisposableDispatch
    {
        public int vertexCount;

        public uint maxThreadSizeX;

        // useless..
        public uint maxThreadSizeY;
        public uint maxThreadSizeZ;

        public int kernelIndex;
        public ComputeShader computeShader;

        public ComputeBuffer boneCurrentPoseMatrixBuffer;
        public ComputeBuffer boneRestPoseMatrixBuffer;
        
        // data caching..
        public Transform[] bones;
        public Matrix4x4[] currentPoseMatrixArray;

        public LinearBlendSkinningDispatcher(ComputeShader computeShader, RenderChunk chunk, Transform[] bones, Func<ComputeBuffer> getMeshDataBuffer, Func<ComputeBuffer> getMeshDataStream)
        {
            this.computeShader = computeShader;
            kernelIndex = computeShader.FindKernel("LinearBlendCompute");
            computeShader.GetKernelThreadGroupSizes(kernelIndex, out maxThreadSizeX, out maxThreadSizeY, out maxThreadSizeZ);

            vertexCount = chunk.vertexCount;
             
            this.bones = bones;
            currentPoseMatrixArray = new Matrix4x4[bones.Length];

            boneRestPoseMatrixBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(Matrix4x4)));
            boneCurrentPoseMatrixBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(Matrix4x4)));

            boneRestPoseMatrixBuffer.SetData(chunk.inverseRestPoseMatrixArray);

            computeShader.SetInt("vertexCount", vertexCount);

            computeShader.SetBuffer(kernelIndex, "currentPoseMatrixBuffer", boneCurrentPoseMatrixBuffer);
            computeShader.SetBuffer(kernelIndex, "restPoseMatrixBuffer", boneRestPoseMatrixBuffer);

            computeShader.SetBuffer(kernelIndex, "meshBuffer", getMeshDataBuffer());
            computeShader.SetBuffer(kernelIndex, "meshStream", getMeshDataStream());
        }
        
        public void Dispatch()
        {
            for (int i = 0; i < currentPoseMatrixArray.Length; i++)
                currentPoseMatrixArray[i] = bones[i].localToWorldMatrix;

            boneCurrentPoseMatrixBuffer.SetData(currentPoseMatrixArray);

            computeShader.Dispatch(kernelIndex, (int)(vertexCount / maxThreadSizeX) + 1, 1, 1);
        }

        public void Dispose()
        {
            boneCurrentPoseMatrixBuffer.Dispose();
            boneRestPoseMatrixBuffer.Dispose();
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
    /// 
    /// getMeshDataBuffer : get data from outside, source vetices, normals, uvs(compatibility for renderer)
    /// getMeshDataStream : get data from outside, converted vertices, normals, uvs(compatibility for renderer)
    /// </summary>
    public class DualQuaternionBlendSkinningDispatcher : IDisposableDispatch
    {
        public int vertexCount;

        public uint maxThreadSizeX;

        // useless..
        public uint maxThreadSizeY;
        public uint maxThreadSizeZ;

        public int kernelIndex;
        public ComputeShader computeShader;

        public ComputeBuffer boneCurrentPoseDQBuffer;
        public ComputeBuffer boneRestPoseDQBuffer;
        
        // data caching..
        public Transform[] bones;
        public DualQuaternion[] currentPoseDQArray;

        public DualQuaternionBlendSkinningDispatcher(ComputeShader computeShader, RenderChunk chunk, Transform[] bones, Func<ComputeBuffer> getMeshDataBuffer, Func<ComputeBuffer> getMeshDataStream)
        {
            this.computeShader = computeShader;
            kernelIndex = computeShader.FindKernel("DualQuaternionBlendCompute");
            computeShader.GetKernelThreadGroupSizes(kernelIndex, out maxThreadSizeX, out maxThreadSizeY, out maxThreadSizeZ);

            vertexCount = chunk.vertexCount;

            this.bones = bones;
            currentPoseDQArray = new DualQuaternion[bones.Length];

            boneRestPoseDQBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(DualQuaternion)));
            boneCurrentPoseDQBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(DualQuaternion)));
            
            boneRestPoseDQBuffer.SetData(chunk.inverseRestPoseDQArray);

            computeShader.SetInt("vertexCount", vertexCount);

            computeShader.SetBuffer(kernelIndex, "currentPoseDQBuffer", boneCurrentPoseDQBuffer);
            computeShader.SetBuffer(kernelIndex, "restPoseDQBuffer", boneRestPoseDQBuffer);

            computeShader.SetBuffer(kernelIndex, "meshBuffer", getMeshDataBuffer());
            computeShader.SetBuffer(kernelIndex, "meshStream", getMeshDataStream());
        }

        public void Dispatch()
        {
            for (int i = 0; i < currentPoseDQArray.Length; i++)
                currentPoseDQArray[i] = bones[i].GetLocalToWorldDQ();

            boneCurrentPoseDQBuffer.SetData(currentPoseDQArray);

            computeShader.Dispatch(kernelIndex, (int)(vertexCount / maxThreadSizeX) + 1, 1, 1);
        }

        public void Dispose()
        {
            boneCurrentPoseDQBuffer.Dispose();
            boneRestPoseDQBuffer.Dispose();
        }
    }

    /// <summary>
    /// Compute realtime skinning.
    /// This class will implement Optimized Center Of Rotation Skinning.
    /// 
    /// This class implementation is depend on ComputeShader.
    /// if you change computeShader source, Have to change this class implementation.
    /// 
    /// boneCurrentPoseMatrixBuffer : current pose transformation matrix from UnityEngine.Transform
    /// boneRestPoseMatrixBuffer : rest pose inverse transformation matrix from RenderChunk.restPoseBoneInverseMatrix
    /// 
    /// boneCurrentPoseRotationBuffer : current pose rotation qutaernion from UnityEngine.Transform
    /// boneRestPoseRotationBuffer : rest pose inverse rotation qutaernion from RenderChunk.restPoesBoneInverseRotation
    /// 
    /// vertexCenterOfRotationBuffer : center of rotation data from RenderChunk.centerOfRotationPositionArray
    /// 
    /// getMeshDataBuffer : get data from outside, source vetices, normals, uvs(compatibility for renderer)
    /// getMeshDataStream : get data from outside, converted vertices, normals, uvs(compatibility for renderer)
    /// </summary>
    public class OptimizedCenterOfRotationSkinningDispatcher : IDisposableDispatch
    {
        public int vertexCount;

        public uint maxThreadSizeX;

        // useless..
        public uint maxThreadSizeY;
        public uint maxThreadSizeZ;

        public int kernelIndex;
        public ComputeShader computeShader;

        public Transform[] bones;
        public Matrix4x4[] currentPoseMatrixArray;
        public Quaternion[] currentPoseRotationQuatArray;

        public ComputeBuffer boneCurrentPoseMatrixBuffer;
        public ComputeBuffer boneRestPoseMatrixBuffer;

        public ComputeBuffer boneCurrentPoseRotationQuatBuffer;
        public ComputeBuffer boneRestPoseRotationQuatBuffer;

        public ComputeBuffer vertexCenterOfRotationBuffer;

        public OptimizedCenterOfRotationSkinningDispatcher(ComputeShader computeShader, RenderChunk chunk, Transform[] bones, Func<ComputeBuffer> getMeshDataBuffer, Func<ComputeBuffer> getMeshDataStream)
        {
            this.computeShader = computeShader;
            kernelIndex = computeShader.FindKernel("OptimizedCenterOfRotationCompute");
            computeShader.GetKernelThreadGroupSizes(kernelIndex, out maxThreadSizeX, out maxThreadSizeY, out maxThreadSizeZ);

            vertexCount = chunk.vertexCount;

            this.bones = bones;
            currentPoseMatrixArray = new Matrix4x4[bones.Length];
            currentPoseRotationQuatArray = new Quaternion[bones.Length];

            boneRestPoseMatrixBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(Matrix4x4)));
            boneCurrentPoseMatrixBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(Matrix4x4)));

            boneRestPoseRotationQuatBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(Quaternion)));
            boneCurrentPoseRotationQuatBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(Quaternion)));

            vertexCenterOfRotationBuffer = new ComputeBuffer(chunk.vertexCount, Marshal.SizeOf(typeof(Vector3)));

            boneRestPoseMatrixBuffer.SetData(chunk.inverseRestPoseMatrixArray);
            boneRestPoseRotationQuatBuffer.SetData(chunk.inverseRestPoseRotationArray);

            vertexCenterOfRotationBuffer.SetData(chunk.centerOfRotationPositionArray);

            computeShader.SetInt("vertexCount", vertexCount);

            computeShader.SetBuffer(kernelIndex, "currentPoseMatrixBuffer", boneCurrentPoseMatrixBuffer);
            computeShader.SetBuffer(kernelIndex, "restPoseMatrixBuffer", boneRestPoseMatrixBuffer);

            computeShader.SetBuffer(kernelIndex, "currentPoseRotationBuffer", boneCurrentPoseRotationQuatBuffer);
            computeShader.SetBuffer(kernelIndex, "restPoseRotationBuffer", boneRestPoseRotationQuatBuffer);

            computeShader.SetBuffer(kernelIndex, "centerOfRotationBuffer", vertexCenterOfRotationBuffer);

            computeShader.SetBuffer(kernelIndex, "meshBuffer", getMeshDataBuffer());
            computeShader.SetBuffer(kernelIndex, "meshStream", getMeshDataStream());
        }

        public void Dispatch()
        {
            for (int i = 0; i < currentPoseMatrixArray.Length; i++)
            {
                currentPoseMatrixArray[i] = bones[i].localToWorldMatrix;
                currentPoseRotationQuatArray[i] = bones[i].rotation;
            }

            boneCurrentPoseMatrixBuffer.SetData(currentPoseMatrixArray);
            boneCurrentPoseRotationQuatBuffer.SetData(currentPoseRotationQuatArray);

            computeShader.Dispatch(kernelIndex, (int)(vertexCount / maxThreadSizeX) + 1, 1, 1);
        }

        public void Dispose()
        {
            boneRestPoseMatrixBuffer.Dispose();
            boneCurrentPoseMatrixBuffer.Dispose();

            boneRestPoseRotationQuatBuffer.Dispose();
            boneCurrentPoseRotationQuatBuffer.Dispose();
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
