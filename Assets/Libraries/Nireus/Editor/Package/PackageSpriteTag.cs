using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Nireus.Editor
{
    class PackageSpriteTag : UnityEditor.Editor
    {

        public static readonly string UI_RESOUCE_ROOT = "Assets/Textures/UI/";

        public static void Package()
        {
            PackageSpriteTags();
        }

        //[MenuItem("Nireus/Make Sprite Package Tag", false, 3)]
        public static void PackageSpriteTags()
        {
            PackTags(UI_RESOUCE_ROOT, UI_RESOUCE_ROOT);
            UnityEngine.Debug.Log("package Sprite tag succ");
        }


        static void PackTags(string asset_root, string cur_dir)
        {
            DirectoryInfo directory_info = new DirectoryInfo(cur_dir);
            FileInfo[] files = directory_info.GetFiles();
            DirectoryInfo[] dirs = directory_info.GetDirectories();
            foreach (var item in files)
            {
                if (item.Extension != ".meta" && item.Extension != ".DS_Store")
                {
                    SetTag(item.FullName);
                }
            }

            foreach (var item in dirs)
            {
                PackTags(asset_root, item.FullName);
            }

        }

        static void SetTag(string full_path_name)
        {
            string asset_patch = GetAssetPath(full_path_name);

            var import = AssetImporter.GetAtPath(asset_patch);

            var texture_impl = import as TextureImporter;
            if (texture_impl != null)
            {
                string tag = Path.GetFileName(Path.GetDirectoryName(asset_patch));
                texture_impl.spritePackingTag = tag;
                TextureImporterSettings settings = new TextureImporterSettings();

                bool changed = false;
                texture_impl.ReadTextureSettings(settings);
                if(settings.spriteGenerateFallbackPhysicsShape)
                {
                    settings.spriteGenerateFallbackPhysicsShape = false;
                    changed = true;
                }
                if (settings.spriteMeshType != UnityEngine.SpriteMeshType.FullRect)
                {
                    settings.spriteMeshType = UnityEngine.SpriteMeshType.FullRect;
                    changed = true;
                }
                if (changed)
                {
                    texture_impl.SetTextureSettings(settings);
                    texture_impl.SaveAndReimport();
                }
            }
        }


        static string GetAssetPath(string full_path_name)
        {
            string assetBundleName = full_path_name.Replace("\\", "/");
            assetBundleName = assetBundleName.Substring(assetBundleName.IndexOf(UI_RESOUCE_ROOT) + UI_RESOUCE_ROOT.Length);
            string assetPath = UI_RESOUCE_ROOT + assetBundleName;//unity 导入路径
            return assetPath;
        }

    }
}