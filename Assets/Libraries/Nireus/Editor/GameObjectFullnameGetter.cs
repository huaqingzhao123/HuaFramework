using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Nireus.Editor
{
    public static class GameObjectFullnameGetter
    {
        /// <summary>
        /// Hierarchy选中某一gameObject，右键菜单此选项可打印其完整路径，并复制到系统剪贴板
        /// </summary>
        [MenuItem("GameObject/Get GameObject Fullname", priority = 49)]
        private static void Menu_GetGameObjectFullname()
        {
            var selectedObj = Selection.activeGameObject;
            var fn = GetGameObjectFullname(selectedObj);
            GUIUtility.systemCopyBuffer = fn;
            Debug.Log(fn);
        }

        public static string GetGameObjectFullname(GameObject obj)
        {
            return obj.GetFullname();
        }
    }
}