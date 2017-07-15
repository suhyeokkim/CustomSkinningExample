namespace Example.VariousSkinning
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public struct Integer4
    {
        public int x;
        public int y;
        public int z;
        public int w;

        public int this[int idx] {
            get { return idx == 0 ? x : idx == 1 ? y : idx == 2 ? z : idx == 3 ? w : -1; }
            set { switch (idx) { case 0: x = value; break; case 1: y = value;break; case 2: z = value; break; case 3: w = value; break; } }
        }

        public Integer4(int x, int y, int z, int w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public override string ToString()
        {
            return string.Format("({0},{1},{2},{3})",x,y,z,w);
        }
    }

    [System.Serializable]
    public struct MeshDataInfo
    {
        public Vector4 position;
        public Vector4 normal;

        public Vector4 weight;
        public Integer4 index;

        public Vector2 uv;

        public override string ToString()
        {
            return String.Format("Position : {0}, Normal : {1}, Weight : {2}, Index : {3}, UV : {4}", position, normal, weight, index, uv);
        }
    };

    [System.Serializable]
    public struct RenderData
    {
        public Vector4 position;
        public Vector4 normal;
        
        public Vector2 uv;
    };

    [CreateAssetMenu]
    public class RenderChunk : ScriptableObject
    {
#if UNITY_EDITOR
        public SkinnedMeshRenderer builtInRenderer;
#endif

        #region bone names for load bones

        public string rootBoneName;
        public string[] indexedBoneNameArray;

        #endregion

        #region rest pose inverse transform data

        public DualQuaternion[] inverseRestPoseDQArray;
        public Matrix4x4[] inverseRestPoseMatrixArray;

        public Quaternion[] inverseRestPoseRotationArray;
        public Vector3[] inverseRestPoseTranslateArray;

        #endregion

        #region common mesh data
        
        public int[] indices;
        public uint[] indexCounts;

        public int vertexCount;
        public MeshDataInfo[] meshData;

        public Integer4 boneVertexCount;

        #endregion

        #region calculate for center of rotation

        // nothing related, maybe related root vertex length
        public int[] centerOfClusterVertexIndexArray;
        // related vertex length
        public int[] centerOfClusterClusterNumberArray;

        // Generated position data per vertex
        public Vector3[] centerOfRotationPositionArray;

        #endregion

        public void Clear()
        {
            rootBoneName = null;
            indexedBoneNameArray = null;
            inverseRestPoseDQArray = null;
            inverseRestPoseMatrixArray = null;
            indices = null;
            indexCounts = null;
            vertexCount = 0;
            meshData = null;
            centerOfClusterVertexIndexArray = null;
            centerOfClusterClusterNumberArray = null;
            centerOfRotationPositionArray = null;
        }

        public Transform[] GetBones(Transform tranform)
        {
            Transform parent = tranform;
            Transform[] bones = new Transform[indexedBoneNameArray.Length];

            Queue<Transform> transformQueue = new Queue<Transform>();

            transformQueue.Enqueue(parent);

            Transform rootBone = null;

            while (transformQueue.Count > 0)
            {
                Transform currentTransform = transformQueue.Dequeue();

                if (currentTransform.gameObject.name.CompareTo(rootBoneName) == 0)
                {
                    rootBone = currentTransform;
                    break;
                }
                else
                {
                    for (int i = 0; i < currentTransform.childCount; i++)
                        transformQueue.Enqueue(currentTransform.GetChild(i));
                }
            }

            transformQueue.Clear();

            if (rootBone == null)
            {
                Debug.LogErrorFormat("Cannot find rootBone.. name is {0}", rootBoneName);
                return null;
            }

            transformQueue.Enqueue(rootBone);
            int boneCheckCount = 0;

            while (transformQueue.Count > 0)
            {
                Transform currentTransform = transformQueue.Dequeue();

                int index = -1;

                for (int i = 0; i < indexedBoneNameArray.Length; i++)
                {
                    if (bones[i] == null && indexedBoneNameArray[i].CompareTo(currentTransform.name) == 0)
                    {
                        index = i;
                        break;
                    }
                }

                if (index >= 0)
                {
                    if (bones[index] == null)
                        boneCheckCount++;

                    bones[index] = currentTransform;
                }

                if (boneCheckCount >= bones.Length)
                    break;

                for (int i = 0; i < currentTransform.childCount; i++)
                    transformQueue.Enqueue(currentTransform.GetChild(i));
            }

            return bones;
        }
    }
}