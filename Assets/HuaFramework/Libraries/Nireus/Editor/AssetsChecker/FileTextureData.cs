using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
//按照文件夹设置图片格式数据
[Serializable]
public class FileTextureData
{
    [HideInInspector]
    public string Path = "Assets";

    [HideIf("IsShowTextureModifyByFileData")]
    [LabelText("导入自动设置格式")]
    [HorizontalGroup("TextureModify")]
    public TextureModifyByFileData textureModifyByFileData = null;

    [Button(ButtonSizes.Small)]
    [HorizontalGroup("TextureModify")]
    [HideIf("IsShowTextureModifyByFileData")]
    public void DeleteConfig()
    {
        TextureModifyEditor.TextureModifyByFileAssets.RemoveData(Path);
        textureModifyByFileData = null;
    }

    [PropertyOrder(20)]
    [HideIf("IsShowTextureModifyByFileData")]
    [Button(ButtonSizes.Large)]
    [GUIColor(0, 1, 0)]
    private void RefreshNow()
    {
        for (int i = 0; i < textureData.Count; i++)
        {
            textureData[i].RefreshByFileSetting();
        }

        for (int j = 0; j < FileTextureList.Count; j++)
        {
            if (!FileTextureList[j].IsShowTextureModifyByFileData())
            {
                continue;
            }
            FileTextureList[j].RefreshNow();
        }
    }

    [PropertyOrder(100)]
    [HideIf("IsShowTextureData")]
    [ListDrawerSettings(HideAddButton = true, DraggableItems = false, HideRemoveButton = true)]
    [TableList(ShowIndexLabels = true, IsReadOnly = true)]
    public List<TextureData> textureData = new List<TextureData>();

    [ShowInInspector]
    [PropertyOrder(200)]
    [HideIf("IsShowFileTextureList")]
    [LabelText("$Path")]
    [ListDrawerSettings(HideAddButton = true, DraggableItems = false, HideRemoveButton = true, OnTitleBarGUI = "AddTextrureModifyData")]
    //[TableList(ShowIndexLabels = true,IsReadOnly = true)]
    public List<FileTextureData> FileTextureList = new List<FileTextureData>();


    public bool IsShowTextureData()
    {
        return textureData.Count == 0;
    }
    public bool IsShowFileTextureList()
    {
        return FileTextureList.Count == 0;
    }

    public bool IsShowTextureModifyByFileData()
    {
        return ((textureModifyByFileData == null || textureModifyByFileData.Path == null || textureModifyByFileData.Path == ""));
    }
    public void AddTextrureModifyData()
    {
        if (GUILayout.Button("Add"))
        {
            TextureModifyByFileData data = new TextureModifyByFileData();
            data.Path = Path;
            TextureModifyEditor.TextureModifyByFileAssets.AddData(data);
            textureModifyByFileData = data;
        }
    }
    public void AddData(TextureData data)
    {
        string file_path = data.Path.Replace(Path, "");
        int findIndex = file_path.IndexOf('/');
        if (findIndex == 0)
        {
            file_path = file_path.Substring(1, file_path.Length - 1);
        }
        string[] file_split = file_path.Split('/');
        if (file_split.Length != 0)
        {
            if (file_split.Length == 1)
            {
                //已经到了资源名称的位置
                string tempS = Path;//+"/"+ file_split[0];
                FileTextureData fileDataTemp = FileTextureList.Find(o => o.Path == tempS);
                if (fileDataTemp != null)
                {
                    fileDataTemp.textureData.Add(data);
                }
                else
                {
                    FileTextureData DataTemp = new FileTextureData();
                    FileTextureList.Add(DataTemp);
                    DataTemp.Path = tempS;
                    DataTemp.textureModifyByFileData = TextureModifyEditor.TextureModifyByFileAssets.GetData(tempS);
                    DataTemp.textureData.Add(data);
                }
                //textureData.Add(data);
            }
            else
            {
                string tempS = Path + "/" + file_split[0];
                FileTextureData fileDataTemp = FileTextureList.Find(o => o.Path == tempS);
                if (fileDataTemp != null)
                {
                    fileDataTemp.AddData(data);
                }
                else
                {
                    FileTextureData DataTemp = new FileTextureData();
                    FileTextureList.Add(DataTemp);
                    DataTemp.Path = tempS;

                    DataTemp.AddData(data);
                }
            }
        }
        else
        {
            Debug.Log("TODO");
        }

    }

}