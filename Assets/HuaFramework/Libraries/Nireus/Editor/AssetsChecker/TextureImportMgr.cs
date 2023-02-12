using System;
using System.Collections;
using System.Collections.Generic;
using EditorCommon;
using UnityEditor;
using UnityEngine;

public class TextureImportMgr : AssetPostprocessor
{
    private static TextureModifyByFileAssets textureModifyByFileAssets;
    private static TextureFirstImportDataAssets textureFirstImportDataAssets;
    private const string texture_modify_by_file_config_path = "Assets/Scripts/Nireus/Editor/AssetsChecker/TextureModifyByFileConfig.asset";
    private const string texture_first_import_config_path = "Assets/Scripts/Nireus/Editor/AssetsChecker/TextureFirstImportDataAssets.asset";
    private static HashSet<string> importPath = new HashSet<string>();
    private static void OnPostprocessAllAssets(
        string[] importedAssets, 
        string[] deletedAssets, 
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (textureModifyByFileAssets == null)
        {
            textureModifyByFileAssets = AssetDatabase.LoadAssetAtPath<TextureModifyByFileAssets>(texture_modify_by_file_config_path);
        }
        if (textureFirstImportDataAssets == null)
        {
            textureFirstImportDataAssets = AssetDatabase.LoadAssetAtPath<TextureFirstImportDataAssets>(texture_first_import_config_path);
        }

        fixTexture(importedAssets);
        fixTexture(movedAssets);
    }
    
    static void fixTexture(string[] importedAssets)
    {
        foreach (var path in importedAssets)
        {
            if (importPath.Contains(path))
            {
                importPath.Remove(path);
                continue;
            }
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter == null)
            {
                continue;
            }
            textureImporter.mipmapEnabled = false;
            textureImporter.isReadable = false;
            FindAndMOdifyTexture(path,path);
        }
    }

    static void FindAndMOdifyTexture(string oriPath,string operatePath)
    {
        int pathTemp = operatePath.LastIndexOf('/');
        
        if (pathTemp != -1)
        {
            string temp = operatePath.Substring(0,pathTemp);
            TextureModifyByFileData dataTemp = textureModifyByFileAssets.GetData(temp);
            if (dataTemp != null)
            {
                //只第一次导入时 按照配置设置
                if(dataTemp.IsFirstImport)
                {
                    if(textureFirstImportDataAssets!=null)
                    {
                        if(textureFirstImportDataAssets.DataList.Contains(oriPath))
                        {
                            return;
                        }
                    }
                }
                TextureImporter textureImporter = AssetImporter.GetAtPath(oriPath) as TextureImporter;
                if (dataTemp.IsOverWriteReadWriteEnable)
                {
                    textureImporter.isReadable = dataTemp.ReadWriteEnable;     
                }

                if (dataTemp.IsOverWriteMipmapEnable)
                {
                    textureImporter.mipmapEnabled = dataTemp.MipmapEnable;
                }

                if (dataTemp.IsOverWriteImportType)
                {
                    textureImporter.textureType = dataTemp.ImportType;    
                }

                if (dataTemp.IsOverWriteImportShape)
                {
                    textureImporter.textureShape = dataTemp.ImportShape;    
                }

                if (dataTemp.IsOverWriteWrapMode)
                {
                    textureImporter.wrapMode = dataTemp.WrapMode;
                }

                if (dataTemp.IsOverWriteFilterMode)
                {
                    textureImporter.filterMode = dataTemp.FilterMode;    
                }

                if (dataTemp.IsOverWriteAndroidFormat)
                {
                    TextureImporterPlatformSettings
                        settingAndroid = textureImporter.GetPlatformTextureSettings(EditorConst.PlatformAndroid);
                    settingAndroid.overridden = true;
                    settingAndroid.format = dataTemp.AndroidFormat;
                    textureImporter.SetPlatformTextureSettings(settingAndroid);
                    
                }
                else
                {
                    TextureImporterPlatformSettings
                        settingAndroid = textureImporter.GetPlatformTextureSettings(EditorConst.PlatformAndroid);
                    settingAndroid.overridden = false;
                    textureImporter.SetPlatformTextureSettings(settingAndroid);
                }

                if (dataTemp.IsOverWriteIosFormat)
                {
                    TextureImporterPlatformSettings settingIos =
                        textureImporter.GetPlatformTextureSettings(EditorConst.PlatformIos);
                    settingIos.overridden = true;
                    settingIos.format = dataTemp.IosFormat;
                    textureImporter.SetPlatformTextureSettings(settingIos);
                }
                else
                {
                    TextureImporterPlatformSettings settingIos = textureImporter.GetPlatformTextureSettings(EditorConst.PlatformIos);
                    settingIos.overridden = false;
                    textureImporter.SetPlatformTextureSettings(settingIos);
                }
                if(dataTemp.IsOverWriteWidth)
                {
                    Texture2D tenxture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(oriPath);
                    if(tenxture2D.width != dataTemp.Width)
                    {
                        Debug.LogError("texture import width ="+ tenxture2D.width+" is error,path="+ oriPath);
                        //error_importPath.Add(oriPath);
                    }

                }
                if(dataTemp.IsOverWriteHeight)
                {
                    Texture2D tenxture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(oriPath);
                    if (tenxture2D.height != dataTemp.Height)
                    {
                        Debug.LogError("texture import height = "+ tenxture2D.height + " is error,path=" + oriPath);
                        //error_importPath.Add(oriPath);
                    }
                }
                if(dataTemp.IsFirstImport)
                {
                    if (textureFirstImportDataAssets != null)
                    {
                        textureFirstImportDataAssets.DataList.Add(oriPath);      
                    }
                }
                importPath.Add(oriPath);
                AssetDatabase.ImportAsset(oriPath);
            }
            else
            {
                FindAndMOdifyTexture(oriPath,temp);
            }
        }
    }
}
