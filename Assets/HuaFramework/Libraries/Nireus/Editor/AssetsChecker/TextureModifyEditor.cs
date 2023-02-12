using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Nireus;
using ObjectFieldAlignment = Sirenix.OdinInspector.ObjectFieldAlignment;

public class TextureModifyEditor : OdinEditorWindow
{
    
    private const string texture_modify_by_file_config_path = "Assets/Scripts/Nireus/Editor/AssetsChecker/TextureModifyByFileConfig.asset";
    private const string texture_modidy_data_path = "Assets/Scripts/Nireus/Editor/AssetsChecker/TextureModifyData.asset";
    private static TextureModifyByFileAssets textureModifyByFileAssets;
    private static TextureModifyDataAssets textureModifyData;
    private static bool isShow = false;
     public static TextureModifyDataAssets TextureModifyData
     {
         get
         {
             if (textureModifyData == null)
             {
                 textureModifyData = AssetDatabase.LoadAssetAtPath<TextureModifyDataAssets>(texture_modidy_data_path);
                if(textureModifyData == null)
                {
                    textureModifyData = ScriptableObject.CreateInstance<TextureModifyDataAssets>();
                    AssetDatabase.CreateAsset(textureModifyData, texture_modidy_data_path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
             }
             return textureModifyData;   
         }
     }
    public static TextureModifyByFileAssets TextureModifyByFileAssets
    {
        get
        {
            if (textureModifyByFileAssets == null)
            {
                textureModifyByFileAssets = AssetDatabase.LoadAssetAtPath<TextureModifyByFileAssets>(texture_modify_by_file_config_path);
                if (textureModifyByFileAssets == null)
                {
                    textureModifyByFileAssets = ScriptableObject.CreateInstance<TextureModifyByFileAssets>();
                    AssetDatabase.CreateAsset(textureModifyByFileAssets, texture_modify_by_file_config_path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            return textureModifyByFileAssets;
        }
    }
    private void OnDestroy()
    {
        isShow = false;
    }
    public static TextureModifyEditor window;
    [MenuItem("Tools/Texture Check Window")]
    private static void OpenWindow()
    {
        window = GetWindow<TextureModifyEditor>();
        
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 700);
        if (_error_label_style == null)
        {
            _error_label_style = new GUIStyle("error_label_style");
            _error_label_style.normal.textColor = Color.red;
            _green_label_style = new GUIStyle("green_label_style");
            _green_label_style.normal.textColor = Color.magenta;
        }
        isShow = true;
        if(TextureModifyByFileAssets!=null)
        {

        }
        //textureModifyByFileAssets = AssetDatabase.LoadAssetAtPath<TextureModifyByFileAssets>(texture_modify_by_file_config_path);
        if (TextureModifyData != null)
        {
        }
    }
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void RefreshEditorWindow()
    {
        if(!isShow)
        {
            return;
        }
        Debug.Log("RefreshEditorWindow");
        window = GetWindow<TextureModifyEditor>();
        window.m_texInfoList = TextureModifyData.DataList;
        TableListWithIndexLabels.Clear();

        for (int i = 0; i < window.m_texInfoList.Count; i++)
        {
            TableListWithIndexLabels.Add(window.m_texInfoList[i]);
        }
        window.fileTextureData = new FileTextureData();
        for (int k = 0; k < TableListWithIndexLabels.Count; k++)
        {
            window.fileTextureData.AddData(TableListWithIndexLabels[k]);
        }
    }

    private static GUIStyle _error_label_style;
    private static GUIStyle _green_label_style;

    public static GUIStyle Green_label_style
    {
        get
        {
            if (_green_label_style == null)
            {
                _green_label_style = new GUIStyle("green_label_style");
                _green_label_style.normal.textColor = Color.magenta;
            }
            return _green_label_style;
        }
    }
    public static GUIStyle Error_label_style
    {
        get
        {
            if (_error_label_style == null)
            {
                _error_label_style = new GUIStyle("error_label_style");
                _error_label_style.normal.textColor = Color.red;
            }
            return _error_label_style;
        }
    }

    protected List<TextureData> m_texInfoList = new List<TextureData>();

    [ToggleLeft]
    [LabelText("是否按文件夹操作")]
    [HorizontalGroup("Tittle",LabelWidth = 30)]
    public bool IsFileShow = false;

    [HideIf("IsFileShow")]
    [LabelText("数据操纵选项")]
    [ShowInInspector]
    public static ShaiXuanData shaixuanData = new ShaiXuanData();
    
    [TableList(ShowIndexLabels = true,IsReadOnly = true)]
    [HideIf("IsFileShow")]
    [ShowInInspector]
    public static List<TextureData> TableListWithOrder = new List<TextureData>();
    
    [TableList(ShowIndexLabels = true,IsReadOnly = true)]
    [HideIf("IsFileShow")]
    [ShowInInspector]
    public static List<TextureData> TableListWithIndexLabels = new List<TextureData>();
    
    [ShowIf("IsFileShow")]
    public FileTextureData fileTextureData;


    [TableList(ShowIndexLabels = true, IsReadOnly = true)]
    [ShowIf("IsFileShow")]
    [ShowInInspector]
    public static List<TextureData> TableListErrorWithFile = new List<TextureData>();

    [Button("刷新")]
    private void Refresh()
    {
        m_texInfoList = TextureData.GetTextureInfoByDirectory("Assets/");
        TextureModifyData.DataList.Clear();
        TableListWithIndexLabels.Clear();

        for (int i = 0; i < m_texInfoList.Count; i++)
        {
            TableListWithIndexLabels.Add(m_texInfoList[i]);
        }
        fileTextureData = new FileTextureData();
        for(int k=0;k<TableListWithIndexLabels.Count;k++)
        {
            fileTextureData.AddData(TableListWithIndexLabels[k]);
        }
        TextureModifyData.DataList = m_texInfoList;
    }
    [ShowIf("IsFileShow")]
    [Button("找出不符合规定的图片")]
    private void ShaiXuanError()
    {
        if(fileTextureData!=null)
        {
            TableListErrorWithFile.Clear();
            CheckErrorByFile(fileTextureData);
        }
    }
    private void CheckErrorByFile(FileTextureData fileTextureData)
    {
        
        if (fileTextureData != null)
        {
            string t = $"{PathConst.BUNDLE_RES}Textures/SKIN_ROLE/pvp";
            if (fileTextureData.Path.Contains(t))
            {
                Debug.Log("aaa");
            }
            if (fileTextureData.textureModifyByFileData != null)
            {
                List<TextureData> textureData = fileTextureData.textureData;
                for (int i = 0; i < textureData.Count; i++)
                {
                    TextureData data = textureData[i];
                    if(fileTextureData.textureModifyByFileData.IsOverWriteHeight)
                    {
                        if(data.Height != fileTextureData.textureModifyByFileData.Height)
                        {
                            TableListErrorWithFile.Add(data);
                        }
                    }
                    if (fileTextureData.textureModifyByFileData.IsOverWriteWidth)
                    {
                        if (data.Width != fileTextureData.textureModifyByFileData.Width)
                        {
                            TableListErrorWithFile.Add(data);
                        }
                    }
                }
            }
           
            for(int j=0;j< fileTextureData.FileTextureList.Count;j++)
            {
                CheckErrorByFile(fileTextureData.FileTextureList[j]);
            }
        }
    }
}