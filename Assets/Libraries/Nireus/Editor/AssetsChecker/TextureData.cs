using System;
using System.Collections.Generic;
using System.Linq;
using APlus;
using EditorCommon;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using ObjectFieldAlignment = Sirenix.OdinInspector.ObjectFieldAlignment;
//图片格式
[Serializable]
public class TextureData
{
    public enum TextureImportDataType
    {
        TextureImporterType,
        TextureImporterShape,
        ReadAnaWrite,
        MimMap,
        WrapMode,
        FilterMode,
        AndroidFormat,
        IOSFormat,
    }
    [TableColumnWidth(50, Resizable = false)]
    [HideIf("Is_CanMultSelect")]
    public bool Is_select = false;
    [TableColumnWidth(100, Resizable = false)]
    //[AssetsOnly]
    [OnClickMethod("ClickTexture")]
    [PreviewField(40, ObjectFieldAlignment.Center)]
    public UnityEngine.Object Obj;

    //[TableColumnWidth(200, Resizable = false)]
    //[DisplayAsString]
    [HideInInspector]
    public string Path;

    [TableColumnWidth(65, Resizable = false)]
    [OnValueChanged("ReImportTextureWithTextureType")]
    public TextureImporterType ImportType;

    [TableColumnWidth(100, Resizable = false)]
    [CustomValueDrawer("SizeDraw")]
    public string Size;

    [OnValueChanged("ReImportTextureWithAndroidFormat")]
    [TableColumnWidth(100, Resizable = true)]
    public TextureImporterFormat AndroidFormat;

    [TableColumnWidth(100, Resizable = false)]
    [DisplayAsString]
    public string AndroidMemory;

    [TableColumnWidth(100, Resizable = true)]
    [OnValueChanged("ReImportTextureWithIOSFormat")]
    public TextureImporterFormat IosFormat;

    [TableColumnWidth(100, Resizable = false)]
    [DisplayAsString]
    public string IosMemory;

    [TableColumnWidth(100, Resizable = false)]
    [OnValueChanged("ReImportTextureWithReadWrite")]
    //[ValidateInput("CheckReadWriteEnable", "一般不勾选", InfoMessageType.Error)]
    [GUIColor("CheckReadWriteEnableColor")]
    public bool ReadWriteEnable = false;

    [TableColumnWidth(100, Resizable = false)]
    [OnValueChanged("ReImportTextureWithMipMap")]
    [GUIColor("MipmapColor")]
    //[ValidateInput("CheckMipmapEnable", "一般不勾选", InfoMessageType.Error)]
    public bool MipmapEnable = false;

    [TableColumnWidth(100, Resizable = false)]
    [OnValueChanged("ReImportTextureWithWrapMode")]
    public TextureWrapMode WrapMode;

    [TableColumnWidth(100, Resizable = false)]
    [OnValueChanged("ReImportTextureWithFilterMode")]
    public FilterMode FilterMode;

    [TableColumnWidth(100, Resizable = false)]
    [OnValueChanged("ReImportTextureWithTextureShape")]
    public TextureImporterShape ImportShape;

    [HideInTables]
    public int Width;
    [HideInTables]
    public int Height;
    [HideInTables]
    public int AndroidSize;
    [HideInTables]
    public int IosSize;
    [HideInTables]
    public bool IsPowerOfTwo = false;
    [HideInTables]
    public bool IsAndroidFormatError = false;
    [HideInTables]
    public bool IsIOSFormatError = false;

