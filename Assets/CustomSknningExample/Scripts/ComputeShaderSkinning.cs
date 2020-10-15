namespace CustomSkinningExample
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using UnityEngine;
    using UnityEngine.Rendering;

    public interface IDispatch { void Dispatch(); }
    public interface IDisposableDispatch : IDispatch, IDisposable { }

    public static class DispatcherFactory
    {
        public static IDisposableDispatch CreateComputeBy(
            SkinningBlend method, ComputeShader computeShader, RenderChunk chunk, Transform[] bones, 
            Func<ComputeBuffer> getPerVertexBuffer, Func<ComputeBuffer> getPerVertexSkinBuffer, Func<ComputeBuffer> getPerVertexStream, 
            Func<ComputeBuffer> getMinMaxBoundsBuffer
            )
        {
            switch (method)
            {
                case SkinningBlend.Linear:
                    return new LinearBlendSkinningDispatcher(computeShader, chunk, bones, getPerVertexBuffer, getPerVertexSkinBuffer, getPerVertexStream, getMinMaxBoundsBuffer);
                case SkinningBlend.DualQuaternion:
                    return new DualQuaternionBlendSkinningDispatcher(computeShader, chunk, bones, getPerVertexBuffer, getPerVertexSkinBuffer, getPerVertexStream, getMinMaxBoundsBuffer);
                case SkinningBlend.OptimizedCenterOfRotation:
                    return new OptimizedCenterOfRotationSkinningDispatcher(computeShader, chunk, bones, getPerVertexBuffer, getPerVertexSkinBuffer, getPerVertexStream, getMinMaxBoundsBuffer);
                default:
                    return null;
            }
        }
    }

    public class ComputeShaderSkinningAdapter : IRenderAdapter, IDisposable
    {
        public SkinningBlend method;

        public IDispatch sourceDispatcher;
        public IDisposableDispatch dispatchcer;
        public IDisposableRenderer renderer;

        public ComputeBuffer perVertexBuffer;
        public ComputeBuffer perVertexSkinBuffer;
        public ComputeBuffer perVertexStream;

        public GraphicsBuffer indexBuffer;
        public ComputeBuffer indexCountBuffer;

        public ComputeBuffer minmaxBoundsBuffer;

        public IDispatch tensionDispatcher;

        public ComputeBuffer edgeLengthBuffer;
        public ComputeBuffer tensionBuffer;

        public ComputeShaderSkinningAdapter(SkinningBlend method, ComputeShader computeShader, RenderChunk chunk, Transform[] bones, Material material, bool tension = false)
        {
            this.method = method;

            perVertexBuffer = new ComputeBuffer(chunk.vertexCount, Marshal.SizeOf(typeof(DataPerVertex)));
            perVertexBuffer.SetData(chunk.dataPerVertex);

            perVertexSkinBuffer = new ComputeBuffer(chunk.vertexCount, Marshal.SizeOf(typeof(SkinPerVertex)));
            perVertexSkinBuffer.SetData(chunk.skinPerVertex);

            perVertexStream = new ComputeBuffer(chunk.vertexCount, Marshal.SizeOf(typeof(DataPerVertex)));
            perVertexStream.SetData(chunk.dataPerVertex);

            indexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, chunk.indices.Length, sizeof(int));
            indexBuffer.SetData(chunk.indices);

            indexCountBuffer = new ComputeBuffer(chunk.indexCounts.Length, sizeof(int));
            indexCountBuffer.SetData(chunk.indexCounts);

            minmaxBoundsBuffer = new ComputeBuffer(1, Marshal.SizeOf<Vector3>() * 2, ComputeBufferType.Structured);

            sourceDispatcher = new DataToDataDispatcher(
                                            computeShader, 
                                            () => perVertexBuffer, 
                                            () => perVertexStream,
                                            () => minmaxBoundsBuffer
                                            );
            dispatchcer = DispatcherFactory.CreateComputeBy(
                                            method, 
                                            computeShader, 
                                            chunk, 
                                            bones, 
                                            () => perVertexBuffer, 
                                            () => perVertexSkinBuffer, 
                                            () => perVertexStream,
                                            () => minmaxBoundsBuffer
                                            );

            if (!tension)
            {
                renderer = 
                    new ComputeShaderRenderer(
                        chunk, material, () => perVertexStream, () => indexBuffer, 
                        () => 
                        {
                            Bounds b = new Bounds();
                            // TODO:: REMOVE GC ALLOCATION, fixed or stack allocation as c# Array
                            Vector3[] p = new Vector3[2];
                            minmaxBoundsBuffer.GetData(p);
                            b.center = (p[0] + p[1]) / 2.0f;
                            b.extents = (p[1] - p[0]) / 2.0f;

                            return b;
                        }
                    );
            }
            else
            {
                edgeLengthBuffer = new ComputeBuffer(chunk.edges.Length, sizeof(float));
                edgeLengthBuffer.SetData(chunk.edges);

                tensionBuffer = new ComputeBuffer(chunk.vertexCount, sizeof(float));
                tensionBuffer.SetData(new float[chunk.vertexCount]);

                tensionDispatcher = 
                    new TensionDispatcher(
                        computeShader, chunk, 
                        () => perVertexStream, () => indexBuffer, () => edgeLengthBuffer, () => tensionBuffer
                    );
                renderer = new TensionRenderer(
                        chunk, material, () => tensionBuffer, () => perVertexStream, () => indexBuffer,
                        () =>
                        {
                            Bounds b = new Bounds();
                            // TODO:: REMOVE GC ALLOCATION, fixed or stack allocation as c# Array
                            Vector3[] p = new Vector3[2];
                            minmaxBoundsBuffer.GetData(p);
                            b.center = (p[0] + p[1]) / 2.0f;
                            b.extents = (p[1] - p[0]) / 2.0f;

                            return b;
                        }
                    );
            }
        }
        
        public void Update()
        {
            if (Input.GetKey(KeyCode.Space))
            {
                sourceDispatcher.Dispatch();
            }
            else
                dispatchcer.Dispatch();

            if (tensionDispatcher != null)
                tensionDispatcher.Dispatch();
        }

        public void OnRenderObject()
        {
            renderer.OnRenderObject();
        }

        public void Dispose()
        {
            perVertexBuffer.Dispose();
            perVertexSkinBuffer.Dispose();
            perVertexStream.Dispose();

            indexBuffer.Dispose();
            indexCountBuffer.Dispose();

            if (edgeLengthBuffer != null)
                edgeLengthBuffer.Dispose();
            if (tensionBuffer != null)
                tensionBuffer.Dispose();

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
        
        public DataToDataDispatcher(ComputeShader computeShader, Func<ComputeBuffer> getPerVertexBuffer, Func<ComputeBuffer> getPerVertexStream, Func<ComputeBuffer> getMinMaxBoundsBuffer)
        {
            uint maxThreadSizeY;
            uint maxThreadSizeZ;

            this.computeShader = computeShader;

            kernelIndex = computeShader.FindKernel("DataToDataCompute");
            computeShader.GetKernelThreadGroupSizes(kernelIndex, out maxThreadSizeX, out maxThreadSizeY, out maxThreadSizeZ);

            ComputeBuffer buffer = getPerVertexBuffer();

            vertexCount = buffer.count;

            computeShader.SetInt("vertexCount", vertexCount);
            computeShader.SetBuffer(kernelIndex, "vertices", buffer);
            computeShader.SetBuffer(kernelIndex, "output", getPerVertexStream());
            computeShader.SetBuffer(kernelIndex, "minmax", getMinMaxBoundsBuffer());
        }

        public void Dispatch()
        {
            computeShader.Dispatch(kernelIndex, (int)(vertexCount / (long)maxThreadSizeX + 1), 1, 1);
        }
    }

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

        public LinearBlendSkinningDispatcher(
            ComputeShader computeShader, RenderChunk chunk, Transform[] bones, 
            Func<ComputeBuffer> getPerVertexBuffer, Func<ComputeBuffer> getPerVertexSkinBuffer, Func<ComputeBuffer> getPerVertexStream,
            Func<ComputeBuffer> getMinMaxBoundsBuffer
            )
        {
            this.computeShader = computeShader;
            kernelIndex = computeShader.FindKernel("LinearBlendCompute");
            computeShader.GetKernelThreadGroupSizes(kernelIndex, out maxThreadSizeX, out maxThreadSizeY, out maxThreadSizeZ);

            vertexCount = chunk.vertexCount;
             
            this.bones = bones;
            currentPoseMatrixArray = new Matrix4x4[bones.Length];

            boneRestPoseMatrixBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(Matrix4x4)));
            boneRestPoseMatrixBuffer.SetData(chunk.inverseRestPoseMatrixArray);

            boneCurrentPoseMatrixBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(Matrix4x4)));
            for (int i = 0; i < currentPoseMatrixArray.Length; i++)
                currentPoseMatrixArray[i] = bones[i].localToWorldMatrix;
            boneCurrentPoseMatrixBuffer.SetData(currentPoseMatrixArray);

            computeShader.SetInt("vertexCount", vertexCount);

            computeShader.SetBuffer(kernelIndex, "currPoseMtrx", boneCurrentPoseMatrixBuffer);
            computeShader.SetBuffer(kernelIndex, "restPoseMtrx", boneRestPoseMatrixBuffer);

            computeShader.SetBuffer(kernelIndex, "vertices", getPerVertexBuffer());
            computeShader.SetBuffer(kernelIndex, "skin", getPerVertexSkinBuffer());
            computeShader.SetBuffer(kernelIndex, "output", getPerVertexStream());
            computeShader.SetBuffer(kernelIndex, "minmax", getMinMaxBoundsBuffer());
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

        public DualQuaternionBlendSkinningDispatcher(
            ComputeShader computeShader, RenderChunk chunk, Transform[] bones, 
            Func<ComputeBuffer> getPerVertexBuffer, Func<ComputeBuffer> getPerVertexSkinBuffer, Func<ComputeBuffer> getPerVertexStream,
            Func<ComputeBuffer> getMinMaxBoundsBuffer
            )
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

            computeShader.SetBuffer(kernelIndex, "currPoseDQ", boneCurrentPoseDQBuffer);
            computeShader.SetBuffer(kernelIndex, "restPoseDQ", boneRestPoseDQBuffer);

            computeShader.SetBuffer(kernelIndex, "vertices", getPerVertexBuffer());
            computeShader.SetBuffer(kernelIndex, "skin", getPerVertexSkinBuffer());
            computeShader.SetBuffer(kernelIndex, "output", getPerVertexStream());
            computeShader.SetBuffer(kernelIndex, "minmax", getMinMaxBoundsBuffer());
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
    
    public class OptimizedCenterOfRotationSkinningDispatcher : IDisposableDispatch
    {
        public int vertexCount;
        public uint maxThreadSizeX;

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

        public OptimizedCenterOfRotationSkinningDispatcher(
            ComputeShader computeShader, RenderChunk chunk, Transform[] bones, 
            Func<ComputeBuffer> getPerVertexBuffer, Func<ComputeBuffer> getPerVertexSkinBuffer, Func<ComputeBuffer> getPerVertexStream,
            Func<ComputeBuffer> getMinMaxBoundsBuffer
            )
        {
            uint maxThreadSizeY, maxThreadSizeZ;

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

            computeShader.SetBuffer(kernelIndex, "currPoseMtrx", boneCurrentPoseMatrixBuffer);
            computeShader.SetBuffer(kernelIndex, "restPoseMtrx", boneRestPoseMatrixBuffer);

            computeShader.SetBuffer(kernelIndex, "currPoseRot", boneCurrentPoseRotationQuatBuffer);
            computeShader.SetBuffer(kernelIndex, "restPoseRot", boneRestPoseRotationQuatBuffer);

            computeShader.SetBuffer(kernelIndex, "CORBuffer", vertexCenterOfRotationBuffer);

            computeShader.SetBuffer(kernelIndex, "vertices", getPerVertexBuffer());
            computeShader.SetBuffer(kernelIndex, "skin", getPerVertexSkinBuffer());
            computeShader.SetBuffer(kernelIndex, "output", getPerVertexStream());
            computeShader.SetBuffer(kernelIndex, "minmax", getMinMaxBoundsBuffer());
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

            vertexCenterOfRotationBuffer.Dispose();
        }
    }

    public class TensionDispatcher : IDispatch
    {
        public int patchCount;
        public uint maxThreadSizeX;

        public int kernelIndex;
        public ComputeShader computeShader;

        public Func<ComputeBuffer> getPerVertexStream;
        public Func<GraphicsBuffer> getIndexBuffer;
        public Func<ComputeBuffer> getEdgeLengthBuffer;
        public Func<ComputeBuffer> getTensionPerVertex;

        public TensionDispatcher(ComputeShader computeShader, RenderChunk chunk, Func<ComputeBuffer> getPerVertexStream, Func<GraphicsBuffer> getIndexBuffer,  Func<ComputeBuffer> getEdgeLengthBuffer, Func<ComputeBuffer> getTensionPerVertex)
        {
            this.computeShader = computeShader;

            kernelIndex = computeShader.FindKernel("TensionCompute");

            uint maxThreadSizeY, maxThreadSizeZ;
            computeShader.GetKernelThreadGroupSizes(kernelIndex, out maxThreadSizeX, out maxThreadSizeY, out maxThreadSizeZ);

            this.getPerVertexStream = getPerVertexStream;
            this.getIndexBuffer = getIndexBuffer;
            this.getEdgeLengthBuffer = getEdgeLengthBuffer;
            this.getTensionPerVertex = getTensionPerVertex;

            patchCount = chunk.indices.Length / chunk.topologyCount;
        }

        public void Dispatch()
        {
            computeShader.SetInt("patchCount", patchCount);

            computeShader.SetBuffer(kernelIndex, "deformVertices", getPerVertexStream());
            computeShader.SetBuffer(kernelIndex, "indexBuffer", getIndexBuffer());
            computeShader.SetBuffer(kernelIndex, "edgeLengthBuffer", getEdgeLengthBuffer());
            computeShader.SetBuffer(kernelIndex, "tensionPerVertex", getTensionPerVertex());

            computeShader.Dispatch(kernelIndex, (int)(patchCount / maxThreadSizeX) + 1, 1, 1);
        }
    }

    public class ComputeShaderRenderer : IDisposableRenderer
    {
        public Material material;

        public Func<GraphicsBuffer> getIndexBuffer;
        public Func<ComputeBuffer> getPerVertexStream;
        public Func<Bounds> getBounds;

        public ComputeShaderRenderer(
            RenderChunk chunk, Material material, 
            Func<ComputeBuffer> getPerVertexStream, Func<GraphicsBuffer> getIndexBuffer, Func<Bounds> getBounds
            )
        {
            this.material = material;

            this.getIndexBuffer = getIndexBuffer;
            this.getPerVertexStream = getPerVertexStream;
            this.getBounds = getBounds;

            material.SetBuffer("dataPerVertex", getPerVertexStream());
        }

        public void Dispose()
        {
        }

        public void OnRenderObject()
        {
            material.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Triangles, getIndexBuffer(), getIndexBuffer().count);
        }
    }

    public class TensionRenderer : IDisposableRenderer
    {
        public Material material;

        public Func<GraphicsBuffer> getIndexBuffer;
        public Func<ComputeBuffer> getPerVertexStream;
        public Func<ComputeBuffer> getTensionBuffer;
        public Func<Bounds> getBounds;

        public TensionRenderer(
            RenderChunk chunk, Material material, 
            Func<ComputeBuffer> getTensionBuffer, Func<ComputeBuffer> getPerVertexStream, Func<GraphicsBuffer> getIndexBuffer, Func<Bounds> getBounds
            )
        {
            this.material = material;

            this.getPerVertexStream = getPerVertexStream;
            this.getTensionBuffer = getTensionBuffer;

            this.getIndexBuffer = getIndexBuffer;
            this.getBounds = getBounds;

            material.SetBuffer("dataPerVertex", getPerVertexStream());
            material.SetBuffer("tension", getTensionBuffer());
        }

        public void Dispose()
        {
        }

        public void OnRenderObject()
        {
            material.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Triangles, getIndexBuffer(), getIndexBuffer().count);
        }
    }
}
