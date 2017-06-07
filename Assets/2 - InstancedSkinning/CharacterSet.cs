namespace Example.InstancedSkinning
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CharacterSet : MonoBehaviour
    {
        public Material material;

        //public Shader shader;
        public TextureArray.Texture2DArrayManager texArrayManager;

        public bool receiveShadow;
        public bool castShadow;

        Character[] charArray;
        Dictionary<int, Mesh> meshDict;
        Matrix4x4[] boneMatrixArray;
        Vector4[] bonePositionArray;

        void Awake()
        {
            charArray = GetComponentsInChildren<Character>();

            boneMatrixArray = new Matrix4x4[CharacterData.boneCount * charArray.Length];
            bonePositionArray = new Vector4[CharacterData.boneCount * charArray.Length];
            
            meshDict = new Dictionary<int, Mesh>();

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

                    chr.BuildCharacter(mesh, material, boneArray, receiveShadow, castShadow);
                }
                );
        }
    }
}
