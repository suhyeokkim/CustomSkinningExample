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
        public bool isSkinned;
        public bool isAnimated;
        public RuntimeAnimatorController animatorController;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        SkinnedMeshRenderer skinnedMeshRenderer;
        Animator animator;
        Transform[] bones;
        Matrix4x4[] startBoneMatrixArray;
        Vector3[] startPosArray;
        MaterialPropertyBlock block;

        public void BuildComponents()
        {
            block = new MaterialPropertyBlock();

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

            if (isAnimated)
            {
                animator = GetComponent<Animator>();

                if (animator == null)
                    animator = gameObject.AddComponent<Animator>();

                animator.runtimeAnimatorController = animatorController;

                animator.SetFloat("Forward", UnityEngine.Random.Range(0f, 1f));
            }
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

        public void BuildCharacter(Mesh mesh, Material material, Transform[] boneArray, bool receiveShaodws, bool castShadow)
        {
            BuildComponents();

            if (material == null)
                material = new Material(Shader.Find("Standard"));

            if (boneArray == null)
                boneArray = BuildBone();

            bones = boneArray;
            startBoneMatrixArray = Array.ConvertAll(bones, (bone) => Matrix4x4.TRS(Vector3.zero, bone.localRotation, bone.localScale).inverse);
            startPosArray = Array.ConvertAll(bones, (bone) => bone.localPosition);

            if (mesh == null)
                mesh = BuildMesh(boneArray);

            if (isSkinned)
            {
                skinnedMeshRenderer.sharedMesh = mesh;
                skinnedMeshRenderer.material = material;
                skinnedMeshRenderer.bones = boneArray;
                skinnedMeshRenderer.rootBone = transform;

                skinnedMeshRenderer.receiveShadows = receiveShaodws;
                skinnedMeshRenderer.shadowCastingMode = castShadow? ShadowCastingMode.On: ShadowCastingMode.Off;

                skinnedMeshRenderer.SetPropertyBlock(block);
            }
            else
            {
                meshFilter.sharedMesh = mesh;
                meshRenderer.sharedMaterial = material;

                meshRenderer.receiveShadows = receiveShaodws;
                meshRenderer.shadowCastingMode = castShadow ? ShadowCastingMode.On : ShadowCastingMode.Off;

                meshRenderer.SetPropertyBlock(block);
            }

            bones[0].localEulerAngles = new Vector3(0f, UnityEngine.Random.Range(-90f, 90f), 0f);
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

        void Update()
        {
            for (int i = 0; i < bones.Length; i++)
            {
                Transform bone = bones[i];
                Vector3 pos = bone.localPosition;

                block.SetMatrix(string.Format("_BoneTransfromMatrix{0}", i), Matrix4x4.TRS(Vector3.zero, bone.localRotation, bone.localScale));
                block.SetVector(string.Format("_BonePosition{0}", i), new Vector4(pos.x, pos.y, pos.z, 1f));
                block.SetColor("_Color", Color.white);
            }

            if (isSkinned)
            {
                skinnedMeshRenderer.SetPropertyBlock(block);
            }
            else
            {
                meshRenderer.SetPropertyBlock(block);
            }
        }
    }
}