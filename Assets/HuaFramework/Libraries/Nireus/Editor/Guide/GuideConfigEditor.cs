using UnityEditor;
using UnityEngine;

namespace Nireus.Editor
{
    public static class GuideConfigEditor
    {
        [MenuItem("GameObject/编辑高亮引导", priority = 49)]
        public static void AddHighlightGuide()
        {
            var go = Selection.activeGameObject;

            //var config_list = GuideConfigAssetManager.Instance.ListHighlightConfigs(go.GetFullname());
            //if (config_list.Count <= 0)
            //{
            //    GuideConfigAssetManager.Instance.AddHighlightConfig(go);
            //}
            //else
            //{
            //    GuideHighlightChooseWindow.Show(config_list);
            //}
        }

        [MenuItem("GameObject/编辑高亮引导", priority = 49, validate = true)]
        public static bool ValidateAddHighlightGuide()
        {
            return Application.isPlaying && Selection.activeGameObject.IsNotNull();
        }
    }
}
