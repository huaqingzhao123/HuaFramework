using HuaFramework.Architecture;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
# endif
using UnityEngine;


public interface IStorage:IUtility
{
    void SaveInt(string key, int value);
    int GetInt(string key, int defaultValue = 0);
}
public class PlayStorage : IStorage
{
    public int GetInt(string key, int defaultValue =0)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    public void SaveInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }
}

public class EditorStorage : IStorage
{
    public int GetInt(string key, int defaultValue = 0)
    {
#if UNITY_EDITOR
        return EditorPrefs.GetInt(key,defaultValue);
#else
            return 0;
#endif
    }

    public void SaveInt(string key, int value)
    {
#if UNITY_EDITOR
        EditorPrefs.SetInt(key, value);
#endif
    }
}
