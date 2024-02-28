using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[Serializable]
public class TextureModifyByFileData
{
    [HorizontalGroup("IsFirstImport")]
    [LabelText("是否只第一次导入时按设置导入")]
    public bool IsFirstImport = false;

    public string Path;
    [HorizontalGroup("ImportType")]
    public TextureImporterType ImportType;
    [HorizontalGroup("ImportType")]
    [LabelText("是否强制修改")]
    public bool IsOverWriteImportType = false;
    
    [HorizontalGroup("AndroidFormat")]
    public TextureImporterFormat AndroidFormat;
    [HorizontalGroup("AndroidFormat")]
    [LabelText("是否强制修改")]
    public bool IsOverWriteAndroidFormat = false;
    
    [HorizontalGroup("IosFormat")]
    public TextureImporterFormat IosFormat;
    [HorizontalGroup("IosFormat")]
    [LabelText("是否强制修改")]
    public bool IsOverWriteIosFormat = false;
    
    [HorizontalGroup("ReadWriteEnable")]
    public bool ReadWriteEnable = false;
    [HorizontalGroup("ReadWriteEnable")]
    [LabelText("是否强制修改")]
    public bool IsOverWriteReadWriteEnable = false;
    
    [HorizontalGroup("MipmapEnable")]
    public bool MipmapEnable = false;
    [HorizontalGroup("MipmapEnable")]
    [LabelText("是否强制修改")]
    public bool IsOverWriteMipmapEnable = false;
    
    [HorizontalGroup("WrapMode")]
    public TextureWrapMode WrapMode;
    [HorizontalGroup("WrapMode")]
    [LabelText("是否强制修改")]
    public bool IsOverWriteWrapMode = false;
    
    [HorizontalGroup("FilterMode")]
    public FilterMode FilterMode;
    [HorizontalGroup("FilterMode")]
    [LabelText("是否强制修改")]
    public bool IsOverWriteFilterMode = false;
    
    [HorizontalGroup("ImportShape")]
    public TextureImporterShape ImportShape;
    [HorizontalGroup("ImportShape")]
    [LabelText("是否强制修改")]
    public bool IsOverWriteImportShape = false;

    [HorizontalGroup("Width")]
    public int Width;
    [HorizontalGroup("Width")]
    [LabelText("是否强制修改")]
    public bool IsOverWriteWidth = false;

    [HorizontalGroup("Height")]
    public int Height;
    [HorizontalGroup("Height")]
    [LabelText("是否强制修改")]
    public bool IsOverWriteHeight = false;
}
