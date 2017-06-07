namespace Example.TextureArray
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CharacterSet : MonoBehaviour
    {
        public Shader shader;
        public Texture2DArrayManager texArrayManager;

        Character[] charArray;
        Dictionary<int, Mesh> meshDict;

        void Awake()
        {
            Material material = new Material(shader);
            material.SetTexture("_MainTexArray", texArrayManager.texture2DArray);

            meshDict = new Dictionary<int, Mesh>();

            charArray = GetComponentsInChildren<Character>();

            Array.ForEach(
                charArray,
                (chr) =>
                {
                    Mesh mesh = null;
                    int index = Array.FindIndex(texArrayManager.textureInputArray, (tex) => tex.Equals(chr.data.charTexture));

                    Transform[] boneArray = chr.BuildBone();

                    if (meshDict.ContainsKey(index))
                    {
                        mesh = meshDict[index];
                    }
                    else
                    {
                        mesh = chr.BuildMesh(boneArray, index);
                        meshDict.Add(index, mesh);
                    }

                    chr.BuildCharacter(mesh, material, boneArray);
                }
                );
        }
    }
}