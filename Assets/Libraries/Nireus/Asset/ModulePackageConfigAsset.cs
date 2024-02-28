using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Nireus
{
    public enum ModuleNameIndex
    {
        BossBattle = 1,        //上古异兽
        MenPaiBuild = 2,       //建造分包
        KungFuComb = 3,        //武功战斗动画分包 问题较多暂时不分包了
        FuBen_1 = 4,           //副本1分包
        Kick = 5,              //踢馆
    }
    [Serializable]
    public class ModulePackagePathList
    {
        [FolderPath]
        public List<string> FloderPathList = new List<string>();

    }

    [CreateAssetMenu]
    public class ModulePackageConfigAsset : SerializedScriptableObject
    {
        [ShowInInspector]
        [DictionaryDrawerSettings(KeyLabel = "模块名", ValueLabel = "模块包含文件夹")]
        [Title("分模块加载AssetBundle配置")]
        public Dictionary<ModuleNameIndex, ModulePackagePathList> ModulePackageListDictionary;

        [ShowInInspector]
        [DictionaryDrawerSettings(KeyLabel = "模块名", ValueLabel = "AssetBundle信息", IsReadOnly = true, DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        [Title("模块AssetBundle详细信息")]
        [NonSerialized, OdinSerialize]
        public Dictionary<ModuleNameIndex, List<string>> ModulePackageListDictionary_AB;

#if UNITY_EDITOR
        public static ModulePackageConfigAsset _Instance;
        public static ModulePackageConfigAsset Instance
        {
            get
            {
                if (_Instance == null)
                {
#if USE_SUB_PACKAGE
                    _Instance = UnityEditor.AssetDatabase.LoadAssetAtPath<ModulePackageConfigAsset>(AssetModuleLoadManager.CONFIG_ASSET_PATH);
#endif
                }

                if (_Instance == null)
                {
                    GameDebug.LogError("Module config file can not find!");
                }
                return _Instance;
            }
            set => _Instance = value;
        }

        public static Boolean IsNotConfigModule()
        {
            if (Instance.ModulePackageListDictionary_AB.Count == 0)
            {
                return true;
            }

            return false;
        }
        public void OnValidate()
        {
            GameDebug.Log("ProcessSelfAttributes UpdateView ");
            ModulePackageListDictionary_AB = new Dictionary<ModuleNameIndex, List<string>>();
            foreach (var VARIABLE in ModulePackageListDictionary)
            {
                List<string> abs_path = new List<string>();
                foreach (var path in VARIABLE.Value.FloderPathList)
                {
                    abs_path = abs_path.Union(AssetBundleFilePath.GetPackName_AB(path)).ToList<string>();
                }
                ModulePackageListDictionary_AB.Add(VARIABLE.Key, abs_path);
            }
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();
        }
#endif
    }
}