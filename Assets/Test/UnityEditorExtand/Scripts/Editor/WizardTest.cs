using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class WizardTest : ScriptableWizard
{
    public int Id=10;
    [MenuItem("Test/显示对话框")]
    public static void ShowDialogOrWizard()
    {
        //EditorUtility.DisplayDialog("对话框", "我是对话框", "确定");
        //otherButton所在的第三个参数可多创建一个按钮
        ScriptableWizard.DisplayWizard<WizardTest>("处理向导", "Handle","otherButton");
    }

    /// <summary>
    /// 窗口被创建时调用，可进行初始化操作
    /// </summary>
    private void OnEnable()
    {
            
    }
    /// <summary>
    /// Create第二个参数名,按钮点击,点击后会关闭窗口
    /// </summary>
    private void OnWizardCreate()
    {
        GameObject[] prefabs = Selection.gameObjects;
        foreach (var item in prefabs)
        {
            var monotest = item.GetComponent<MonoTest>();
            //可撤销操作Undo，使用后此操作可Ctrl+z回退下面操作
            Undo.RecordObject(monotest, "change id");
            Undo.RecordObject(item, "change active");
            monotest.Id += Id;
            item.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// 属性值被改变时一直调用，打开时也调用一次
    /// </summary>
    private void OnWizardUpdate()
    {
        //窗口显示提示信息的方法
        ShowNotification(new GUIContent(string.Format("Wizard打开或者属性值被改变")));
    }

    /// <summary>
    /// otherButton的点击事件,第三个参数指定，点击后不会关闭窗口
    /// </summary>
    private void OnWizardOtherButton()
    {
        //窗口显示提示信息的方法
        ShowNotification(new GUIContent(string.Format("点击otherButton按钮")));
    }
    
}
