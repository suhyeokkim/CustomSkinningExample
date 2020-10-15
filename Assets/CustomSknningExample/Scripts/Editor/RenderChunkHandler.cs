namespace CustomSkinningExample.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading;
    using UnityEngine;

    public static class RenderChunkHandler
    {
        public static void SetBoneData(this SkinnedMeshRenderer renderer, RenderChunk chunk)
        {
            if (renderer == null)
            {
                Debug.Log("SetBoneData method need some data, renderer == null..");
                return;
            }

            chunk.rootBoneName = renderer.rootBone.transform.name;
            chunk.indexedBoneNameArray = Array.ConvertAll(renderer.bones, (bone) => bone.name);

            chunk.inverseRestPoseMatrixArray = Array.ConvertAll(renderer.bones, (bone) => bone.worldToLocalMatrix);
            chunk.inverseRestPoseDQArray = Array.ConvertAll(renderer.bones, (bone) => bone.GetWorldToLocalDQ());
            chunk.inverseRestPoseRotationArray = Array.ConvertAll(renderer.bones, (bone) => Quaternion.Inverse(bone.rotation));
        }

        public static void CopyMiscellaneousData(this Mesh mesh, RenderChunk chunk)
        {
            chunk.topology = mesh.GetTopology(0);
            chunk.topologyCount = mesh.GetTopologyCount(0);
        }

        public static void CopyDataPerVertex(this Mesh mesh, RenderChunk chunk)
        {
            if (mesh == null)
            {
                Debug.Log("CopyDataPerVertex method need some data, mesh == null..");
                return;
            }

            int b1Cnt = 0, b2Cnt = 0, b3Cnt = 0, b4Cnt = 0;

            {
                chunk.vertexCount = mesh.vertexCount;
            }

            {
                DataPerVertex[] dataPerVertex = new DataPerVertex[mesh.vertexCount];
                SkinPerVertex[] skinPerVertex = new SkinPerVertex[mesh.vertexCount];

                Vector3[] vertices = mesh.vertices;
                Vector2[] uv = mesh.uv;

                BoneWeight[] sourceBoneWeights = mesh.boneWeights;

                // This code very long time
                for (int i = 0; i < dataPerVertex.Length; i++)
                {
                    dataPerVertex[i] =
                        new DataPerVertex()
                        {
                            position = vertices[i],
                            uv = uv[i],
                        };
                    skinPerVertex[i] =
                        new SkinPerVertex()
                        {
                            weight = new Vector4(sourceBoneWeights[i].weight0, sourceBoneWeights[i].weight1, sourceBoneWeights[i].weight2, sourceBoneWeights[i].weight3),
                            index = new Integer4(sourceBoneWeights[i].boneIndex0, sourceBoneWeights[i].boneIndex1, sourceBoneWeights[i].boneIndex2, sourceBoneWeights[i].boneIndex3),
                        };

                    if (skinPerVertex[i].weight.y == 0) b1Cnt++;
                    else if (skinPerVertex[i].weight.z == 0) b2Cnt++;
                    else if (skinPerVertex[i].weight.w == 0) b3Cnt++;
                    else b4Cnt++;
                }

                chunk.boneVertexCount = new Integer4(b1Cnt, b2Cnt, b3Cnt, b4Cnt);
                chunk.dataPerVertex = dataPerVertex;
                chunk.skinPerVertex = skinPerVertex;
            }
        }

        public static void CopyDataPerIndex(this Mesh mesh, RenderChunk chunk)
        {
            if (mesh == null)
            {
                Debug.Log("CopyDataPerIndex method need some data, mesh == null..");
                return;
            }

            int[] triangles = mesh.triangles;

            chunk.indices = new int[triangles.Length];
            Array.Copy(triangles, chunk.indices, chunk.indices.Length);

            chunk.edges = new float[triangles.Length];
            int topologyLength = mesh.GetTopologyCount(0);

            for (int i = 0; i < triangles.Length; i += topologyLength)
                for (int j = 0; j < topologyLength; j++)
                    chunk.edges[i + j] = 
                        (
                            chunk.dataPerVertex[chunk.indices[i + j]].position - 
                            chunk.dataPerVertex[chunk.indices[i + (j + 1) % topologyLength]].position
                        ).sqrMagnitude;

            chunk.indexCounts = new uint[mesh.subMeshCount];
            for (int i = 0; i < chunk.indexCounts.Length; i++)
                chunk.indexCounts[i] = mesh.GetIndexStart(i) + mesh.GetIndexCount(i);
        }
        
        public static unsafe float GetWeightDistance(this SkinPerVertex info1, SkinPerVertex info2)
        {
            bool* isValidArray = stackalloc bool[8];
            float wholeDistance = 0;
            bool isCaculationValid = false;

            for (int i = 0; i < 4; i++)
            {
                isValidArray[i] = info1.weight[i] != 0f;
                isValidArray[i+4] = info2.weight[i] != 0f;
            }

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (isValidArray[i] && isValidArray[j + 4])
                    {
                        if (info2.index[j] == info1.index[i])
                            isCaculationValid = true;
                    }

            if (!isCaculationValid) return float.MaxValue;

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

        public static float GetArea(this RenderChunk chunk, int vtxIdx1, int vtxIdx2, int vtxIdx3)
        {
            Vector3 vec12 = (Vector3)(chunk.dataPerVertex[vtxIdx2].position - chunk.dataPerVertex[vtxIdx1].position),
                    vec13 = (Vector3)(chunk.dataPerVertex[vtxIdx3].position - chunk.dataPerVertex[vtxIdx1].position);

            return Vector3.Cross(vec12, vec13).magnitude * 0.5f;
        }

        public static unsafe float GetSimliarity(this RenderChunk chunk, int vtxIdx1, int vtxIdx21, int vtxIdx22, int vtxIdx23, float kernel)
        {
            int threeWeightCount = 0, index;
            int* threeWeightIndexArray = stackalloc int[12];
            float* threeWeightSumedWeightArray = stackalloc float[12];

            Integer4 index1 = chunk.skinPerVertex[vtxIdx1].index, 
                    index21 = chunk.skinPerVertex[vtxIdx21].index, index22 = chunk.skinPerVertex[vtxIdx22].index, index23 = chunk.skinPerVertex[vtxIdx23].index;
            Vector4 weight1 = chunk.skinPerVertex[vtxIdx1].weight, 
                    weight21 = chunk.skinPerVertex[vtxIdx21].weight, weight22 = chunk.skinPerVertex[vtxIdx22].weight, weight23 = chunk.skinPerVertex[vtxIdx23].weight;

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
                Integer4 index1 = chunk.skinPerVertex[vtxIdx1].index, index2 = chunk.skinPerVertex[vtxIdx2].index;
                Vector4 weight1 = chunk.skinPerVertex[vtxIdx1].weight, weight2 = chunk.skinPerVertex[vtxIdx2].weight;

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

        public static IEnumerator<int> CalculateCluster(this RenderChunk chunk, float weightDistanceThreshold)
        {
            bool[] checkCalculateTriangle = new bool[chunk.indices.Length];
            List<int> triangleIndexList = new List<int>();

            bool[] checkCalculateVertex = new bool[chunk.vertexCount];
            int nextClusterID = 0;
            List<ClusterData> clusterDataList = new List<ClusterData>();
            List<int> indexList = new List<int>();
            Queue<int> processVertexIndexQueue = new Queue<int>();
            int numberOfCalculatedVertices = 0;

            for (int iterateVertexIndex = 0; iterateVertexIndex < checkCalculateVertex.Length; iterateVertexIndex++) // long
            {
                if (!checkCalculateVertex[iterateVertexIndex])
                {
                    List<int> vertexIndexList = new List<int>();
                    List<int> tempTriangleIndexList = new List<int>();

                    vertexIndexList.Add(iterateVertexIndex);
                    checkCalculateVertex[iterateVertexIndex] = true;
                    numberOfCalculatedVertices++;

                    processVertexIndexQueue.Enqueue(iterateVertexIndex);

                    while (processVertexIndexQueue.Count > 0)
                    {
                        int processVertexIndex = processVertexIndexQueue.Dequeue(), findVertexIndex = 0;

                        for (int i = 0; i < chunk.indices.Length; i++)
                        {
                            if (chunk.indices[i] == processVertexIndex)
                            {
                                findVertexIndex = i;
                                int triangleIndex = findVertexIndex / 3;

                                if (!checkCalculateTriangle[triangleIndex])
                                {
                                    checkCalculateTriangle[triangleIndex] = true;
                                    tempTriangleIndexList.Add(triangleIndex);
                                }

                                for (int j = 0; j < 3; j++)
                                {
                                    int nextIndex = chunk.indices[triangleIndex * 3 + j];

                                    if (chunk.indices[nextIndex] != findVertexIndex && !checkCalculateVertex[nextIndex])
                                        if (chunk.skinPerVertex[processVertexIndex].GetWeightDistance(chunk.skinPerVertex[nextIndex]) <= weightDistanceThreshold)
                                        {
                                            numberOfCalculatedVertices++;
                                            checkCalculateVertex[nextIndex] = true;
                                            vertexIndexList.Add(nextIndex);
                                            processVertexIndexQueue.Enqueue(nextIndex);
                                        }
                                }
                            }
                        }
                    }

                    clusterDataList.Add(
                            new ClusterData()
                            {
                                clusterID = nextClusterID,

                                startIndexOfCluster = indexList.Count,
                                lengthOfCluster = vertexIndexList.Count,

                                startIndexOfTriangles = triangleIndexList.Count,
                                lengthOfTriangles = tempTriangleIndexList.Count
                            }
                        );
                    indexList.AddRange(vertexIndexList);
                    triangleIndexList.AddRange(tempTriangleIndexList);

                    nextClusterID++;
                    yield return numberOfCalculatedVertices;
                }
            }

            chunk.clusterArray = clusterDataList.ToArray();
            chunk.clusteredVertexIndexArray = indexList.ToArray();
            chunk.clusteredTriangleIndexArray = triangleIndexList.ToArray();
        }

        public const string threadNameRegexPattern = "Thread(.+)_(.+)";
        public const string threadNameFormat = "Thread{0:D4}_{1:D4}";

        public struct CoRProcessThreadState
        {
            public bool done;
            public bool fail;
            public int processCount;
            public int emptyCount;
        }

        public static CoRProcessThreadState[] CalculateCenterOfRotation(this RenderChunk chunk, int maxThreadNumber, float similarityKernel, float similarityThreshold)
        {
            if(chunk.clusterArray == null || chunk.clusteredVertexIndexArray == null)
            {
                Debug.LogError("Must need pre-calculated Center Of Clusters");
                return null;
            }

            CoRProcessThreadState[] processStateArray = new CoRProcessThreadState[maxThreadNumber];

            chunk.centerOfRotationPositionArray = new Vector3[chunk.vertexCount];

            System.Threading.ParameterizedThreadStart CoRProcessStart = 
                (threadObj) =>
                {
                    Thread thread = threadObj as Thread;
                    Match match = Regex.Match(thread.Name, threadNameRegexPattern);

                    int currentThreadNum = int.Parse(match.Groups[1].Value), maximumThreadNum = int.Parse(match.Groups[2].Value),
                        vertexLegnth = chunk.vertexCount;
                    
                    try
                    {
                        for (int clusterIndex = currentThreadNum; clusterIndex < chunk.clusterArray.Length; clusterIndex += maximumThreadNum) 
                        {
                            ClusterData currentCluster = chunk.clusterArray[clusterIndex];

                            for (int clusterVertexIndex = currentCluster.startIndexOfCluster; clusterVertexIndex < currentCluster.startIndexOfCluster + currentCluster.lengthOfCluster; clusterVertexIndex++)
                            {
                                int vertexIndex = chunk.clusteredVertexIndexArray[clusterVertexIndex];

                                Vector3 calculatedVertex = Vector3.zero;
                                float sumedSimiliarity = 0f;

                                for (int clusterTriangleIndex = currentCluster.startIndexOfTriangles; clusterTriangleIndex < currentCluster.startIndexOfTriangles + currentCluster.lengthOfTriangles; clusterTriangleIndex++)
                                {
                                    int triangleIndex = chunk.clusteredTriangleIndexArray[clusterTriangleIndex];
                                    
                                    float similarity = 
                                        chunk.GetSimliarity(
                                            vertexIndex,
                                            chunk.indices[triangleIndex * 3 + 0],
                                            chunk.indices[triangleIndex * 3 + 1],
                                            chunk.indices[triangleIndex * 3 + 2],
                                            similarityKernel
                                        );

                                    if (similarity < similarityThreshold)
                                        continue;

                                    float area = chunk.GetArea(
                                            chunk.indices[triangleIndex * 3 + 0], 
                                            chunk.indices[triangleIndex * 3 + 1], 
                                            chunk.indices[triangleIndex * 3 + 2]
                                        );

                                    calculatedVertex += 
                                        (Vector3)(
                                            chunk.dataPerVertex[chunk.indices[triangleIndex * 3 + 0]].position +
                                            chunk.dataPerVertex[chunk.indices[triangleIndex * 3 + 1]].position +
                                            chunk.dataPerVertex[chunk.indices[triangleIndex * 3 + 2]].position
                                        ) / 3 *
                                        area * similarity;
                                    sumedSimiliarity += area * similarity;
                                }

                                if (sumedSimiliarity > 0)
                                {
                                    chunk.centerOfRotationPositionArray[vertexIndex] = calculatedVertex / sumedSimiliarity;
                                }
                                else
                                {
                                    chunk.centerOfRotationPositionArray[vertexIndex] = chunk.dataPerVertex[vertexIndex].position;
                                    processStateArray[currentThreadNum].emptyCount++;
                                }

                                processStateArray[currentThreadNum].processCount++;
                            }
                        }

                        processStateArray[currentThreadNum].done = true;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        Debug.LogError(e.Source);
                        Debug.LogError(e.Message);

                        processStateArray[currentThreadNum].fail = true;
                    }
                };

            for (int i = 0; i < maxThreadNumber; i++)
            {
                Thread thread = new Thread(CoRProcessStart);
                thread.Name = String.Format(threadNameFormat, i, maxThreadNumber);
                thread.Start(thread);
            }

            return processStateArray;
        }
    }
}