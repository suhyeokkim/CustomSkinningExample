namespace Example.VariousSkinning.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using UnityEngine;

    public static class RenderChunkHandler
    {
        public static void SetBoneData(this RenderChunk chunk, SkinnedMeshRenderer renderer)
        {
            if (renderer == null)
            {
                Debug.Log("SetMeshData method need some data, renderer == null..");
                return;
            }

            chunk.rootBoneName = renderer.rootBone.transform.name;
            chunk.indexedBoneNameArray = Array.ConvertAll(renderer.bones, (bone) => bone.name);

            chunk.inverseRestPoseMatrixArray = Array.ConvertAll(renderer.bones, (bone) => bone.worldToLocalMatrix);
            chunk.inverseRestPoseDQArray = Array.ConvertAll(renderer.bones, (bone) => bone.GetWorldToLocalDQ());
        }

        public static void SetMeshData(this RenderChunk chunk, Mesh mesh)
        {
            if (mesh == null)
            {
                Debug.Log("SetMeshData method need some data, mesh == null..");
                return;
            }

            int b1Cnt = 0, b2Cnt = 0, b3Cnt = 0, b4Cnt = 0;

            {
                chunk.vertexCount = mesh.vertexCount;
            }

            {
                MeshDataInfo[] meshData = new MeshDataInfo[mesh.vertexCount];

                Vector3[] vertices = mesh.vertices;
                Vector3[] normals = mesh.normals;
                Vector2[] uv = mesh.uv;
                BoneWeight[] sourceBoneWeights = mesh.boneWeights;

                // This code very long time
                for (int i = 0; i < meshData.Length; i++)
                {
                    meshData[i] =
                        new MeshDataInfo()
                        {
                            position = vertices[i],
                            normal = normals[i],
                            uv = uv[i],
                            weight = new Vector4(sourceBoneWeights[i].weight0, sourceBoneWeights[i].weight1, sourceBoneWeights[i].weight2, sourceBoneWeights[i].weight3),
                            index = new Integer4(sourceBoneWeights[i].boneIndex0, sourceBoneWeights[i].boneIndex1, sourceBoneWeights[i].boneIndex2, sourceBoneWeights[i].boneIndex3)
                        };

                    if (meshData[i].weight.y == 0) b1Cnt++;
                    else if (meshData[i].weight.z == 0) b2Cnt++;
                    else if (meshData[i].weight.w == 0) b3Cnt++;
                    else b4Cnt++;
                }

                chunk.boneVertexCount = new Integer4(b1Cnt, b2Cnt, b3Cnt, b4Cnt);
                chunk.meshData = meshData;
            }
        }

        public static void SetIndices(this RenderChunk chunk, Mesh mesh)
        {
            if (mesh == null)
            {
                Debug.Log("SetMeshData method need some data, mesh == null..");
                return;
            }

            int[] triangles = mesh.triangles;

            chunk.indices = new int[triangles.Length];
            Array.Copy(triangles, chunk.indices, chunk.indices.Length);

            chunk.indexCounts = new uint[mesh.subMeshCount];
            for (int i = 0; i < chunk.indexCounts.Length; i++)
                chunk.indexCounts[i] = mesh.GetIndexStart(i) + mesh.GetIndexCount(i);
        }
        
        public static unsafe float GetWeightDistance(this MeshDataInfo info1, MeshDataInfo info2)
        {
            bool* isValidArray = stackalloc bool[8];
            float wholeDistance = 0;

            for (int i = 0; i < 4; i++)
            {
                isValidArray[i] = info1.weight[i] != 0f;
                isValidArray[i+4] = info2.weight[i] != 0f;
            }

            for (int i = 0; i < 4; i++)
            {
                if (isValidArray[i])
                {
                    float weight = info1.weight[i];

                    for (int j = i; j < 4; j++)
                        if (isValidArray[j + 4])
                            if (info2.index[j] == info1.index[i])
                            {
                                weight -= info2.weight[j];
                                isValidArray[j + 4] = false;
                                break;
                            }

                    wholeDistance += weight * weight;
                    isValidArray[i] = false;
                }

                if (isValidArray[i + 4])
                {
                    float weight = info2.weight[i];

                    for (int j = i; j < 4; j++)
                        if (isValidArray[j])
                            if (info1.index[j] == info2.index[i])
                            {
                                weight -= info1.weight[j];
                                isValidArray[j] = false;
                                break;
                            }

                    wholeDistance += weight * weight;
                    isValidArray[i + 4] = false;
                }
            }

            return Mathf.Sqrt(wholeDistance);
        }

        public static unsafe float GetSimliarity(this RenderChunk chunk, int vtxIdx1, int vtxIdx21, int vtxIdx22, int vtxIdx23, float kernel)
        {
            int threeWeightCount = 0, index;
            int* threeWeightIndexArray = stackalloc int[12];
            float* threeWeightSumedWeightArray = stackalloc float[12];

            Integer4 index1 = chunk.meshData[vtxIdx1].index, 
                    index21 = chunk.meshData[vtxIdx21].index, index22 = chunk.meshData[vtxIdx22].index, index23 = chunk.meshData[vtxIdx23].index;
            Vector4 weight1 = chunk.meshData[vtxIdx1].weight, 
                    weight21 = chunk.meshData[vtxIdx21].weight, weight22 = chunk.meshData[vtxIdx22].weight, weight23 = chunk.meshData[vtxIdx23].weight;

            // Merge 3 weight data 
            {
                for (int i = 0; i < 4; i++)
                {
                    index = -1;

                    for (int j = 0; j < threeWeightCount; j++)
                        if (threeWeightIndexArray[j] == index21[i])
                        {
                            index = j;
                            threeWeightSumedWeightArray[index] += weight21[i] / 3;
                            break;
                        }

                    if (index < 0)
                    {
                        threeWeightIndexArray[threeWeightCount] = index21[i];
                        threeWeightSumedWeightArray[threeWeightCount] = weight21[i] / 3;
                        threeWeightCount++;
                    }

                    index = -1;

                    for (int j = 0; j < threeWeightCount; j++)
                        if (threeWeightIndexArray[j] == index22[i])
                        {
                            index = j;
                            threeWeightSumedWeightArray[index] += weight22[i] / 3;
                            break;
                        }

                    if (index < 0)
                    {
                        threeWeightIndexArray[threeWeightCount] = index22[i];
                        threeWeightSumedWeightArray[threeWeightCount] = weight22[i] / 3;
                        threeWeightCount++;
                    }

                    index = -1;

                    for (int j = 0; j < threeWeightCount; j++)
                        if (threeWeightIndexArray[j] == index23[i])
                        {
                            index = j;
                            threeWeightSumedWeightArray[index] += weight23[i] / 3;
                            break;
                        }

                    if (index < 0)
                    {
                        threeWeightIndexArray[threeWeightCount] = index23[i];
                        threeWeightSumedWeightArray[threeWeightCount] = weight23[i] / 3;
                        threeWeightCount++;
                    }
                }
            }

            int sameIndexCount = 0;
            float* weightArray = stackalloc float[8];

            // Find same bone index, and store weights.
            {
                for (int i = 0; i < 4; i++)
                    if (weight1[i] > 0)
                    {
                        for (int j = 0; j < threeWeightCount; j++)
                            if (threeWeightSumedWeightArray[j] > 0)
                                if (index1[i] == threeWeightIndexArray[j])
                                {
                                    weightArray[sameIndexCount] = weight1[i];
                                    weightArray[sameIndexCount + 1] = threeWeightSumedWeightArray[j];

                                    sameIndexCount++;
                                    break;
                                }
                    }
            }

            // Just calculate similarity with store weights.
            if (sameIndexCount < 2) return 0f;
            else
            {
                float similarity = 0f, diff;

                for (int i = 0; i < sameIndexCount; i++)
                    for (int j = i + 1; j < sameIndexCount; j++)
                    {
                        diff = weightArray[i] * weightArray[j + 1] - weightArray[i + 1] * weightArray[j];
                        similarity += weightArray[i] * weightArray[i + 1] * weightArray[j] * weightArray[j + 1] * Mathf.Exp(-(diff * diff) / (kernel * kernel));
                    }

                return similarity;
            }
        }

        public static unsafe float GetSimliarity(this RenderChunk chunk, int vtxIdx1, int vtxIdx2, float kernel)
        {
            int sameIndexCount = 0;
            float* weightArray = stackalloc float[8];

            // Find same bone index, and store weights.
            {
                Integer4 index1 = chunk.meshData[vtxIdx1].index, index2 = chunk.meshData[vtxIdx2].index;
                Vector4 weight1 = chunk.meshData[vtxIdx1].weight, weight2 = chunk.meshData[vtxIdx2].weight;

                for (int i = 0; i < 4; i++)
                    if (weight1[i] > 0)
                    {
                        for (int j = 0; j < 4; j++)
                            if (weight2[j] > 0)
                                if (index1[i] == index2[j])
                                {
                                    weightArray[sameIndexCount] = weight1[i];
                                    weightArray[sameIndexCount + 1] = weight2[j];

                                    sameIndexCount++;
                                    break;
                                }
                    }
            }

            // Just calculate similarity with store weights.
            if (sameIndexCount < 2) return 0f;
            else
            {
                float similarity = 0f, diff;

                for (int i = 0; i < sameIndexCount; i++)
                    for (int j = i + 1; j < sameIndexCount; j++)
                    {
                        diff = weightArray[i] * weightArray[j + 1] - weightArray[i + 1] * weightArray[j];
                        similarity += weightArray[i] * weightArray[i + 1] * weightArray[j] * weightArray[j + 1] * Mathf.Exp(-(diff * diff) / (kernel * kernel));
                    }

                return similarity;
            }
        }

        /*
         * Approximate nearest neighbor search
         * 1. WeightIndex, WeightWeight 가 기준인 skinning weight space 에서 maximized minimum distance point set 을 계산하여 몇개의 정점을 뽑아서 저장함. 
         *    계산된 Weight들은 비슷한 Weight 의 반복을 피하기 위하여 대표로 뽑히 Weight임. 또한 같은 Weight 를 가지면 같은 cluster 로 취급함.
         * 2. 계산된 정점으로 ||Wi - Wj||2 < ω 식을 계산하여 ANN 을 실행함. 앞의 식은 Weight 사이의 차이를 나타내므로 작으면 작을수록 가깝다는 뜻임. 즉 가까우면 계속 진행하는 구문이다. 
         * 
         * Smooth skinning weights assumption 
         * BFS 로 인덱스 버퍼를 그래프의 가중치로 취급하여 탐색함. 판단 구문은 Similarity < ε 이면 탐색을 멈춘다. ε 는 정해준 threshold 임.
         * 
         * Parallel implementation
         * 병렬 구현.. GPGPU 나 Multi-thread 로 구현해야함. Multi-thread 는 .Net 으로 하면됨. GPGPU 는 몰라 ㅅㅂ 
         */
        public static void CalculateCenterOfRotation(this RenderChunk chunk, Mesh mesh, float similarityKernel, float limitWeightDistance, float similarityThresholds)
        {
        }
    }
}