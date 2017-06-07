namespace Example.TextureArray
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System.Linq;
    using System.Collections;
    using System;
    using System.Text;

    [CustomEditor(typeof(Texture2DArrayManager))]
    public class Texture2DArrayManagerEditor : Editor
    {
        Texture2DArrayManager targetAs { get { return target as Texture2DArrayManager; } }

        void OnEnable()
        {
            Texture2DArrayManager manager = targetAs;

            if (manager.texture2DArray == null && manager != null)
            {
                manager.texture2DArray = AddTexture2DArrayAt(target);
            }
        }

        static StringBuilder errorBuilder = new StringBuilder();

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            Texture2DArrayManager manager = targetAs;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject(manager), typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();

            SerializedProperty textureProperty = serializedObject.FindProperty("textureInputArray");

            EditorGUILayout.PropertyField(textureProperty, true);
            EditorGUILayout.Space();

            if (GUILayout.Button("Refresh"))
            {
                ApplyTexture2DArray(manager);
            }

            serializedObject.ApplyModifiedProperties();
        }

        public static bool ApplyTexture2DArray(Texture2DArrayManager manager)
        {
            Texture2D[] tex2DArray = Array.FindAll(manager.textureInputArray, tex => tex != null);

            if (tex2DArray.Length <= 0)
            {
                errorBuilder.Remove(0, errorBuilder.Length);
                Array.ForEach(tex2DArray, (tex) => errorBuilder.Append(tex.name).Append(" : ").Append(tex.width).Append(',').Append(tex.height).Append('\n'));

                Debug.LogErrorFormat("Fail to apply.. not exist texture in array.");
                return false;
            }

            Texture2D firstTex = tex2DArray[0];
            int width = firstTex.width, height = firstTex.height;

            if (!Array.TrueForAll(tex2DArray, (tex) => tex.width == width && tex.height == height))
            {
                errorBuilder.Remove(0, errorBuilder.Length);
                Array.ForEach(tex2DArray, (tex) => errorBuilder.Append(tex.name).Append(" : ").Append(tex.width).Append(',').Append(tex.height).Append('\n'));

                Debug.LogErrorFormat("Fail to apply.. all texture size must be same.\n{0}", errorBuilder.ToString());
                return false;
            }

            TextureFormat format = firstTex.format;

            if (!Array.TrueForAll(tex2DArray, (tex) => tex.format == format))
            {
                format = TextureFormat.RGBA32;

                errorBuilder.Remove(0, errorBuilder.Length);
                Array.ForEach(tex2DArray, (tex) => errorBuilder.Append(tex.name).Append(" : ").Append(tex.format).Append('\n'));

                Debug.LogWarningFormat("All texture format is not same. Force {0}.{1} \n{2}", format.GetType().ToString(), format.ToString(), errorBuilder.ToString());
            }

            Texture2DArray realArray = manager.texture2DArray;

            if (IsDifferent(manager))
            {
                DestroyImmediate(manager.texture2DArray, true);

                realArray = new Texture2DArray(width, height, tex2DArray.Length, format, false);
                realArray.name = "Texture2DArray";

                AssetDatabase.AddObjectToAsset(realArray, manager);

                manager.texture2DArray = realArray;

                Debug.LogWarning("Texture2DArray instance is re-created.");
            }

            for (int i = 0; i < tex2DArray.Length; i++)
                for (int j = 0; j < tex2DArray[i].mipmapCount; j++)
                    Graphics.CopyTexture(tex2DArray[i], 0, j, realArray, i, j);

            realArray.Apply(true);

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(manager));
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            return true;
        }

        public static Texture2DArray AddTexture2DArrayAt(UnityEngine.Object parentAsset)
        {
            Texture2DArray realArray = new Texture2DArray(512, 512, 1, TextureFormat.RGBA32, false);

            realArray.name = "Texture2DArray";

            AssetDatabase.AddObjectToAsset(realArray, parentAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            return realArray;
        }

        public static bool IsDifferent(Texture2DArrayManager mng)
        {
            return
                mng.texture2DArray.depth != mng.textureInputArray.Length ||
                (mng.textureInputArray.Length > 0 ? mng.texture2DArray.format != mng.textureInputArray[0].format : false) ||
                !Array.TrueForAll(mng.textureInputArray, (tex) => tex.width == mng.texture2DArray.width && tex.height == mng.texture2DArray.height);
        }
    }

    [CustomEditor(typeof(Texture2DArray))]
    public class Texture2DArrayEditor : Editor
    {
        Texture2DArray targetAs { get { return target as Texture2DArray; } }
        Texture2D[] textures;

        void OnEnable()
        {
            Texture2DArray array = targetAs;

            textures = new Texture2D[array.depth];

            for (int i = 0; i < textures.Length; i++)
            {
                textures[i] = new Texture2D(array.width, array.height, array.format, false);
                Graphics.CopyTexture(array, i, 0, textures[i], 0, 0);
            }
        }

        void OnDisable()
        {
            Array.ForEach(textures, (tex) => DestroyImmediate(tex));
        }

        int textureIndex = 0;

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);

            Texture2DArray array = targetAs;

            EditorGUILayout.IntField("Width", array.width);
            EditorGUILayout.IntField("Height", array.height);
            EditorGUILayout.IntField("Depth", array.depth);
            EditorGUILayout.EnumPopup("Format", array.format);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUI.EndDisabledGroup();

            textureIndex = EditorGUILayout.IntSlider("Texture Index", textureIndex, 0, textures.Length - 1);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            Texture2D texture = textures[textureIndex];

            Rect rect = EditorGUILayout.BeginVertical();

            rect.height = rect.width / texture.width * texture.height / 2;
            rect.width = rect.width / 2;

            rect.x += rect.width / 2;

            EditorGUI.DrawTextureTransparent(rect, texture);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }

}