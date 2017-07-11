namespace Example.InstancedSkinning
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CharacterSet : MonoBehaviour
    {
        public bool isInstanced = false;

        [Header("Variable for DrawMeshInstanced"), Space()]

        public Material material;

        //public Shader shader;
        public TextureArray.Texture2DArrayManager texArrayManager;

        public bool receiveShadow;
        public bool castShadow;

        [Range(0, 31)]
        public int drawLayerNumber;
        public Camera drawCamera;

        [System.Serializable]
        public struct DrawData
        {
            public Mesh mesh;
            public float textureIndex;
            public MaterialPropertyBlock block;

            public List<Character> characterList;
            public List<Matrix4x4> mainMatrixList;

            public List<Transform[]> boneTransformList;
            public Matrix4x4[][] boneRSMatrix;
            public Vector4[][] bonePosition;

            public DrawData(Mesh _mesh, float _texutreIndex)
            {
                mesh = _mesh;
                textureIndex = _texutreIndex;

                characterList = new List<Character>();
                mainMatrixList = new List<Matrix4x4>();

                boneTransformList = new List<Transform[]>();

                block = new MaterialPropertyBlock();

                boneRSMatrix = new Matrix4x4[CharacterData.boneCount][];
                bonePosition = new Vector4[CharacterData.boneCount][];
            }

            public void BoneSetting()
            {
                for (int i = 0; i < boneRSMatrix.Length; i++)
                    boneRSMatrix[i] = new Matrix4x4[boneTransformList.Count];

                for (int i = 0; i < bonePosition.Length; i++)
                    bonePosition[i] = new Vector4[boneTransformList.Count];
            }

            public void UpdateMatrix()
            {
                if (characterList.Count != mainMatrixList.Count)
                {
                    mainMatrixList.Clear();

                    for (int i = 0; i < characterList.Count; i++)
                        mainMatrixList.Add(characterList[i].transform.localToWorldMatrix);
                }
                else
                {
                    for (int i = 0; i < characterList.Count; i++)
                        mainMatrixList[i] = characterList[i].transform.localToWorldMatrix;
                }
            }

            public void UpdateMaterialblcok()
            {
                block.SetFloat("_TextureIndex", textureIndex);

                for (int i = 0; i < characterList.Count; i++)
                {
                    Character.InstancedData data = characterList[i].GetInstancedData();

                    for (int j = 0; j < CharacterData.boneCount; j++)
                    {
                        boneRSMatrix[j][i] = data.boneTransformMatrix[j];
                        bonePosition[j][i] = data.bonePosition[j];
                    }
                }

                for (int j = 0; j < CharacterData.boneCount; j++)
                {
                    block.SetMatrixArray(boneMatrixNameArray[j], boneRSMatrix[j]);
                    block.SetVectorArray(bonePositionNameArray[j], bonePosition[j]);
                }
            }

            private static string[] boneMatrixNameArray;
            private static string[] bonePositionNameArray;

            public static void LoadBlockParameterName(int count)
            {
                boneMatrixNameArray = new string[count];
                bonePositionNameArray = new string[count];

                for (int i = 0; i < CharacterData.boneCount; i++)
                {
                    boneMatrixNameArray[i] = string.Format("_BoneTransfromMatrix{0}", i);
                    bonePositionNameArray[i] = string.Format("_BonePosition{0}", i);
                }
            }
        }
        
        Character[] charArray;
        Dictionary<CharacterData, DrawData> drawDataDict;

        void Awake()
        {
            charArray = GetComponentsInChildren<Character>();
            drawDataDict = new Dictionary<CharacterData, DrawData>();

            DrawData.LoadBlockParameterName(CharacterData.boneCount);

            for (int i = 0; i < charArray.Length; i++)
            {
                DrawData data;
                Character chr = charArray[i];
                Transform[] boneArray = chr.BuildBone();

                if (drawDataDict.ContainsKey(chr.data))
                {
                    data = drawDataDict[chr.data];

                    data.characterList.Add(chr);
                    data.boneTransformList.Add(chr.bones);
                }
                else
                {
                    data = new DrawData(
                        Character.BuildMesh(chr.data),
                        Array.FindIndex(texArrayManager.textureInputArray, (tex) => tex.Equals(chr.data.texture)));

                    data.characterList.Add(chr);
                    data.boneTransformList.Add(chr.bones);

                    drawDataDict.Add(chr.data, data);
                }

                chr.BuildCharacter(data.mesh, material, boneArray);
            }

            var enumer = drawDataDict.GetEnumerator();

            while (enumer.MoveNext())
            {
                enumer.Current.Value.BoneSetting();
            }
        }

        void Update()
        {
            var enumer = drawDataDict.GetEnumerator();

            if (isInstanced)
            {
                while (enumer.MoveNext())
                {
                    DrawData data = enumer.Current.Value;

                    data.UpdateMatrix();
                    data.UpdateMaterialblcok();

                    Graphics.DrawMeshInstanced(
                            data.mesh,
                            0,
                            material,
                            data.mainMatrixList,
                            data.block,
                            castShadow ?
                                UnityEngine.Rendering.ShadowCastingMode.On :
                                UnityEngine.Rendering.ShadowCastingMode.Off,
                            receiveShadow,
                            drawLayerNumber,
                            drawCamera
                        );
                }
            }
            else
            {
                while (enumer.MoveNext())
                {
                    DrawData data = enumer.Current.Value;

                    data.UpdateMatrix();
                    data.UpdateMaterialblcok();

                    data.mainMatrixList.ForEach(
                        (matrix) =>
                        {
                            Graphics.DrawMesh(data.mesh, matrix, material, drawLayerNumber, drawCamera, 0, data.block);
                        }
                        );
                }
            }
        }
    }
}
