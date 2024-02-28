using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestWindwo : OdinEditorWindow
{
    [MenuItem("Tools/TestWindow")]
    private static void OpenWindow()
    {
        GetWindow<TestWindwo>()
            .position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
    }
    [LabelText("公用路径")]
    public List<string> common_path = new List<string>()
    {
        "One",
        "Two",

    };
    protected override IEnumerable<object> GetTargets()
    {
        yield return this;
        yield return GUI.skin.settings;
        yield return GUI.skin;
    }
}
