namespace Example.TextureArray
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Texture2DArray", fileName = "TextureArray")]
    public class Texture2DArrayManager : ScriptableObject
    {
        public Texture2D[] textureInputArray;
        public Texture2DArray texture2DArray;

        void OnEnable()
        {
            if (texture2DArray == null)
                texture2DArray = GenerateTexture2DArray(textureInputArray);
        }

        public static Texture2DArray GenerateTexture2DArray(Texture2D[] textureInputArray, TextureFormat defaultFormat = TextureFormat.RGBA32)
        {
            Texture2D[] tex2DArray = Array.FindAll(textureInputArray, tex => tex != null);

            if (tex2DArray.Length <= 0)
            {
                return null;
            }

            Texture2D firstTex = tex2DArray[0];
            int width = firstTex.width, height = firstTex.height;

            if (!Array.TrueForAll(tex2DArray, (tex) => tex.width == width && tex.height == height))
            {
                return null;
            }

            TextureFormat format = firstTex.format;

            if (!Array.TrueForAll(tex2DArray, (tex) => tex.format == format))
            {
                format = defaultFormat;
            }

            Texture2DArray realArray = new Texture2DArray(width, height, tex2DArray.Length, format, false);
            realArray.name = "Texture2DArray";

            for (int i = 0; i < tex2DArray.Length; i++)
                for (int j = 0; j < tex2DArray[i].mipmapCount; j++)
                    Graphics.CopyTexture(tex2DArray[i], 0, j, realArray, i, j);

            realArray.Apply(true);

            return realArray;
        }
    }

}