using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


namespace Nireus.Editor
{
    public static class MoveImageToLanguageFlod
    {
        public const string MULTI_LANGUAGE_TEXTURES_PATH = "Assets/Res/Textures/MultiLanguages/";
        public const string MULTI_LANGUAGE_TEXTURES_PATH_CNS = "Assets/Res/Textures/MultiLanguages/CNS/";
        
        /// <summary>
        /// Hierarchy选中某一gameObject，右键菜单此选项可打印其完整路径，并复制到系统剪贴板
        /// </summary>
        [MenuItem("GameObject/Add MultiLangImage Component", priority = 40)]
        private static void Menu_MoveImageToLanguageFlod()
        {
            var selectedObj = Selection.activeGameObject;
            Image image = selectedObj.GetComponent<Image>();
            string path = AssetDatabase.GetAssetPath (image.sprite.texture);
            var mul_image = selectedObj.AddComponentIfNeeded<MultiLangImage>();
            
            UnityEditor.Undo.RecordObject(mul_image, "Add MultiLangImage Component");

            var new_path = MoveFileToNewFold(path);
            mul_image.ImageName = new_path;
            
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(mul_image);
            
            Debug.Log(path);
        }

        [MenuItem("Assets/Move Texture To LanguageFolder")]
        static void Asset_MoveTextureToLanguageFlod()
        {
            foreach (var obj in Selection.objects)
            {
                if(obj is Texture)
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    MoveFileToNewFold(path);
                }

            }
            AssetDatabase.Refresh();
        }

        private static string MoveFileToNewFold(string srcPath)
        {
            if (srcPath.Contains(MULTI_LANGUAGE_TEXTURES_PATH))
            {
                return srcPath;
            }
            string file_name = Path.GetFileName(srcPath);
            var dic_name = Path.GetFileName(Path.GetDirectoryName(srcPath));
            var destDir = MULTI_LANGUAGE_TEXTURES_PATH_CNS + dic_name + "_" +file_name;
            AssetDatabase.MoveAsset(srcPath, destDir);
            AssetDatabase.ImportAsset(destDir);
            return destDir;
        }
    }
}