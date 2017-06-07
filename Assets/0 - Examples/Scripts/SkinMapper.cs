using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SkinMapper
{
    public const int partCount = 6;
    public const int vertexByPart = 24;

    public static void GetBoneWieghts(ref BoneWeight[] weights)
    {
        if (weights == null)
            weights = new BoneWeight[partCount * vertexByPart];
        else if (weights.Length < partCount * vertexByPart)
            Array.Resize(ref weights, partCount * vertexByPart);

        for (int i = 0; i < partCount; i++)
        {
            for (int j = 0; j < vertexByPart; j++)
            {
                weights[i * vertexByPart + j] = new BoneWeight() { boneIndex0 = i, weight0 = 1 };
            }
        }
    }
}
