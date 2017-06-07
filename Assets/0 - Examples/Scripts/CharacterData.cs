using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Int3
{
    public int x;
    public int y;
    public int z;

    public Int3(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static Vector3 operator *(Int3 data, float mul)
    {
        return new Vector3(mul * data.x, mul * data.y, mul * data.z);
    }
}

[CreateAssetMenu(fileName = "CharData")]
public class CharacterData : ScriptableObject
{
    [SerializeField]
    private string characterName = "Steve";

    [SerializeField]
    private Texture2D texture;

    [SerializeField]
    private Int3[] bodySizeArray = new Int3[] {
        new Int3(8, 8, 8),          // HEAD
        new Int3(8, 12, 4),         // BODY
        new Int3(4, 12, 4),         // LARM
        new Int3(4, 12, 4),         // RARM
        new Int3(4, 12, 4),         // LLEG
        new Int3(4, 12, 4),         // RLEG
    };

    public string charName { get { return characterName; } private set { characterName = value; } }
    public Texture2D charTexture { get { return texture; } }

    public static float defaultBodyMultipiler = 0.0625f;
    public const int boneCount = 6;

    public Vector3[] GetBodyPoses() { return GetBodyPoses(defaultBodyMultipiler); }
    public Vector3[] GetBodySizes() { return GetBodySizes(defaultBodyMultipiler); }
    public Vector3[] GetBonePoses() { return GetBonePoses(defaultBodyMultipiler); }

    public Vector3[] GetBodyPoses(float multiplier)
    {
        Vector3[] bodyPoses = new Vector3[boneCount];

        bodyPoses[4] = new Vector3(bodySizeArray[4].x / -2f, bodySizeArray[4].y / 2f, 0f) * multiplier;
        bodyPoses[5] = new Vector3(bodySizeArray[5].x /  2f, bodySizeArray[5].y / 2f, 0f) * multiplier;

        bodyPoses[1] = new Vector3(0f, bodySizeArray[4].y + bodySizeArray[1].y / 2f, 0f) * multiplier;
        bodyPoses[0] = new Vector3(0f, bodySizeArray[4].y + bodySizeArray[1].y + bodySizeArray[0].y / 2f, 0f) * multiplier;

        bodyPoses[2] = 
            new Vector3(
                    (bodySizeArray[1].x / 2f + bodySizeArray[2].x / 2f) * -1f, 
                    bodySizeArray[4].y + bodySizeArray[1].y - bodySizeArray[2].y / 2f, 
                    0f
                ) * multiplier;
        bodyPoses[3] = 
            new Vector3(
                    bodySizeArray[1].x / 2f + bodySizeArray[3].x / 2f, 
                    bodySizeArray[5].y + bodySizeArray[1].y - bodySizeArray[3].y / 2f, 
                    0f
                ) * multiplier;

        return bodyPoses;
    }
    public Vector3[] GetBodySizes(float multiplier)
    {
        return Array.ConvertAll<Int3, Vector3>(bodySizeArray, (data) => { return data * multiplier; });
    }

    public Vector3[] GetBonePoses(float multiplier)
    {
        Vector3[] bonePoses = new Vector3[6];

        bonePoses[4] = new Vector3(bodySizeArray[4].x / -2f, bodySizeArray[4].y, 0f) * multiplier;
        bonePoses[5] = new Vector3(bodySizeArray[5].x / 2f, bodySizeArray[5].y, 0f) * multiplier;

        bonePoses[1] = new Vector3(0f, bodySizeArray[4].y + bodySizeArray[1].y, 0f) * multiplier;
        bonePoses[0] = new Vector3(0f, bodySizeArray[4].y + bodySizeArray[1].y, 0f) * multiplier;

        bonePoses[2] =
            new Vector3(
                    (bodySizeArray[1].x / 2f + bodySizeArray[2].x / 2f) * -1f,
                    bodySizeArray[4].y + bodySizeArray[1].y - 2f,
                    0f
                ) * multiplier;
        bonePoses[3] =
            new Vector3(
                    bodySizeArray[1].x / 2f + bodySizeArray[3].x / 2f,
                    bodySizeArray[5].y + bodySizeArray[1].y - 2f,
                    0f
                ) * multiplier;

        return bonePoses;
    }

    public static float defaultUVMultipiler = 0.015625f;

    public Vector2[] GetUVPoses() { return GetUVPoses(defaultUVMultipiler); }
    public Vector3[] GetUVSizes() { return GetUVSizes(defaultUVMultipiler); }

    private Vector2[] GetUVPoses(float multiplier)
    {
        Vector2[] uvPoses = new Vector2[boneCount * 2];

        uvPoses[0               ] = new Vector2(0f, 0.75f);
        uvPoses[0 + boneCount   ] = new Vector2((bodySizeArray[0].x + bodySizeArray[0].z) * 2f * multiplier, 0.75f);

        uvPoses[1               ] = new Vector2((bodySizeArray[5].x + bodySizeArray[5].z) * 2f * multiplier, 0.5f);
        uvPoses[1 + boneCount   ] = new Vector2((bodySizeArray[5].x + bodySizeArray[5].z) * 2f * multiplier, 0.25f);
        
        uvPoses[2               ] = new Vector2(0.5f, 0f);
        uvPoses[2 + boneCount   ] = new Vector2(0.5f + (bodySizeArray[2].x + bodySizeArray[2].z) * 2f * multiplier, 0f);
        
        uvPoses[3               ] = new Vector2((bodySizeArray[5].x + bodySizeArray[5].z + bodySizeArray[1].x + bodySizeArray[1].z) * 2f * multiplier, 0.5f);
        uvPoses[3 + boneCount   ] = new Vector2((bodySizeArray[5].x + bodySizeArray[5].z + bodySizeArray[1].x + bodySizeArray[1].z) * 2f * multiplier, 0.25f);
        
        uvPoses[4               ] = new Vector2((bodySizeArray[4].x + bodySizeArray[4].z) * 2f * multiplier, 0f); 
        uvPoses[4 + boneCount   ] = new Vector2(0f, 0f);

        uvPoses[5               ] = new Vector2(0f, 0.5f);
        uvPoses[5 + boneCount   ] = new Vector2(0f, 0.25f);
        
        return uvPoses;
    }
    private Vector3[] GetUVSizes(float multiplier)
    {
        return Array.ConvertAll<Int3, Vector3>(bodySizeArray, (data) => { return data * multiplier; });
    }
}
