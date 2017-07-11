namespace Example.InstancedSkinning
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;

    public class Character : MonoBehaviour
    {
        [Header("Character script for InstancedSkinning"), Space()]
        public CharacterData data;
        public bool isAnimated;
        public RuntimeAnimatorController animatorController;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        SkinnedMeshRenderer skinnedMeshRenderer;
        Animator animator;
        public Transform[] bones;
        Matrix4x4[] startBoneMatrixArray;
        Vector3[] startPosArray;
        InstancedData instancedData;

        public struct InstancedData
        {
            public Matrix4x4[] boneTransformMatrix;
            public Vector4[] bonePosition;
        }

        public void BuildComponents()
        {
            if (isAnimated)
            {
                animator = GetComponent<Animator>();

                if (animator == null)
                    animator = gameObject.AddComponent<Animator>();

                animator.runtimeAnimatorController = animatorController;

                animator.SetFloat("Forward", UnityEngine.Random.Range(0f, 1f));
            }
        }

        public static Mesh BuildMesh(CharacterData data)
        {
            Mesh mesh = new Mesh();

            Vector3[] vertices = null;
            int[] indices = null;

            VertexMapper.GetVertices(ref vertices, data.GetBodyPoses(), data.GetBodySizes());
            VertexMapper.GetIndices(ref indices);

            mesh.vertices = vertices;
            mesh.triangles = indices;

            Vector3[] uvs = null;
            UVMapper.GetUV(ref uvs, data.GetUVPoses(), data.GetUVSizes());
            mesh.SetUVs(0, new List<Vector3>(uvs));
            
            return mesh;
        }

        public static Mesh BuildMesh(CharacterData data, Transform transform, Transform[] boneArray, int textureIndex)
        {
            Mesh mesh = new Mesh();

            Vector3[] vertices = null;
            int[] indices = null;

            VertexMapper.GetVertices(ref vertices, data.GetBodyPoses(), data.GetBodySizes());
            VertexMapper.GetIndices(ref indices);

            mesh.vertices = vertices;
            mesh.triangles = indices;

            Vector4[] uvs = null;
            UVMapper.GetUV(ref uvs, data.GetUVPoses(), data.GetUVSizes(), textureIndex);
            mesh.SetUVs(0, new List<Vector4>(uvs));

            Matrix4x4[] bindPoses = null;
            BoneWeight[] weight = null;

            SkinMapper.GetBoneWieghts(ref weight);
            Rigger.GetBindPoses(ref bindPoses, transform, boneArray);

            mesh.boneWeights = weight;
            mesh.bindposes = bindPoses;

            return mesh;
        }

        public Transform[] BuildBone()
        {
            return Array.ConvertAll(
                           data.GetBonePoses(),
                           (poses) =>
                           {
                               Transform bone = new GameObject(string.Format("Bone{0}", transform.childCount)).transform;

                               bone.parent = transform;
                               bone.localPosition = poses;

                               return bone;
                           }
                           );
        }

        public void BuildCharacter(Mesh mesh, Material material, Transform[] boneArray)
        {
            BuildComponents();

            if (material == null)
                material = new Material(Shader.Find("Standard"));

            if (boneArray == null)
                boneArray = BuildBone();

            bones = boneArray;
            startBoneMatrixArray = Array.ConvertAll(bones, (bone) => Matrix4x4.TRS(Vector3.zero, bone.localRotation, bone.localScale).inverse);
            startPosArray = Array.ConvertAll(bones, (bone) => bone.localPosition);

            bones[0].localEulerAngles = new Vector3(0f, UnityEngine.Random.Range(-90f, 90f), 0f);

            instancedData = new InstancedData();

            instancedData.boneTransformMatrix = new Matrix4x4[bones.Length];
            instancedData.bonePosition = new Vector4[bones.Length];
        }

        public void CopyEachBoneInfo(ref Matrix4x4[] matrixArray, ref Vector4[] posArray, int startIndex)
        {
            for (int i = 0; i < bones.Length; i++)
            {
                Transform bone = bones[i];
                matrixArray[i + startIndex] = Matrix4x4.TRS(Vector3.zero, bone.localRotation, bone.localScale);
                posArray[i] = bones[i].localPosition;
            }
        }

        public InstancedData GetInstancedData()
        {
            for (int i = 0; i < bones.Length; i++)
            {
                Transform bone = bones[i];
                Vector3 pos = bone.localPosition;

                instancedData.boneTransformMatrix[i] = Matrix4x4.TRS(Vector3.zero, bone.localRotation, bone.localScale);
                instancedData.bonePosition[i] = new Vector4(pos.x, pos.y, pos.z, 1f);
            }

            return instancedData;
        }
    }
}
