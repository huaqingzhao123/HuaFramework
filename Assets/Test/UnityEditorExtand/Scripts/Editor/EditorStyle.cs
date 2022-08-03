using HuaFramework.Configs;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CustomEditor用于为一个Mono脚本设置inspector样式
/// </summary>
[CustomEditor(typeof(MonoTest))]
[CanEditMultipleObjects]
public class EditorStyle : Editor
{
    SerializedProperty autoConfig;
    SerializedProperty id;
    private void OnEnable()
    {
        autoConfig = serializedObject.FindProperty("autoGenConfig");
        id = serializedObject.FindProperty("Id");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(id,new GUIContent("测试Id"));
        EditorGUILayout.PropertyField(autoConfig,new GUIContent("配置"));
        serializedObject.ApplyModifiedProperties();
    }
}
