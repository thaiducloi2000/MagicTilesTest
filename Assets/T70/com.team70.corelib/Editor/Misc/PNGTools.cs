using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace nano
{
    public class PNGTools
    {
        [MenuItem("T70/Tools/Split MultiSprites Texture")]
        public static void SplitSprites()
        {
            var s = Selection.activeObject;
            if (!(s is Texture2D))
            {
                Debug.LogWarning("You must select a Texture2D");
                return;
            }

            var baseTex = s as Texture2D;
            bool revertReadable = false;
            
            string path = AssetDatabase.GetAssetPath(s);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            
            if (!baseTex.isReadable)
            {
                revertReadable = true;
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
            
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            
            var basePath = path.Substring(0, path.LastIndexOf('.'));
            for (var i = 0; i < sprites.Length; i++)
            {
                Texture2D tex = CreateTextureFromSprite(sprites[i]);
                var cPath = basePath + "_" + sprites[i].name + ".png";
                File.WriteAllBytes(cPath, tex.EncodeToPNG());
                
                // STUPID FIRST !!!
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                
                var cImporter = (TextureImporter)AssetImporter.GetAtPath(cPath);
                cImporter.textureType = TextureImporterType.Sprite;
                cImporter.alphaSource = importer.alphaSource;
                cImporter.anisoLevel = importer.anisoLevel;
                cImporter.alphaIsTransparency = true;
                cImporter.npotScale = TextureImporterNPOTScale.None;
                cImporter.mipmapEnabled = importer.mipmapEnabled;
                cImporter.textureCompression = TextureImporterCompression.Uncompressed;
                cImporter.spritePackingTag = importer.spritePackingTag;
                cImporter.maxTextureSize = importer.maxTextureSize;
                cImporter.spriteBorder = sprites[i].border;
                cImporter.SaveAndReimport(); 
            }
            
            if (revertReadable)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }
            
            AssetDatabase.Refresh();
        }
        
        public static Texture2D CreateTextureFromSprite(Sprite sprite)
        {
            var newTex = new Texture2D((int)sprite.rect.width,(int)sprite.rect.height);
            Color[] pixels = sprite.texture.GetPixels
            (
                (int)sprite.rect.x, 
                (int)sprite.rect.y, 
                (int)sprite.rect.width, 
                (int)sprite.rect.height
            );
            
            newTex.SetPixels(pixels);
            newTex.Apply();
            return newTex;
        }
    }    
}

