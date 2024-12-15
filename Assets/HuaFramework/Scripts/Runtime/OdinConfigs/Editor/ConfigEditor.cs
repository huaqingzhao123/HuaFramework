#if  UNITY_EDITOR &&ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;
using HuaFramework.Configs;

public class ConfigEditor : OdinMenuEditorWindow
{

    [MenuItem("HuaFramework/配置编辑器")]
    private static void Open()
    {
        var window = GetWindow<ConfigEditor>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1200, 600);

        window.titleContent = new GUIContent("配置编辑器");
        window.DrawUnityEditorPreview = true;
    }
    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree(false);

        tree.Config.DrawSearchToolbar = true;

        ConfigOverview.Instance.UpdateConfigOverview();

        foreach (var config in ConfigOverview.Instance.AllConfigs)
        {
            var showName = config.Name;
            if (!string.IsNullOrEmpty(showName))
            {
                tree.Add(string.Format("配置/{0}_{1}", config.Id,showName), config);
            }
            else
            {
                tree.Add(string.Format("配置/空配置_{0}", config.Id), config);
            }
        }
        return tree;
    }


    protected override void OnBeginDrawEditors()
    {
        var selected = this.MenuTree.Selection.FirstOrDefault();
        var toolbarHeight = this.MenuTree.Config.SearchToolbarHeight;

        // Draws a toolbar with the name of the currently selected menu item.
        SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
        {
            if (selected != null)
            {
                GUILayout.Label(selected.Name);
            }

            if (SirenixEditorGUI.ToolbarButton(new GUIContent("创建新配置")))
            {
                ConfigCreator.ShowDialog<BaseConfig>(config =>
                {
                    config.Id = 0;
                    base.TrySelectMenuItemWithObject(config);
                });
            }

            if (SirenixEditorGUI.ToolbarButton(new GUIContent("刷新")))
            {
                this.ForceMenuTreeRebuild();
            }
        }

        SirenixEditorGUI.EndHorizontalToolbar();
    }
}

[GlobalConfig("Code/GameConfig/InitConfig", UseAsset = true)]
public class ConfigOverview : GlobalConfig<ConfigOverview>
{
    [ReadOnly]
    public List<BaseConfig> AllConfigs = new List<BaseConfig>();
    public void UpdateConfigOverview()
    {
        AllConfigs.Clear();
        int rootCount = 0;
        DirectoryInfo root = new DirectoryInfo(BaseConfig.ConfigPathRoot);
        if (root.Exists)
        {
            foreach (var directoryInfo in root.GetDirectories())
            {
                foreach (FileInfo fInfo in directoryInfo.GetFiles())
                {
                    if (fInfo.Name.EndsWith("meta"))
                    {
                        continue;
                    }
                    var asset = AssetDatabase.LoadAssetAtPath<BaseConfig>( BaseConfig.ConfigPathRoot+directoryInfo.Name+"/"+fInfo.Name);
                    AllConfigs.Add(asset);
                }
            }
        }
    }
    //public void UpdateConfigOverview()
    //{
    //    AllConfigs.Clear();
    //    int rootCount = 0;
    //    DirectoryInfo root = new DirectoryInfo(Application.dataPath + "/Resource/Configs");
    //    if (root.Exists)
    //    {
    //        foreach (DirectoryInfo dInfo in root.GetDirectories())
    //        {
    //            if (dInfo.Name == "Buffs" || dInfo.Name == "SimpleAI" || dInfo.Name == "CameraShake" || dInfo.Name == "Names") continue;

    //            if (dInfo.GetFiles().Length > 0) rootCount++;

    //            foreach (FileInfo fInfo in dInfo.GetFiles())
    //            {
    //                if (fInfo.Name.EndsWith("meta"))
    //                {
    //                    continue;
    //                }

    //                var instanceType = BDLauncher.GetConfigTypeFromItemString(dInfo.Name);

    //                string res = File.ReadAllText(fInfo.FullName);
    //                AllConfigs.Add((BaseConfig)JsonUtility.ToObject(instanceType, res));
    //            }
    //        }
    //    }

    //    AllConfigs.Sort((first, second) =>
    //    {
    //        if (first.id < second.id)
    //            return -1;
    //        else
    //            return 1;
    //    });

    //    //lin: 生成配置汇总文件（包含所有英雄，怪物，等等的id信息）
    //    var allCfg = new GameConfigTotal();

    //    root = new DirectoryInfo(Application.dataPath + "/Resource/Configs/");
    //    allCfg.allConfigs = new OneConfigTatal[rootCount];

    //    var idx = 0;
    //    foreach (var dir in root.GetDirectories())
    //    {
    //        if (dir.Name == "Buffs" || dir.Name == "SimpleAI" || dir.Name == "CameraShake" || dir.Name == "Names") continue;

    //        var ids = GameConfigTotal.GetIdsInDir(dir.Name);
    //        if (ids != null && ids.Length > 0)
    //        {
    //            allCfg.allConfigs[idx++] = new OneConfigTatal
    //            {
    //                dirName = dir.Name,
    //                itemIDs = ids,
    //            };
    //        }
    //    }

    //    var allcfgStr = JsonUtility.ToJson(allCfg);
    //    File.WriteAllText(Application.dataPath + "/Resource/Configs/allcfg.json", allcfgStr);

    //}
}

public class ConfigCreator
{
    public static T ShowDialog<T>(Action<T> onComplete) where T : BaseConfig
    {
        var id = (Directory.GetFiles(BaseConfig.ConfigPathRoot+ "BaseConfigs/").Length + 1);
        var path = BaseConfig.ConfigPathRoot + "BaseConfigs/" + id + ".asset";
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance(typeof(T)), path);
        var instance = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) as T;
        onComplete?.Invoke(instance);
        return instance;
    }
}

#endif
