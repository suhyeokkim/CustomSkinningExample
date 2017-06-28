namespace Example.VariousSkinning
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
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

    public static class Matrix4x4Extension
    {
        public static void Multiply(this Matrix4x4 matrix, float scalar)
        {
            for (int i = 0; i < 16; i++)
                matrix[i] *= scalar;
        }

        public static Vector3 ToTranslate(this Matrix4x4 matrix)
        {
            return new Vector3(matrix[0, 3], matrix[1, 3], matrix[2, 3]);
        }

        public static Quaternion ToRotation(this Matrix4x4 matrix)
        {
            Quaternion q = new Quaternion();
            q.w = Mathf.Sqrt(1.0f + matrix.m00 + matrix.m11 + matrix.m22) / 2.0f;
            float w4 = (4.0f * q.w);
            q.x = (matrix.m21 - matrix.m12) / w4;
            q.y = (matrix.m02 - matrix.m20) / w4;
            q.z = (matrix.m10 - matrix.m01) / w4;

            return q;
        }

        public static DualQuaternion ToDQ(this Matrix4x4 matrix)
        {
            DualQuaternion dq = DualQuaternion.identity;
            return new DualQuaternion(matrix.ToRotation(), matrix.ToTranslate());
        }
    }

    public static class QuaternionExtension
    {
        public static Quaternion AddQuaternion(Quaternion q1, Quaternion q2)
        {
            return new Quaternion(q1.x + q2.x, q1.y + q2.y, q1.z + q2.z, q1.w + q2.w);
        }

        public static Quaternion Normalize(Quaternion q)
        {
            float len = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);

            q.x /= len;
            q.y /= len;
            q.z /= len;
            q.w /= len;

            return q;
        }

        public static Quaternion Multiply(this Quaternion q, float s)
        {
            q.x *= s;
            q.y *= s;
            q.z *= s;
            q.w *= s;
            return q;
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
            dual = new Quaternion(position.x, position.y, position.z, 0) * rotation;

            dual.x = dual.x / 2f;
            dual.y = dual.y / 2f;
            dual.z = dual.z / 2f;
            dual.w = dual.w / 2f;
        }

        public Quaternion rotation { get { return real; } }
        public Vector3 translate
        {
            get
            {
                Quaternion t = new Quaternion(dual.x * 2f, dual.y * 2f, dual.z * 2f, dual.w * 2f) * (Quaternion.Inverse(real));
                return new Vector3(t.x, t.y, t.z);
            }
        }
        public DualQuaternion inverse { get { return Inverse(this); } }

        public static DualQuaternion Inverse(DualQuaternion dq)
        {
            return new DualQuaternion(Quaternion.Inverse(dq.real), Quaternion.Inverse(dq.dual));
        }

        public Matrix4x4 ToMatrix()
        {
            Matrix4x4 matrix = Matrix4x4.identity;
            float
                xx = real.x * real.x, xy = real.x * real.y, xz = real.x * real.z, xw = real.x * real.w,
                yy = real.y * real.y, yz = real.y * real.z, yw = real.y * real.w,
                zz = real.z * real.z, zw = real.z * real.w;

            matrix[0, 0] = 1 - 2 * yy - 2 * zz;
            matrix[0, 1] = 2 * xy - 2 * zw;
            matrix[0, 2] = 2 * xz + 2 * yw;

            matrix[1, 0] = 2 * xy + 2 * zw;
            matrix[1, 1] = 1 - 2 * xx - 2 * zz;
            matrix[1, 2] = 2 * yz - 2 * xw;

            matrix[2, 0] = 2 * xz - 2 * yw;
            matrix[2, 1] = 2 * yz + 2 * xw;
            matrix[2, 2] = 1 - 2 * xx - 2 * yy;

            Vector3 trans = translate;

            matrix[0, 3] = trans.x;
            matrix[1, 3] = trans.y;
            matrix[2, 3] = trans.z;

            matrix[3, 0] = 0;
            matrix[3, 1] = 0;
            matrix[3, 2] = 0;
            matrix[3, 3] = 1;

            return matrix;
        }
                  
        public static DualQuaternion operator *(DualQuaternion dq1, DualQuaternion dq2)
        {
            return new DualQuaternion(dq1.real * dq2.real, (QuaternionExtension.AddQuaternion(dq1.dual * dq2.real, dq1.real * dq2.dual)));
        }

        public static Vector3 operator *(DualQuaternion dq, Vector3 pos)
        {
            return dq.real * pos + dq.translate;
        }

        public static bool operator ==(DualQuaternion dq1, DualQuaternion dq2)
        {
            return dq1.real.Equals(dq2.real) && dq1.dual.Equals(dq2.dual);
        }

        public static bool operator !=(DualQuaternion dq1, DualQuaternion dq2)
        {
            return !dq1.real.Equals(dq2.real) || !dq1.dual.Equals(dq2.dual);
        }

        public override bool Equals(object obj)
        {
            if (obj is DualQuaternion)
            {
                DualQuaternion dq = (DualQuaternion)obj;
                return dq == this;
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// dual quaternion translation extension 
    /// </summary>
    public static class DQExtension
    {
        public static DualQuaternion GetLocalToWorldDQ(this Transform transform)
        {
            if(transform.parent != null)
                return GetLocalToWorldDQ(transform.parent) * new DualQuaternion(transform.localRotation, transform.localPosition);
            else
                return new DualQuaternion(transform.localRotation, transform.localPosition);
        }
        
        public static DualQuaternion GetWorldToLocalDQ(this Transform transform)
        {
            return transform.GetLocalToWorldDQ().inverse;
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
        public DualQuaternion[] dqArray;

        public Matrix4x4[] matrixArray;
        public Matrix4x4[] restMatrixArray;

        public DualQuaternionBlendSkinningCompute(RenderChunk chunk, RuntimeRenderChunk runtimeChunk, Func<ComputeBuffer> getMeshDataBuffer, Func<ComputeBuffer> getMeshDataStream)
        {
            matrixArray = new Matrix4x4[runtimeChunk.bones.Length]; 
            restMatrixArray = runtimeChunk.restPoseBoneInverseMatrix;
            dqArray = runtimeChunk.restPoseBoneInverseDQ;

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
                matrixArray[i] = bones[i].localToWorldMatrix;
                currentPoseDQArray[i] = bones[i].GetLocalToWorldDQ();
            }

            for (int i = 0; i < bones.Length; i++)
            {
                Vector3 matCon = (restMatrixArray[i] * matrixArray[i]).MultiplyPoint(bones[i].transform.position),
                        dqCon = (dqArray[i] * currentPoseDQArray[i]) * bones[i].transform.position;
                if (matCon != (dqCon))
                {
                    Debug.Log(i + ":" + matCon.ToString("F4") + "," + dqCon.ToString("F4"));
                }
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