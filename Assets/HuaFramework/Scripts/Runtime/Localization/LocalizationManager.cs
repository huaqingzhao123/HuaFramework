using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace HuaFramework.Localization
{
    /// <summary>
    /// 本地化管理器。
    /// </summary>
    public sealed class LocalizationManager 
    {
        #region 单例部分（之所以不基础ManagerBase是因为ManagerBase在热更dll里面）
        static private LocalizationManager i;

        static public LocalizationManager Inst
        {
            get
            {
                if (i == null)
                {
                    i = new LocalizationManager();
                }
                return i;
            }
        }
        #endregion

        private readonly Dictionary<string, string> m_Dictionary;
        private XmlLocalizationHelper m_LocalizationHelper = new XmlLocalizationHelper();
        private string ConfigPath = Application.streamingAssetsPath + "/FrameWork/Localization/LocalizationConfig.xml";
        public string LoaclLanguagueStr = "zh_cn";
        public string LoacalizationText {

            get;private set;
        }
        /// <summary>
        /// 初始化本地化管理器的新实例。
        /// </summary>
        private LocalizationManager()
        {
            m_Dictionary = new Dictionary<string, string>();
            //加载dictionary
            LoacalizationText=File.ReadAllText(ConfigPath);
        }

        /// <summary>
        /// 获取字典数量。
        /// </summary>
        public int DictionaryCount
        {
            get
            {
                return m_Dictionary.Count;
            }
        }      

        /// <summary>
        /// 解析字典。
        /// </summary>
        /// <param name="text">要解析的字典文本。</param>
        /// <returns>是否解析字典成功。</returns>
        public bool ParseDictionary(string text)
        {
            return ParseDictionary(text, null);
        }

        /// <summary>
        /// 解析字典。
        /// </summary>
        /// <param name="text">要解析的字典文本。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否解析字典成功。</returns>
        public bool ParseDictionary(string text, object userData)
        {      
            return m_LocalizationHelper.ParseDictionary(text, userData);
        }


        /// <summary>
        /// 根据字典主键获取字典内容字符串。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <param name="args">字典参数。</param>
        /// <returns>要获取的字典内容字符串。</returns>
        public string GetString(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("Key is invalid.");
            }

            string value = null;
            if (!m_Dictionary.TryGetValue(key, out value))
            {
                return string.Format("<NoKey>{0}", key);
            }

            try
            {
                return string.Format(value, args);
            }
            catch (Exception exception)
            {
                string errorString = string.Format("<Error>{0},{1}", key, value);
                if (args != null)
                {
                    foreach (object arg in args)
                    {
                        errorString += "," + arg.ToString();
                    }
                }

                errorString += "," + exception.Message;
                return errorString;
            }
        }

        /// <summary>
        /// 是否存在字典。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <returns>是否存在字典。</returns>
        public bool HasRawString(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("Key is invalid.");
            }
            return m_Dictionary.ContainsKey(key);
        }

        /// <summary>
        /// 根据字典主键获取字典值。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <returns>字典值。</returns>
        public string GetRawString(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("Key is invalid.");
            }
            if (m_Dictionary.TryGetValue(key, out string value))
            {
                return value;
            }

            return key;
        }

        int time = 30;
        /// <summary>
        /// 增加字典。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <param name="value">字典内容。</param>
        /// <returns>是否增加字典成功。</returns>
        public bool AddRawString(string key, string value)
        {
            if (HasRawString(key))
            {
                return false;
            }
            time--;
            if (time <= 0)
            {
                time = 30;
                //WingjoyFramework.Log.Debug("key的值：{0}，value的值:{1}", key,value);
            }
            m_Dictionary.Add(key, value ?? string.Empty);
            return true;
        }

        /// <summary>
        /// 移除字典。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <returns>是否移除字典成功。</returns>
        public bool RemoveRawString(string key)
        {
            if (!HasRawString(key))
            {
                return false;
            }

            return m_Dictionary.Remove(key);
        }
    }
}
