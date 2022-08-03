using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace EditorTest
{
    public class WindowsPanel:EditorWindow
    {
        [MenuItem("Test/ShowWindow")]
        public static void ShowWindow()
        {
            //GetWindowWithRect(typeof(WindowsPanel),new Rect(100,2000,1000,1000));
          var window= GetWindow<WindowsPanel>();
            window.titleContent.text = "我的窗口";
            window.Show(); 
        }
        string text = "text\nAreaaaaaaaaaaaaaaaaaaa";
        private void OnGUI()
        {
            //EditorGUILayout.Space();
            //GUILayout.BeginVertical();
            //GUILayout.TextField("窗口"/* new Rect(0,0,50,30), "窗口",GUI.skin.button*/);
            var textTemp= GUILayout.TextArea(text, 200);
            text = textTemp;
            GUILayout.Box("我的窗口");
            if (GUILayout.Button("改变内容"))
            {
                text+= "0";
            }
            //GUILayout.HorizontalScrollbar(/*new Rect(50, 50, 500, 50),*/10,10,50,100);
            //GUILayout.EndVertical();

        }
    }


}
