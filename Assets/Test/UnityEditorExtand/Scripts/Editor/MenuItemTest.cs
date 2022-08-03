using UnityEngine;
using UnityEditor;
public class MenuItemTest
{
    [MenuItem("Test/菜单快捷键 &t")]//_+字母为设置字母快捷键,%+字母为Ctrl+字母快捷键, (%=ctrl,#=shift,&=alt)
    public static void TestQuickKey()
    {
        Debug.LogError("按下快捷键");
    }
    [MenuItem("CONTEXT/MonoTest/Move")]//组件添加鼠标右键显示菜单 CONTEXT+组件名+按钮名 MenuCommand.context 所在的组件
    public static void TestScriptInspectorMouseDown(MenuCommand command)
    {
        MonoTest test = command.context as MonoTest;
        Debug.Log(test.autoGenConfig);
    }

    /// <summary>
    /// 测试出现在Hierarchy视图的右键菜单中，priority必须小于49,priority相差11可以分组
    /// </summary>

    [MenuItem("GameObject/Test",priority =49)]
    public static void TestGameObject()
    {
    }

    [MenuItem("GameObject/Test", true,49)]//第二个参数true表示此方法是同名Menuiten的验证方法
    static bool VerifyTestGameObject()
    {
        //条件逻辑
        return true;
    }

}
