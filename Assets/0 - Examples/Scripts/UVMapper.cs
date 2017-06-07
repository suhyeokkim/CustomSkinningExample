using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UV order in plane is bottom, left, top, right
/// Plane order in cube is  top, bottom, front, behind, right, left
/// Parts order is head, torso, larm, rarm, lleg, rleg, 
///                helm, torso2, larm2, rarm2, lleg2, rleg2
/// </summary>
public static class UVMapper
{
    public const int uvCountPart = 24;
    public const int uvPosCount = 24 * 6;

    private static void SetCubeUVs(Vector2[] uvArray, int startIndex, Vector2 partCoord, Vector3 partSize)
    {
        // Right
        uvArray[startIndex + 16] = new Vector2(partCoord.x, partCoord.y);
        uvArray[startIndex + 17] = new Vector2(partCoord.x, partCoord.y + partSize.y);
        uvArray[startIndex + 18] = new Vector2(partCoord.x + partSize.z, partCoord.y + partSize.y);
        uvArray[startIndex + 19] = new Vector2(partCoord.x + partSize.z, partCoord.y);

        // Front
        uvArray[startIndex + 12] = new Vector2(partCoord.x + partSize.z, partCoord.y);
        uvArray[startIndex + 13] = new Vector2(partCoord.x + partSize.z, partCoord.y + partSize.y);
        uvArray[startIndex + 14] = new Vector2(partCoord.x + partSize.z + partSize.x, partCoord.y + partSize.y);
        uvArray[startIndex + 15] = new Vector2(partCoord.x + partSize.z + partSize.x, partCoord.y);

        // Left
        uvArray[startIndex + 20] = new Vector2(partCoord.x + partSize.z + partSize.x, partCoord.y);
        uvArray[startIndex + 21] = new Vector2(partCoord.x + partSize.z + partSize.x, partCoord.y + partSize.y);
        uvArray[startIndex + 22] = new Vector2(partCoord.x + partSize.z * 2 + partSize.x, partCoord.y + partSize.y);
        uvArray[startIndex + 23] = new Vector2(partCoord.x + partSize.z * 2 + partSize.x, partCoord.y);

        // Front
        uvArray[startIndex + 8] = new Vector2(partCoord.x + partSize.x + partSize.z * 2, partCoord.y);
        uvArray[startIndex + 9] = new Vector2(partCoord.x + partSize.x + partSize.z * 2, partCoord.y + partSize.y);
        uvArray[startIndex + 10] = new Vector2(partCoord.x + partSize.x * 2 + partSize.z * 2, partCoord.y + partSize.y);
        uvArray[startIndex + 11] = new Vector2(partCoord.x + partSize.x * 2 + partSize.z * 2, partCoord.y);

        // Top
        uvArray[startIndex + 0] = new Vector2(partCoord.x + partSize.z + partSize.x, partCoord.y + partSize.y + partSize.z);
        uvArray[startIndex + 1] = new Vector2(partCoord.x + partSize.z + partSize.x, partCoord.y + partSize.y);
        uvArray[startIndex + 2] = new Vector2(partCoord.x + partSize.z, partCoord.y + partSize.y);
        uvArray[startIndex + 3] = new Vector2(partCoord.x + partSize.z, partCoord.y + partSize.y + partSize.z);

        // Bottom
        uvArray[startIndex + 4] = new Vector2(partCoord.x + partSize.x + partSize.z, partCoord.y + partSize.y + partSize.z);
        uvArray[startIndex + 5] = new Vector2(partCoord.x + partSize.x + partSize.z, partCoord.y + partSize.y);
        uvArray[startIndex + 6] = new Vector2(partCoord.x + partSize.x + partSize.z * 2, partCoord.y + partSize.y);
        uvArray[startIndex + 7] = new Vector2(partCoord.x + partSize.x + partSize.z * 2, partCoord.y + partSize.y + partSize.z);
    }

