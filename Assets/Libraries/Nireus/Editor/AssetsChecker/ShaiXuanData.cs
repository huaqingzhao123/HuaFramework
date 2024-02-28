using System;
using System.Collections.Generic;
using System.Linq;
using EditorCommon;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using ObjectFieldAlignment = Sirenix.OdinInspector.ObjectFieldAlignment;

[Serializable]
public class ShaiXuanData
{
    private static HashSet<string> atlasSpritesPath = new HashSet<string>();
    [LabelText("是否排除图集中的图片")]
    [OnValueChanged("CollectAtlasSprite")]
    [ToggleLeft]
    public bool IsPaiChuAtlas = false;
    [HorizontalGroup("select")]
    [LabelText("是否使用批量选中修改")]
    [ToggleLeft]
    public bool IsCanMultiSelect = false;

    [HorizontalGroup("select")]
    [Button("取消所有选择")]
    public void UnSelect()
    {
        for (int i = 0; i < TextureModifyEditor.TableListWithIndexLabels.Count; i++)
        {
            TextureData textureData = TextureModifyEditor.TableListWithIndexLabels[i];
            textureData.Is_select = false;
        }
    }
    [HideLabel]
    [HorizontalGroup("TextureImporterType")]
    public bool IsUseTextureImportType = false;
    [HorizontalGroup("TextureImporterType")]
    public TextureImporterType textureImportType;

    [HideLabel]
    [HorizontalGroup("ReadWriteEnable")]
    public bool IsUseReadWrite = false;
    [HorizontalGroup("ReadWriteEnable")]
    public bool ReadWriteEnable = false;

    [HideLabel]
    [HorizontalGroup("MipMapEnable")]
    public bool IsUseMipMap = false;
    [HorizontalGroup("MipMapEnable")]
    public bool MipMapEnable = false;

    [HideLabel]
    [HorizontalGroup("AndroidFormat")]
    public bool IsUseAndroidFormat = false;
    [HorizontalGroup("AndroidFormat")]
    public TextureImporterFormat AndroidFormat = TextureImporterFormat.AutomaticCompressed;

    [HideLabel]
    [HorizontalGroup("IosFormat")]
    public bool IsUseIosFormat = false;
    [HorizontalGroup("IosFormat")]
    public TextureImporterFormat IosFormat = TextureImporterFormat.AutomaticCompressed;

    [HideLabel]
    [HorizontalGroup("TextureWrapMode")]
    public bool IsUseTextureWrapMode = false;
    [HorizontalGroup("TextureWrapMode")]
    public My_TextureWrapMode textureWrapMode = My_TextureWrapMode.Default;

    [HideLabel]
    [HorizontalGroup("FilterMode")]
    public bool IsUseFilterMode = false;
    [HorizontalGroup("FilterMode")]
    public My_FilterMode filterMode = My_FilterMode.Default;

    [HideLabel]
    [HorizontalGroup("TextureImporterShape")]
    public bool IsUseTextureImporterShape = false;
    [HorizontalGroup("TextureImporterShape")]
    public TextureImporterShape ImportShape = TextureImporterShape.Texture2D;

    private string AndroidMemoryName = "Android Memory ▲";
    private bool isAndroidMemoryDown = false;

    [HorizontalGroup("Tittle", LabelWidth = 80)]
    [Button("$AndroidMemoryName")]
    private void OrderByAndroidMemory()
    {
        isAndroidMemoryDown = !isAndroidMemoryDown;
        if (isAndroidMemoryDown)
        {
            AndroidMemoryName = "Android Memory ▼";
        }
        else
        {
            AndroidMemoryName = "Android Memory ▲";
        }
    }

    private string IOSMemoryName = "IOS Memory ▲";
    private bool isIOSMemoryDown = false;

