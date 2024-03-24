/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/11/20 22:36:17
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    public partial class GlobalSetting
    {
        [SerializeField]
        private ArchiveService.Setting _archive = null;

        public static ArchiveService.Setting archive => Inst._archive;
    }

    /// <summary>
    /// ArchiveService
    /// </summary>
    public class ArchiveService : IServiceInitialize, IDisposable
    {
        [Serializable]
        public class Setting
        {
            public int firstLevelID = 1;
        }

        public static class SettingName
        {
            public const string LastLevelID = "LastLevelID";
        }

        public Setting setting => GlobalSetting.archive;

        //

        public int lastLevelID { get => PlayerPrefs.GetInt(SettingName.LastLevelID, setting.firstLevelID); set => PlayerPrefs.SetInt(SettingName.LastLevelID, value); }

        //

        public IEnumerator OnServiceInitialize()
        {
            CheckData();

            yield break;
        }

        private void CheckData()
        {
            if (!PlayerPrefs.HasKey(SettingName.LastLevelID))
            {
                PlayerPrefs.SetInt(SettingName.LastLevelID, setting.firstLevelID);
            }
        }

        public void Dispose()
        {
            PlayerPrefs.Save();
        }
    }
}