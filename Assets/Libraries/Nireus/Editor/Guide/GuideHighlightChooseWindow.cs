//using Sirenix.OdinInspector;
//using Sirenix.OdinInspector.Editor;
//using System.Collections.Generic;

//public class GuideHighlightChooseWindow : OdinEditorWindow
//{
//    protected override void OnEnable()
//    {
//        base.OnEnable();
//        if (HighlightConfigList.Count > 0)
//        {
//            Select_Config_Id = HighlightConfigList[0];
//        }
//    }

//    //public static void Show(List<GuideHighlightConfig> config_list)
//    //{
//    //    HighlightConfigList.Clear();
//    //    foreach (var config in config_list)
//    //    {
//    //        HighlightConfigList.Add(config.id);
//    //    }

//    //    GetWindow<GuideHighlightChooseWindow>().Show();
//    //}

//    [ValueDropdown("HighlightConfigList")]
//    [Title("选择需要编辑的高亮配置id")]
//    [HideLabel]
//    public int Select_Config_Id = 0;
//    private static List<int> HighlightConfigList = new List<int>();

//    [PropertySpace(SpaceBefore = 30)]
//    [Button("编辑配置", ButtonSizes.Large), GUIColor(0, 1, 0)]
//    public void EditHighlight()
//    {
//        GuideConfigAssetManager.Instance.EditHighlightConfig(Select_Config_Id);
//        GetWindow<GuideHighlightChooseWindow>().Close();
//    }
//}
