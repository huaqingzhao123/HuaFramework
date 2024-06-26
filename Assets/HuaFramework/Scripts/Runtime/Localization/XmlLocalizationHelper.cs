﻿using System;
using System.Xml;
using UnityEngine;

namespace HuaFramework.Localization
{
    /// <summary>
    /// XML 格式的本地化辅助器。
    /// </summary>
    public class XmlLocalizationHelper 
    {
        private LocalizationManager m_LocalizationManager;
        /// <summary>
        /// 解析字典。
        /// </summary>
        /// <param name="text">要解析的字典文本。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否解析字典成功。</returns>
        public  bool ParseDictionary(string text, object userData)
        {
            if (m_LocalizationManager == null)
                m_LocalizationManager = LocalizationManager.Inst;
            try
            {
                string currentLanguage = m_LocalizationManager.LoaclLanguagueStr;

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(text);
                XmlNode xmlRoot = xmlDocument.SelectSingleNode("Dictionaries");
                XmlNodeList xmlNodeDictionaryList = xmlRoot.ChildNodes;
                for (int i = 0; i < xmlNodeDictionaryList.Count; i++)
                {
                    XmlNode xmlNodeDictionary = xmlNodeDictionaryList.Item(i);
                    if (xmlNodeDictionary.Name != "Dictionary")
                    {
                        continue;
                    }

                    string language = xmlNodeDictionary.Attributes.GetNamedItem("Language").Value;
                    if (language != currentLanguage)
                    {
                        continue;
                    }

                    XmlNodeList xmlNodeStringList = xmlNodeDictionary.ChildNodes;
                    for (int j = 0; j < xmlNodeStringList.Count; j++)
                    {
                        XmlNode xmlNodeString = xmlNodeStringList.Item(j);
                        if (xmlNodeString.Name != "String")
                        {
                            continue;
                        }

                        string key = xmlNodeString.Attributes.GetNamedItem("Key").Value;
                        string value = xmlNodeString.Attributes.GetNamedItem("Value").Value;
                        if (!AddRawString(key, value))
                        {
                            Debug.LogErrorFormat(string.Format("Can not add raw string with key '{0}' which may be invalid or duplicate.", key));
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogErrorFormat(string.Format("Can not parse dictionary '{0}' with exception '{1}'.", text, string.Format("{0}\n{1}", exception.Message, exception.StackTrace)));
                return false;
            }
        }
        /// <summary>
        /// 增加字典。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <param name="value">字典内容。</param>
        /// <returns>是否增加字典成功。</returns>
        protected bool AddRawString(string key, string value)
        {
            return m_LocalizationManager.AddRawString(key, value);
        }
    }
}