    [HorizontalGroup("Tittle", LabelWidth = 80)]
    [Button("$IOSMemoryName")]
    private void OrderByIOSMemory()
    {
        isIOSMemoryDown = !isIOSMemoryDown;
        if (isIOSMemoryDown)
        {
            IOSMemoryName = "IOS Memory ▼";
        }
        else
        {
            IOSMemoryName = "IOS Memory ▲";
        }
    }
    [HorizontalGroup("error_format")]
    [LabelText("是否不显示安卓格式错误图片")]
    [ToggleLeft]
    public bool IsNotShowAndroidErrorFormat = false;
    [LabelText("是否不显示IOS格式错误图片")]
    [HorizontalGroup("error_format")]
    [ToggleLeft]
    public bool IsNotShowIOSErrorFormat = false;
    [ToggleLeft]
    [LabelText("是否使用包括/排除文件夹操作")]
    public bool IsUseOperaFile = false;

    [LabelText("排除文件夹")]
    [ShowIf("IsShowFileParam")]
    public List<string> RemoveFileList = new List<string>();

    [LabelText("只查找文件夹")]
    [ShowIf("IsShowFileParam")]
    public List<string> OnFileList = new List<string>();

    [HorizontalGroup("ShaiXuanButton")]
    [Button("筛选", ButtonSizes.Medium)]
    [GUIColor(0, 1f, 0)]
    private void RefreshShaiXuan()
    {
        TextureModifyEditor.TableListWithOrder.Clear();

        for (int i = 0; i < TextureModifyEditor.TableListWithIndexLabels.Count; i++)
        {
            TextureData textureData = TextureModifyEditor.TableListWithIndexLabels[i];
            //文件夹操作
            if (IsUseOperaFile)
            {
                if (RemoveFileList.Count != 0)
                {
                    bool contineFlag = false;
                    for (int j = 0; j < RemoveFileList.Count; j++)
                    {
                        string removePath = RemoveFileList[j].Replace("\\", "/");
                        if (removePath != "")
                        {
                            int index = textureData.Path.IndexOf(removePath);
                            if (index == 0)
                            {
                                contineFlag = true;
                                continue;
                            }
                        }

                    }
                    if (contineFlag)
                    {
                        continue;
                    }

                }
                if (OnFileList.Count != 0)
                {
                    bool exitFlag = false;
                    for (int j = 0; j < OnFileList.Count; j++)
                    {
                        string onPath = OnFileList[j].Replace("\\", "/");
                        if (onPath != "")
                        {
                            int index = textureData.Path.IndexOf(onPath);
                            if (index == 0)
                            {
                                exitFlag = true;
                                continue;
                            }
                        }
                    }
                    if (!exitFlag)
                    {
                        continue;
                    }
                }
            }
            //是否再图集中
            if (IsPaiChuAtlas)
            {
                if (ShaiXuanData.IsExitInAtlas(textureData.Path))
                {
                    continue;
                }
            }
            if(IsUseTextureImportType)
            {
                if (textureData.ImportType != textureImportType)
                {
                    continue;
                }
            }
            if(IsUseReadWrite)
            {
                if (textureData.ReadWriteEnable != ReadWriteEnable)
                {
                    continue;
                }
            }
            if(IsUseMipMap)
            {
                if (textureData.MipmapEnable != MipMapEnable)
                {
                    continue;
                }
            }

            if(IsUseAndroidFormat)
            {
                if (textureData.AndroidFormat != AndroidFormat)
                {
                    continue;
                }
            }
            if(IsUseIosFormat)
            {
                if (textureData.IosFormat != IosFormat)
                {
                    continue;
                }
            }
            if(IsUseTextureWrapMode)
            {
                if ((int)textureData.WrapMode != (int)textureWrapMode)
                {
                    continue;
                }
            }
            if(IsUseFilterMode)
            {
                if ((int)textureData.FilterMode != (int)filterMode)
                {
                    continue;
                }
            }
            if(IsUseTextureImporterShape)
            {
                if (textureData.ImportShape != ImportShape)
                {
                    continue;
                }
            }            

            TextureModifyEditor.TableListWithOrder.Add(textureData);
        }
        if (TextureModifyEditor.TableListWithOrder.Count != 1)
        {
            TextureModifyEditor.TableListWithOrder.Sort(SortByOrder);
        }
    }
    private bool IsShowFileParam()
    {
        return IsUseOperaFile;
    }
    [HorizontalGroup("ShaiXuanButton")]
    [Button("筛选错误格式", ButtonSizes.Medium)]
    [GUIColor(1f, 0, 0)]
    private void ShaiXuanError()
    {
        TextureModifyEditor.TableListWithOrder.Clear();

        for (int i = 0; i < TextureModifyEditor.TableListWithIndexLabels.Count; i++)
        {
            TextureData textureData = TextureModifyEditor.TableListWithIndexLabels[i];
            //是否再图集中
            if (IsPaiChuAtlas)
            {
                if (ShaiXuanData.IsExitInAtlas(textureData.Path))
                {
                    continue;
                }
            }
            if (IsNotShowAndroidErrorFormat)
            {
                if (textureData.IsAndroidFormatError && !textureData.IsIOSFormatError)
                {
                    continue;
                }
            }
            if (IsNotShowIOSErrorFormat)
            {
                if (!textureData.IsAndroidFormatError && textureData.IsIOSFormatError)
                {
                    continue;
                }
            }
            if (IsUseTextureImportType)
            {
                if (textureData.ImportType != textureImportType)
                {
                    continue;
                }
            }
            if (IsUseReadWrite)
            {
                if (textureData.ReadWriteEnable != ReadWriteEnable)
                {
                    continue;
                }
            }
            if (IsUseMipMap)
            {
                if (textureData.MipmapEnable != MipMapEnable)
                {
                    continue;
                }
            }

            if (IsUseAndroidFormat)
            {
                if (textureData.AndroidFormat != AndroidFormat)
                {
                    continue;
                }
            }
            if (IsUseIosFormat)
            {
                if (textureData.IosFormat != IosFormat)
                {
                    continue;
                }
            }
            if (IsUseTextureWrapMode)
            {
                if ((int)textureData.WrapMode != (int)textureWrapMode)
                {
                    continue;
                }
            }
            if (IsUseFilterMode)
            {
                if ((int)textureData.FilterMode != (int)filterMode)
                {
                    continue;
                }
            }
            if (IsUseTextureImporterShape)
            {
                if (textureData.ImportShape != ImportShape)
                {
                    continue;
                }
            }
            if (textureData.IsAndroidFormatError || textureData.IsIOSFormatError)
            {
                TextureModifyEditor.TableListWithOrder.Add(textureData);
            }
        }
        if (TextureModifyEditor.TableListWithOrder.Count != 1)
        {
            TextureModifyEditor.TableListWithOrder.Sort(SortByOrder);
        }
    }
    private void CollectAtlasSprite()
    {
        atlasSpritesPath.Clear();
        var atlasGuids = AssetDatabase.FindAssets("t:SpriteAtlas");
        foreach (var atlasGuid in atlasGuids)
        {
            string atlasPath = AssetDatabase.GUIDToAssetPath(atlasGuid);
            string[] atlasDependence = AssetDatabase.GetDependencies(atlasPath);
            for (int i = 0; i < atlasDependence.Length; i++)
            {
                string path = atlasDependence[i];
                if (path.EndsWith(".png") || path.EndsWith(".jpg"))
                {
                    if (atlasSpritesPath.Contains(atlasDependence[i]))
                    {
                        Debug.LogError(path + " is already in other atlas");
                    }
                    else
                    {
                        atlasSpritesPath.Add(path);
                    }
                }

            }
        }
    }

    public static bool IsExitInAtlas(string path)
    {
        return atlasSpritesPath.Contains(path);
    }
    private int SortByOrder(TextureData a, TextureData b)
    {
        if (isAndroidMemoryDown)
        {
            if (a.AndroidSize > b.AndroidSize)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        if (isIOSMemoryDown)
        {
            if (a.IosSize > b.IosSize)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
        return 0;
    }
}