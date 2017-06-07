namespace Example.TextureArray
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Character : MonoBehaviour
    {
        [Header("Character script for TextureArray"), Space()]
        public CharacterData data;
        public bool isSkinned;
        public RuntimeAnimatorController animatorController;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        SkinnedMeshRenderer skinnedMeshRenderer;
        Animator animator;

        public void BuildComponents()
        {
            if (isSkinned)
            {
                skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

                if (skinnedMeshRenderer == null)
                    skinnedMeshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();

                skinnedMeshRenderer.receiveShadows = false;
            }
            else
            {
                meshFilter = GetComponent<MeshFilter>();

                if (meshFilter == null)
                    meshFilter = gameObject.AddComponent<MeshFilter>();

                meshRenderer = GetComponent<MeshRenderer>();

                if (meshRenderer == null)
                    meshRenderer = gameObject.AddComponent<MeshRenderer>();

                meshRenderer.receiveShadows = false;
            }

            animator = GetComponent<Animator>();

            if (animator == null)
                animator = gameObject.AddComponent<Animator>();

            animator.runtimeAnimatorController = animatorController;
        }

        public Mesh BuildMesh(Transform[] boneArray)
        {
            Mesh mesh = new Mesh();

            Vector3[] vertices = null;
            int[] indices = null;

            VertexMapper.GetVertices(ref vertices, data.GetBodyPoses(), data.GetBodySizes());
            VertexMapper.GetIndices(ref indices);

            mesh.vertices = vertices;
            mesh.triangles = indices;

            Vector2[] uvs = null;
            UVMapper.GetUV(ref uvs, data.GetUVPoses(), data.GetUVSizes());
            mesh.uv = uvs;

            Matrix4x4[] bindPoses = null;
            BoneWeight[] weight = null;

            SkinMapper.GetBoneWieghts(ref weight);
            Rigger.GetBindPoses(ref bindPoses, transform, boneArray);

            mesh.boneWeights = weight;
            mesh.bindposes = bindPoses;

            return mesh;
        }

        public Mesh BuildMesh(Transform[] boneArray, int textureIndex)
        {
            Mesh mesh = new Mesh();

            Vector3[] vertices = null;
            int[] indices = null;

            VertexMapper.GetVertices(ref vertices, data.GetBodyPoses(), data.GetBodySizes());
            VertexMapper.GetIndices(ref indices);

            mesh.vertices = vertices;
            mesh.triangles = indices;

            Vector3[] uvs = null;
            UVMapper.GetUV(ref uvs, data.GetUVPoses(), data.GetUVSizes(), textureIndex);
            mesh.SetUVs(0, new List<Vector3>(uvs));

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

            if (mesh == null)
                mesh = BuildMesh(boneArray);

            if (isSkinned)
            {
                skinnedMeshRenderer.sharedMesh = mesh;
                skinnedMeshRenderer.material = material;
                skinnedMeshRenderer.bones = boneArray;
                skinnedMeshRenderer.rootBone = transform;
            }
            else
            {
                meshFilter.sharedMesh = mesh;
                meshRenderer.sharedMaterial = material;
            }
        }
    }
}