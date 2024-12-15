#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
namespace HuaFramework.Configs
{
    public enum ConfigType
    {
        Equip = 0,
        Skill,
    }

    [Serializable]
    [CreateAssetMenu]
    public class BaseConfig : ScriptableObject
    {
        public const string ConfigPathRoot = "Assets/HuaFramework/Framework/Configs/ConfigsAssets/";
        [LabelText("名字"), BoxGroup("基础信息")]
        public string Name;
        [LabelText("Id"), BoxGroup("基础信息"), ReadOnly]
        public int Id = 1;
        [LabelText("配置类型"), BoxGroup("基础信息")]
        public ConfigType ConfigType;
        [LabelText("描述"), TextArea(5, 5), BoxGroup("基础信息")]
        public string Describe;
#if UNITY_EDITOR
        /// <summary>
        /// 保存设置
        /// </summary>
        [Button("保存设置")]
        public virtual void SaveConfig()
        {
            var oldPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            var directoryPath = ConfigPathRoot + "/" + ConfigType.ToString() + "s/";
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
            var directoryInfo = new DirectoryInfo(directoryPath);
            var newPath = directoryPath + ConfigType.ToString() + "_" + Id + ".asset";
            if (Id == 0)
            {
                var files = directoryInfo.GetFiles();
                var allConfigs = new List<BaseConfig>();
                var allIndex = 1;
                for (int i = 0; i < files.Length; i++)
                {
                    var item = files[i];
                    if (item.Name.EndsWith("meta")) continue;
                    var assetPath = item.FullName.Substring(item.FullName.IndexOf("Assets"));
                    var oldAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<BaseConfig>(assetPath);
                    //Debug.LogErrorFormat("资源路径:{0}", assetPath);
                    //不连续的话新建这个位置的
                    allConfigs.Add(oldAsset);
                    Debug.LogErrorFormat("此配置的id:{0},排到的id:{1},此配置名字:{2}", oldAsset.Id, allIndex, item.Name);
                }
                allConfigs.Sort((a,b)=>{

                    if (a.Id > b.Id)
                        return 1;
                    else if (a.Id < b.Id)
                        return -1;
                    else
                        return 1;
                });
                foreach (var oldAsset in allConfigs)
                {
                    if (oldAsset.Id > allIndex)
                        break;
                    allIndex++;
                }
                Id = allIndex;
                if (string.IsNullOrEmpty(Name)) Name = ConfigType.ToString();
                //保存后的名字，统一格式为Skill_0...
                newPath = directoryPath + ConfigType.ToString() + "_" + Id + ".asset";
            }
            //此路径存在
            if (File.Exists(newPath))
            {
                UnityEditor.AssetDatabase.SaveAssets();
                return;
            }
            UnityEditor.AssetDatabase.SaveAssets();
            FileInfo fileInfo = new FileInfo(oldPath);
            fileInfo.MoveTo(newPath);
            UnityEditor.AssetDatabase.Refresh();
        }
#endif
    }

}
#endif