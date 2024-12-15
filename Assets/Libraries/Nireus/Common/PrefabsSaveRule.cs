#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;

using UnityEngine;

namespace Nireus
{
    [InitializeOnLoad]
    public static class PrefabsSaveRuleListener
    {
        private static string _cur_prefab_path = "";
        private static HashSet<string> _path_set = new HashSet<string>();
        static PrefabsSaveRuleListener()
        {
            UnityEditor.SceneManagement.PrefabStage.prefabSaving += OnPrefabSaving;
            UnityEditor.SceneManagement.PrefabStage.prefabStageOpened += OnPrefabOpened;
        }

        private static void OnPrefabOpened(UnityEditor.SceneManagement.PrefabStage obj)
        {
            _cur_prefab_path = obj.prefabAssetPath;
        }

        private static void OnPrefabSaving(GameObject instance)
        {
            string prefabPath = AssetDatabase.GetAssetPath(instance);
            if(string.IsNullOrEmpty(prefabPath))
            {
                prefabPath = _cur_prefab_path;
            }
            GameDebug.Log("UIPrefabsSaveRuleListener check prefab rule = " + prefabPath);
            
            _CheckUIPrefabHasParticleSystem(instance, prefabPath);

        }
        private static void _CheckUIPrefabHasParticleSystem(GameObject instance,string prefab_path)
        {
            GameDebug.Log("UIPrefabsSaveRuleListener _CheckUIPrefabHasParticleSystem start");
            bool need_check = false;
            if (!string.IsNullOrEmpty(prefab_path) && !prefab_path.StartsWith($"{PathConst.BUNDLE_RES}Prefabs/EffectPrefabs")
                && !prefab_path.StartsWith($"{PathConst.BUNDLE_RES}Prefabs/EffectPrefabs3D")
                && !prefab_path.StartsWith($"{PathConst.BUNDLE_RES}Prefabs/Product"))
            {
                need_check = true;
            }
            if (need_check)
            {
                _path_set.Clear();
                GameDebug.Log("UIPrefabsSaveRuleListener _CheckUIPrefabHasParticleSystem need check");
                ParticleSystem[] ps_array = instance.GetComponentsInChildren<ParticleSystem>();

                bool has_error = false;
                foreach(var ps in ps_array)
                {
                    if(ps != null)
                    {
                        var ps_path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(ps);
                        if (!string.IsNullOrEmpty(ps_path))
                        {
                            if (_path_set.Contains(ps_path)) continue;
                            if (ps_path.StartsWith($"{PathConst.BUNDLE_RES}Prefabs/EffectPrefabs")
                               || ps_path.StartsWith($"{PathConst.BUNDLE_RES}Prefabs/EffectPrefabs3D")
                               || ps_path.StartsWith($"{PathConst.BUNDLE_RES}Prefabs/Product"))
                            {
                                _path_set.Add(ps_path);
                                continue;
                            }
                            else
                            {
                                has_error = true;
                                break;
                            }
                        }
                        else
                        {
                            has_error = true;
                            break;
                        }
                    }
                }
                if(has_error)
                {
                    UnityEditor.EditorUtility.DisplayDialog("[Error] UI prefab error:", "has particle system,did't save,please fix it", "ok");
                    throw new Exception();
                }
            }
        }
    }
}


#endif