    private static void SetCubeUVs(Vector3[] uvArray, int startIndex, Vector2 partCoord, Vector3 partSize, int textureIndex)
    {
        // Right
        uvArray[startIndex + 16] = new Vector3(partCoord.x, partCoord.y, textureIndex);
        uvArray[startIndex + 17] = new Vector3(partCoord.x, partCoord.y + partSize.y, textureIndex);
        uvArray[startIndex + 18] = new Vector3(partCoord.x + partSize.z, partCoord.y + partSize.y, textureIndex);
        uvArray[startIndex + 19] = new Vector3(partCoord.x + partSize.z, partCoord.y, textureIndex);

        // Front
        uvArray[startIndex + 12] = new Vector3(partCoord.x + partSize.z, partCoord.y, textureIndex);
        uvArray[startIndex + 13] = new Vector3(partCoord.x + partSize.z, partCoord.y + partSize.y, textureIndex);
        uvArray[startIndex + 14] = new Vector3(partCoord.x + partSize.z + partSize.x, partCoord.y + partSize.y, textureIndex);
        uvArray[startIndex + 15] = new Vector3(partCoord.x + partSize.z + partSize.x, partCoord.y, textureIndex);

        // Left
        uvArray[startIndex + 20] = new Vector3(partCoord.x + partSize.z + partSize.x, partCoord.y, textureIndex);
        uvArray[startIndex + 21] = new Vector3(partCoord.x + partSize.z + partSize.x, partCoord.y + partSize.y, textureIndex);
        uvArray[startIndex + 22] = new Vector3(partCoord.x + partSize.z * 2 + partSize.x, partCoord.y + partSize.y, textureIndex);
        uvArray[startIndex + 23] = new Vector3(partCoord.x + partSize.z * 2 + partSize.x, partCoord.y, textureIndex);

        // Front
        uvArray[startIndex + 8] = new Vector3(partCoord.x + partSize.x + partSize.z * 2, partCoord.y, textureIndex);
        uvArray[startIndex + 9] = new Vector3(partCoord.x + partSize.x + partSize.z * 2, partCoord.y + partSize.y, textureIndex);
        uvArray[startIndex + 10] = new Vector3(partCoord.x + partSize.x * 2 + partSize.z * 2, partCoord.y + partSize.y, textureIndex);
        uvArray[startIndex + 11] = new Vector3(partCoord.x + partSize.x * 2 + partSize.z * 2, partCoord.y, textureIndex);

        // Top
        uvArray[startIndex + 0] = new Vector3(partCoord.x + partSize.z + partSize.x, partCoord.y + partSize.y + partSize.z, textureIndex);
        uvArray[startIndex + 1] = new Vector3(partCoord.x + partSize.z + partSize.x, partCoord.y + partSize.y, textureIndex);
        uvArray[startIndex + 2] = new Vector3(partCoord.x + partSize.z, partCoord.y + partSize.y, textureIndex);
        uvArray[startIndex + 3] = new Vector3(partCoord.x + partSize.z, partCoord.y + partSize.y + partSize.z, textureIndex);

        // Bottom
        uvArray[startIndex + 4] = new Vector3(partCoord.x + partSize.x + partSize.z, partCoord.y + partSize.y + partSize.z, textureIndex);
        uvArray[startIndex + 5] = new Vector3(partCoord.x + partSize.x + partSize.z, partCoord.y + partSize.y, textureIndex);
        uvArray[startIndex + 6] = new Vector3(partCoord.x + partSize.x + partSize.z * 2, partCoord.y + partSize.y, textureIndex);
        uvArray[startIndex + 7] = new Vector3(partCoord.x + partSize.x + partSize.z * 2, partCoord.y + partSize.y + partSize.z, textureIndex);
    }

