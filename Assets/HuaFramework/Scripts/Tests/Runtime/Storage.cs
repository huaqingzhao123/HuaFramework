#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Assets.HuaFramework.Examples.Editor
{
    public interface IStorage
    {
        void SaveString(string key, string value);
        string GetString(string key, string defaultValue = "");
    }
    public class PlayStorage : IStorage
    {
        public string GetString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        public void SaveString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }
    }

    public class EditorStorage : IStorage
    {
        public string GetString(string key, string defaultValue = "")
        {
#if UNITY_EDITOR
            return EditorPrefs.GetString(key, defaultValue);
#else
            return "";
#endif
        }

        public void SaveString(string key, string value)
        {
#if UNITY_EDITOR
            EditorPrefs.SetString(key, value);
#endif
        }
    }
}
