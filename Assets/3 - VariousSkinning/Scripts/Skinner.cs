namespace Example.VariousSkinning
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public interface IRenderer { void OnRenderObject(); }
    public interface IDisposableRenderer : IDisposable, IRenderer { }

    public interface IUpdate { void Update(); }

    public enum SkinningMethod
    {
        LinearBlend,
        DualQuaternion,
    }

    public class Skinner : MonoBehaviour
    {
        public SkinningMethod skinning;

        public SkinnedMeshRenderer skinnedMeshRenderer;
        public RenderChunk chunk;
        public RuntimeRenderChunk chunk2;
        public Material material;

        public IRenderer implRenderer;
        public IUpdate implUpdate;

        void Awake()
        {
            chunk2.bones = skinnedMeshRenderer.bones;

            chunk2.SetBoneInverseTransform();

            if (SystemInfo.supportsComputeShaders)
            {
                ComputeShaderAdapter adapter = new ComputeShaderAdapter(skinning, chunk, chunk2, material);

                implRenderer = adapter as IRenderer;
                implUpdate = adapter as IUpdate;
            }

            DualQuaternion dq1 = new DualQuaternion(Quaternion.AngleAxis(90, Vector3.up), new Vector3(0f, 0.5f, 0f)),
                            dq2 = new DualQuaternion(Quaternion.AngleAxis(60, Vector3.up), new Vector3(0.5f, 0f, 0f)),
                            dq3 = new DualQuaternion(Quaternion.AngleAxis(90, Vector3.up), new Vector3(0f, 0f, -0.5f));

            Matrix4x4 mat1 = Matrix4x4.TRS(new Vector3(0f, 0.5f, 0f), Quaternion.AngleAxis(90, Vector3.up), Vector3.one),
                mat2 = Matrix4x4.TRS(new Vector3(0.5f, 0f, 0f), Quaternion.AngleAxis(60, Vector3.up), Vector3.one),
                mat3 = Matrix4x4.TRS(new Vector3(0f, 0f, -0.5f), Quaternion.AngleAxis(90, Vector3.up), Vector3.one);

            // DQ 곱하기 연산에 문제
            // DQ 변환 연산도 문제

            Debug.Log("separated dq : " + (dq1 * Vector3.one).ToString("F4") + ", " + (dq1 * (dq2 * Vector3.one)).ToString("F4") + ", " + (dq1 * (dq2 * (dq3 * Vector3.one))).ToString("F4"));
            Debug.Log("combined dq : " + (dq1 * Vector3.one).ToString("F4") + ", " + (dq1 * dq2 * Vector3.one).ToString("F4") + ", " + (dq1 * dq2 * dq3 * Vector3.one).ToString("F4"));
            Debug.Log("point mat : " + (mat1.MultiplyPoint(Vector3.one)).ToString("F4") + ", " + ((mat1 * mat2).MultiplyPoint(Vector3.one)).ToString("F4") + ", " + ((mat1 * mat2 * mat3).MultiplyPoint(Vector3.one)).ToString("F4"));
        }

        private void OnRenderObject()
        {
            implRenderer.OnRenderObject();
        }

        private void Update()
        {
            if (implUpdate != null)
                implUpdate.Update();
        }
    }
}