    private static void SetCubeUVs(Vector4[] uvArray, int startIndex, Vector2 partCoord, Vector3 partSize, int textureIndex, int boneIndex)
    {
        // Right
        uvArray[startIndex + 16] = new Vector4(partCoord.x, partCoord.y, textureIndex, boneIndex);
        uvArray[startIndex + 17] = new Vector4(partCoord.x, partCoord.y + partSize.y, textureIndex, boneIndex);
        uvArray[startIndex + 18] = new Vector4(partCoord.x + partSize.z, partCoord.y + partSize.y, textureIndex, boneIndex);
        uvArray[startIndex + 19] = new Vector4(partCoord.x + partSize.z, partCoord.y, textureIndex, boneIndex);

        // Front
        uvArray[startIndex + 12] = new Vector4(partCoord.x + partSize.z, partCoord.y, textureIndex, boneIndex);
        uvArray[startIndex + 13] = new Vector4(partCoord.x + partSize.z, partCoord.y + partSize.y, textureIndex, boneIndex);
        uvArray[startIndex + 14] = new Vector4(partCoord.x + partSize.z + partSize.x, partCoord.y + partSize.y, textureIndex, boneIndex);
        uvArray[startIndex + 15] = new Vector4(partCoord.x + partSize.z + partSize.x, partCoord.y, textureIndex, boneIndex);

        // Left
        uvArray[startIndex + 20] = new Vector4(partCoord.x + partSize.z + partSize.x, partCoord.y, textureIndex, boneIndex);
        uvArray[startIndex + 21] = new Vector4(partCoord.x + partSize.z + partSize.x, partCoord.y + partSize.y, textureIndex, boneIndex);
        uvArray[startIndex + 22] = new Vector4(partCoord.x + partSize.z * 2 + partSize.x, partCoord.y + partSize.y, textureIndex, boneIndex);
        uvArray[startIndex + 23] = new Vector4(partCoord.x + partSize.z * 2 + partSize.x, partCoord.y, textureIndex, boneIndex);

        // Front
        uvArray[startIndex + 8] = new Vector4(partCoord.x + partSize.x + partSize.z * 2, partCoord.y, textureIndex, boneIndex);
        uvArray[startIndex + 9] = new Vector4(partCoord.x + partSize.x + partSize.z * 2, partCoord.y + partSize.y, textureIndex, boneIndex);
        uvArray[startIndex + 10] = new Vector4(partCoord.x + partSize.x * 2 + partSize.z * 2, partCoord.y + partSize.y, textureIndex, boneIndex);
        uvArray[startIndex + 11] = new Vector4(partCoord.x + partSize.x * 2 + partSize.z * 2, partCoord.y, textureIndex, boneIndex);

        // Top
        uvArray[startIndex + 0] = new Vector4(partCoord.x + partSize.z + partSize.x, partCoord.y + partSize.y + partSize.z, textureIndex, boneIndex);
        uvArray[startIndex + 1] = new Vector4(partCoord.x + partSize.z + partSize.x, partCoord.y + partSize.y, textureIndex, boneIndex);
        uvArray[startIndex + 2] = new Vector4(partCoord.x + partSize.z, partCoord.y + partSize.y, textureIndex, boneIndex);
        uvArray[startIndex + 3] = new Vector4(partCoord.x + partSize.z, partCoord.y + partSize.y + partSize.z, textureIndex, boneIndex);

        // Bottom
        uvArray[startIndex + 4] = new Vector4(partCoord.x + partSize.x + partSize.z, partCoord.y + partSize.y + partSize.z, textureIndex, boneIndex);
        uvArray[startIndex + 5] = new Vector4(partCoord.x + partSize.x + partSize.z, partCoord.y + partSize.y, textureIndex, boneIndex);
        uvArray[startIndex + 6] = new Vector4(partCoord.x + partSize.x + partSize.z * 2, partCoord.y + partSize.y, textureIndex, boneIndex);
        uvArray[startIndex + 7] = new Vector4(partCoord.x + partSize.x + partSize.z * 2, partCoord.y + partSize.y + partSize.z, textureIndex, boneIndex);
    }

    public static void GetUV(ref Vector2[] uvArray, Vector2[] uvPosArray, Vector3[] uvSizeArray)
    {
        if (uvArray == null)
            uvArray = new Vector2[uvPosCount];
        else if (uvArray.Length < uvPosCount)
            Array.Resize(ref uvArray, uvPosCount);

        for (int i = 0; i < 6; i++)
            SetCubeUVs(uvArray, UVMapper.uvCountPart * i, uvPosArray[i], uvSizeArray[i]);
    }

    public static void GetUV(ref Vector3[] uvArray, Vector2[] uvPosArray, Vector3[] uvSizeArray, int textureIndex)
    {
        if (uvArray == null)
            uvArray = new Vector3[uvPosCount];
        else if (uvArray.Length < uvPosCount)
            Array.Resize(ref uvArray, uvPosCount);

        for (int i = 0; i < 6; i++)
            SetCubeUVs(uvArray, UVMapper.uvCountPart * i, uvPosArray[i], uvSizeArray[i], textureIndex);
    }

    public static void GetUV(ref Vector4[] uvArray, Vector2[] uvPosArray, Vector3[] uvSizeArray, int textureIndex)
    {
        if (uvArray == null)
            uvArray = new Vector4[uvPosCount];
        else if (uvArray.Length < uvPosCount)
            Array.Resize(ref uvArray, uvPosCount);

        for (int i = 0; i < 6; i++)
            SetCubeUVs(uvArray, UVMapper.uvCountPart * i, uvPosArray[i], uvSizeArray[i], textureIndex, i);
    }
}