using UnityEngine;
using UnityEditor;
using System;


namespace Nireus.Editor
{
	class UIResurcesImportor : AssetPostprocessor
	{
		static void OnPostprocessAllAssets(string[] import_assets, string[] deleted_assets, string[] moved_assets, string[] moved_from_asset_paths)
		{
			for (int i = 0; i < import_assets.Length; ++i)
			{
				try
				{
                    if (import_assets[i].IndexOf("UI") != -1 && (  import_assets[i].EndsWith(".png") || import_assets[i].EndsWith(".jpg")))
					{
						// 图片，设置为Sprite;
						TextureImporter txt_import = AssetImporter.GetAtPath(import_assets[i]) as TextureImporter;
						if (txt_import == null || txt_import.textureType == TextureImporterType.Sprite) continue;

                        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(import_assets[i]);
                        txt_import.textureType = TextureImporterType.Sprite;
                        txt_import.mipmapEnabled = false;
                        //图片太小不转成硬件平台的格式,避免模糊.
                        if (texture.width <= 128 && texture.height <= 128 && Mathf.IsPowerOfTwo(texture.width) && Mathf.IsPowerOfTwo(texture.height))
                        {
                            TextureImporterPlatformSettings set = new TextureImporterPlatformSettings();                            
                            set.overridden = true;
                            set.format = TextureImporterFormat.RGBA32;
                            txt_import.SetPlatformTextureSettings(set);
                        }

                        AssetDatabase.ImportAsset(import_assets[i]);
					}
				}
				catch (System.Exception ex)
				{
					Debug.LogError("Import Asset File: " + import_assets[i] + "Failed, exception: " + ex.Message);
				}
			}
		}
	}
}