    private static Dictionary<string, TextureData> m_dictTexInfo = new Dictionary<string, TextureData>();
    private static int m_loadCount = 0;
    public static TextureData CreateTextureInfo(string assetPath)
    {
        if (!EditorPath.IsTexture(assetPath))
        {
            return null;
        }

        TextureData tInfo = null;
        if (!m_dictTexInfo.TryGetValue(assetPath, out tInfo))
        {
            tInfo = new TextureData();
            m_dictTexInfo.Add(assetPath, tInfo);
        }

        TextureImporter tImport = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
        if (tImport == null || texture == null)
            return null;
        tInfo.Obj = texture;
        tInfo.Path = tImport.assetPath;
        tInfo.ImportType = tImport.textureType;
        tInfo.ImportShape = tImport.textureShape;
        tInfo.ReadWriteEnable = tImport.isReadable;
        tInfo.MipmapEnable = tImport.mipmapEnabled;
        tInfo.WrapMode = tImport.wrapMode;
        tInfo.FilterMode = tImport.filterMode;
        TextureImporterPlatformSettings
            settingAndroid = tImport.GetPlatformTextureSettings(EditorConst.PlatformAndroid);
        tInfo.AndroidFormat = settingAndroid.format;
        TextureImporterPlatformSettings settingIos = tImport.GetPlatformTextureSettings(EditorConst.PlatformIos);
        tInfo.IosFormat = settingIos.format;
        tInfo.Width = texture.width;
        tInfo.Height = texture.height;


        tInfo.AndroidSize = EditorTool.CalculateTextureSizeBytes(texture, tInfo.AndroidFormat);
        tInfo.IosSize = EditorTool.CalculateTextureSizeBytes(texture, tInfo.IosFormat);
#if UNITY_ANDROID
        tInfo.AndroidSize = TextureUtillity.GetStorageMemorySize(texture);
#endif
#if UNITY_IOS
          tInfo.IosSize = TextureUtillity.GetStorageMemorySize(texture);
#endif
        tInfo.IsPowerOfTwo = TextureUtillity.IsPowerOfTwo(texture);
        tInfo.AndroidMemory = EditorUtility.FormatBytes(tInfo.AndroidSize);
        tInfo.IosMemory = EditorUtility.FormatBytes(tInfo.IosSize);
        //tInfo.MemSize = Mathf.Max(tInfo.AndroidSize, tInfo.IosSize);
        tInfo.ProcessIOSFormat(tInfo.IosFormat);
        tInfo.ProcessAndroidFormat(tInfo.AndroidFormat);
        tInfo.Size = "";
        bool is_error = false;
        if (tInfo.IsIOSFormatError)
        {
            is_error = true;
            tInfo.Size = "IOS:" + texture.width + "x" + texture.height;
        }
        if (tInfo.IsAndroidFormatError)
        {
            is_error = true;
            if (tInfo.IsIOSFormatError)
            {
                tInfo.Size += "\n";
            }
            tInfo.Size += "Android:" + texture.width + "x" + texture.height;
        }
        if (!is_error)
        {
            tInfo.Size = texture.width + "x" + texture.height;
        }

        if (Selection.activeObject != texture)
        {
            Resources.UnloadAsset(texture);
        }

        if (++m_loadCount % 256 == 0)
        {
            Resources.UnloadUnusedAssets();
        }

        return tInfo;
    }
    private void ProcessAndroidFormat(TextureImporterFormat format)
    {
        IsAndroidFormatError = false;
        switch (format)
        {
            case TextureImporterFormat.Automatic:
            case TextureImporterFormat.ETC2_RGBA8:
            case TextureImporterFormat.ETC2_RGB4:
            case TextureImporterFormat.ETC2_RGB4_PUNCHTHROUGH_ALPHA:
            case TextureImporterFormat.ETC_RGB4:
            case TextureImporterFormat.ETC_RGB4Crunched:
            case TextureImporterFormat.ETC2_RGBA8Crunched:
                if (Width % 4 != 0 || Height % 4 != 0)
                {
                    IsAndroidFormatError = true;
                }
                break;
        }
    }
    private void ProcessIOSFormat(TextureImporterFormat format)
    {
        IsIOSFormatError = false;
        switch (format)
        {
            case TextureImporterFormat.Automatic:
            case TextureImporterFormat.PVRTC_RGB2:
            case TextureImporterFormat.PVRTC_RGB4:
            case TextureImporterFormat.PVRTC_RGBA2:
            case TextureImporterFormat.PVRTC_RGBA4:
                if (Width != Height || Width % 4 != 0 || Height % 4 != 0)
                {
                    IsIOSFormatError = true;
                }
                break;
            case TextureImporterFormat.ASTC_4x4:
            case TextureImporterFormat.ASTC_5x5:
            case TextureImporterFormat.ASTC_6x6:
            case TextureImporterFormat.ASTC_8x8:
            case TextureImporterFormat.ASTC_10x10:
            case TextureImporterFormat.ASTC_12x12:
                break;
        }
    }
    public void RefreshSelf()
    {
        TextureImporter tImport = AssetImporter.GetAtPath(Path) as TextureImporter;
        Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(Path);
        if (tImport == null || texture == null)
            return;
        Obj = texture;
        Path = tImport.assetPath;
        ImportType = tImport.textureType;
        ImportShape = tImport.textureShape;
        ReadWriteEnable = tImport.isReadable;
        MipmapEnable = tImport.mipmapEnabled;
        WrapMode = tImport.wrapMode;
        FilterMode = tImport.filterMode;
        TextureImporterPlatformSettings
            settingAndroid = tImport.GetPlatformTextureSettings(EditorConst.PlatformAndroid);
        AndroidFormat = settingAndroid.format;
        TextureImporterPlatformSettings settingIos = tImport.GetPlatformTextureSettings(EditorConst.PlatformIos);
        IosFormat = settingIos.format;
        Width = texture.width;
        Height = texture.height;

        ProcessIOSFormat(IosFormat);
        ProcessAndroidFormat(AndroidFormat);
        Size = "";
        bool is_error = false;
        if (IsIOSFormatError)
        {
            is_error = true;
            Size = "IOS:" + texture.width + "x" + texture.height;
        }
        if (IsAndroidFormatError)
        {
            is_error = true;
            if (IsIOSFormatError)
            {
                Size += "\n";
            }
            Size += "Android:" + texture.width + "x" + texture.height;
        }
        if (!is_error)
        {
            Size = texture.width + "x" + texture.height;
        }

        //AndroidSize = TextureUtillity.GetStorageMemorySize(texture);
        //IosSize = TextureUtillity.GetStorageMemorySize(texture);
        AndroidSize = EditorTool.CalculateTextureSizeBytes(texture, AndroidFormat);
        IosSize = EditorTool.CalculateTextureSizeBytes(texture, IosFormat);

#if UNITY_ANDROID
        AndroidSize = TextureUtillity.GetStorageMemorySize(texture);
#endif
#if UNITY_IOS
        IosSize = TextureUtillity.GetStorageMemorySize(texture);
#endif
        IsPowerOfTwo = TextureUtillity.IsPowerOfTwo(texture);
        AndroidMemory = EditorUtility.FormatBytes(AndroidSize);
        IosMemory = EditorUtility.FormatBytes(IosSize);
    }

