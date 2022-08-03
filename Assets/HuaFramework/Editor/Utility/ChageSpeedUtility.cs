using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace HuaFramework.Editor.Utility
{
    public class ChageSpeedUtility : EditorWindow
    {

        [MenuItem("HuaFramework/Tools/测试变速工具 &d")]
        public static void OnOpenWindow()
        {
            var window = GetWindow(typeof(ChageSpeedUtility));
            window.titleContent = new GUIContent("测试变速工具");
            window.Show();
        }
        float time = 1;
        float height = 50;
      
        private void OnGUI()
        {
            GUILayout.BeginVertical();
            {
                GUIStyle style = new GUIStyle(GUI.skin.button) { richText = true };
                GUILayout.TextField("<b><color=yellow><size=50>测试变速工具</size></color></b>", style, GUILayout.Height(height));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("<b><color=yellow><size=30>0.1x</size></color></b>", style, GUILayout.Height(height)))
                    time = 0.1f;
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("<b><color=yellow><size=30>0.2x</size></color></b>", style, GUILayout.Height(height)))
                        time = 0.2f;
                    if (GUILayout.Button("<b><color=yellow><size=30>0.5x</size></color></b>", style, GUILayout.Height(height)))
                        time = 0.5f;
                }
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("<b><color=yellow><size=30>1x</size></color></b>", style, GUILayout.Height(height)))
                    time = 1f;
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("<b><color=yellow><size=30>2x</size></color></b>", style, GUILayout.Height(height)))
                    time = 2f;
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("<b><color=yellow><size=30>5x</size></color></b>", style, GUILayout.Height(height)))
                    time = 5f;

            }
            Time.timeScale = time;
            GUILayout.EndVertical();
        }
    }

}