    public void RefreshByFileSetting()
    {
        //直接导入  因为导入的时候直接会设置
        AssetDatabase.ImportAsset(Path);
        RefreshSelf();
    }
    public static List<TextureData> GetTextureInfoByDirectory(string dir)
    {
        List<TextureData> texInfoList = new List<TextureData>();
        List<string> list = new List<string>();
        EditorPath.ScanDirectoryFile(dir, true, list);
        for (int i = 0; i < list.Count; ++i)
        {
            string assetPath = EditorPath.FormatAssetPath(list[i]);
            string name = System.IO.Path.GetFileName(assetPath);
            EditorUtility.DisplayProgressBar("获取贴图数据", name, (i * 1.0f) / list.Count);
            TextureData texInfo = CreateTextureInfo(assetPath);
            if (texInfo != null)
            {
                texInfoList.Add(texInfo);
            }
        }

        EditorUtility.ClearProgressBar();
        return texInfoList;
    }

    #region 导入刷新相关
    public void ReImportTextureWithTextureType()
    {
        ReImportTexture(TextureImportDataType.TextureImporterType);
    }
    public void ReImportTextureWithTextureShape()
    {
        ReImportTexture(TextureImportDataType.TextureImporterShape);
    }
    public void ReImportTextureWithReadWrite()
    {
        ReImportTexture(TextureImportDataType.ReadAnaWrite);
    }
    public void ReImportTextureWithMipMap()
    {
        ReImportTexture(TextureImportDataType.MimMap);
    }
    public void ReImportTextureWithWrapMode()
    {
        ReImportTexture(TextureImportDataType.WrapMode);
    }
    public void ReImportTextureWithFilterMode()
    {
        ReImportTexture(TextureImportDataType.FilterMode);
    }
    public void ReImportTextureWithAndroidFormat()
    {
        ReImportTexture(TextureImportDataType.AndroidFormat);
    }
    public void ReImportTextureWithIOSFormat()
    {
        ReImportTexture(TextureImportDataType.IOSFormat);
    }
    //修改参数后  重新导入设置
    public void ReImportTexture(TextureImportDataType modifyType)
    {
        bool is_modify_self = false;
        if (TextureModifyEditor.shaixuanData.IsCanMultiSelect)
        {
            if (!Is_select)
            {
                if (EditorUtility.DisplayDialog("您要修改的图片未被选中，请确认后修改", "您要修改的图片未被选中，请确认后修改",
                    "修改所有选中", "只修改自己"))
                {
                    //is_modify_self = true;
                }
                else
                {
                    is_modify_self = true;
                    //return;
                }
            }

            for (int i = 0; i < TextureModifyEditor.TableListWithIndexLabels.Count; i++)
            {
                TextureData textureData = TextureModifyEditor.TableListWithIndexLabels[i];
                if (textureData.Is_select)
                {
                    TextureImporter tImport = AssetImporter.GetAtPath(textureData.Path) as TextureImporter;
                    switch (modifyType)
                    {
                        case TextureImportDataType.TextureImporterType:
                            tImport.textureType = ImportType;
                            break; ;
                        case TextureImportDataType.TextureImporterShape:
                            tImport.textureShape = ImportShape;
                            break;
                        case TextureImportDataType.ReadAnaWrite:
                            tImport.textureShape = ImportShape;
                            break;
                        case TextureImportDataType.MimMap:
                            tImport.mipmapEnabled = MipmapEnable;
                            break;
                        case TextureImportDataType.WrapMode:
                            tImport.wrapMode = WrapMode;
                            break;
                        case TextureImportDataType.FilterMode:
                            tImport.filterMode = FilterMode;
                            break;
                        case TextureImportDataType.AndroidFormat:
                            TextureImporterPlatformSettings
                                settingAndroid = tImport.GetPlatformTextureSettings(EditorConst.PlatformAndroid);
                            if (AndroidFormat == TextureImporterFormat.AutomaticCompressed)
                            {
                                settingAndroid.overridden = false;
                            }
                            else
                            {
                                settingAndroid.overridden = true;
                            }
                            settingAndroid.format = AndroidFormat;
                            tImport.SetPlatformTextureSettings(settingAndroid);
                            break;
                        case TextureImportDataType.IOSFormat:
                            TextureImporterPlatformSettings
                                settingIos = tImport.GetPlatformTextureSettings(EditorConst.PlatformIos);
                            if (IosFormat == TextureImporterFormat.AutomaticCompressed)
                            {
                                settingIos.overridden = false;
                            }
                            else
                            {
                                settingIos.overridden = true;
                            }
                            settingIos.format = IosFormat;
                            tImport.SetPlatformTextureSettings(settingIos);
                            break;
                    }
                    AssetDatabase.ImportAsset(textureData.Path);
                    textureData.RefreshSelf();
                }
            }
        }
        else
        {
            is_modify_self = true;
        }
        if (is_modify_self)
        {
            TextureImporter tImport = AssetImporter.GetAtPath(Path) as TextureImporter;
            tImport.textureType = ImportType;
            tImport.textureShape = ImportShape;
            tImport.isReadable = ReadWriteEnable;
            tImport.mipmapEnabled = MipmapEnable;
            tImport.wrapMode = WrapMode;
            tImport.filterMode = FilterMode;


            TextureImporterPlatformSettings
                settingAndroid = tImport.GetPlatformTextureSettings(EditorConst.PlatformAndroid);
            if (AndroidFormat == TextureImporterFormat.AutomaticCompressed)
            {
                settingAndroid.overridden = false;
            }
            else
            {
                settingAndroid.overridden = true;
            }
            settingAndroid.format = AndroidFormat;
            tImport.SetPlatformTextureSettings(settingAndroid);

            TextureImporterPlatformSettings
                settingIos = tImport.GetPlatformTextureSettings(EditorConst.PlatformIos);
            if (IosFormat == TextureImporterFormat.AutomaticCompressed)
            {
                settingIos.overridden = false;
            }
            else
            {
                settingIos.overridden = true;
            }
            settingIos.format = IosFormat;
            tImport.SetPlatformTextureSettings(settingIos);

            AssetDatabase.ImportAsset(Path);
            RefreshSelf();
        }

    }


    #endregion


    private bool Is_CanMultSelect()
    {
        return !TextureModifyEditor.shaixuanData.IsCanMultiSelect;
    }
    #region Check

    private Color CheckReadWriteEnableColor()
    {
        return ReadWriteEnable == false ? Color.white : Color.red;
    }

    private string SizeDraw(string text, GUIContent label)
    {
        bool is_error = false;
        if (IsIOSFormatError)
        {
            is_error = true;
            //string android_error_s = "Android:" + text;
            return SirenixEditorFields.TextField(null, text, TextureModifyEditor.Green_label_style);
        }
        else if (IsAndroidFormatError)
        {
            is_error = true;
            //string ios_error_s = "IOS:" + text;
            return SirenixEditorFields.TextField(null, text, TextureModifyEditor.Error_label_style);
        }
        else
        {
            return SirenixEditorFields.TextField(null, text, EditorStyles.label);
        }

    }

    private Color MipmapColor()
    {
        return MipmapEnable == false ? Color.white : Color.red;
    }

    private void ClickTexture()
    {
        Selection.SetActiveObjectWithContext(Obj, null);
    }
    #endregion
}