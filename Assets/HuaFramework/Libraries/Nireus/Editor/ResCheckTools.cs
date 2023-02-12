using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ResCheck : MonoBehaviour
{
    [MenuItem("Tools/Report/All/删除Prefab粒子特效无用的Mesh")]
    static void AllReportTexture()
    {
        Dictionary<string, string> md5dic = new Dictionary<string, string>();
        string[] paths = AssetDatabase.FindAssets("t:prefab", new string[] { "Assets/Res/BundleRes" });

        foreach (var prefabGuid in paths)
        {
            GameObject goTemp = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGuid));
            int child_count = goTemp.transform.childCount;

            for (int i = 0; i < child_count; i++)
                //foreach (var item in gos)
            {
                // Filter non-prefab type,
                Transform temp_obj = goTemp.transform.GetChild(i);
                if (PrefabUtility.GetPrefabAssetType(temp_obj) != PrefabAssetType.NotAPrefab)
                {
                    GameObject go = temp_obj.gameObject; //as GameObject;
                    if (go)
                    {
                        ParticleSystemRenderer[] renders = go.GetComponentsInChildren<ParticleSystemRenderer>(true);
                        foreach (var renderItem in renders)
                        {
                            if (renderItem.renderMode != ParticleSystemRenderMode.Mesh)
                            {
                                renderItem.mesh = null;
                                EditorUtility.SetDirty(go);
                            }
                        }
                    }
                }
            }

            Debug.Log("<color=green> CheckParticleDependency Success </color>");
            AssetDatabase.SaveAssets();
        }
    }

    [MenuItem("Tools/Report/All/所有Prefab重新序列化")]
    static void AllCheckSelectionCommonPrefab()
    {
        Dictionary<string, string> md5dic = new Dictionary<string, string>();
        string[] paths = AssetDatabase.FindAssets("t:prefab", new string[] { "Assets/Res/BundleRes" });

        foreach (var prefabGuid in paths)
        {
            GameObject goTemp = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGuid));
            int child_count = goTemp.transform.childCount;

            for (int i = 0; i < child_count; i++)
                //foreach (var item in gos)
            {
                // Filter non-prefab type,
                Transform temp_obj = goTemp.transform.GetChild(i);
                if (PrefabUtility.GetPrefabAssetType(temp_obj) != PrefabAssetType.NotAPrefab)
                {
                    GameObject go = temp_obj.gameObject; //as GameObject;
                    if (go)
                    {
                        EditorUtility.SetDirty(go);
                    }
                }
            }
        }
        Debug.Log("<color=green> CheckParticleDependency Success </color>");
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/Report/All/去除Meaterial的冗余引用")]
    public static void AllCheckMaterialPropertyDependency()
    {
        int iCounts = 0;
        System.Text.StringBuilder
            sb = new System.Text.StringBuilder("Find and clean useless texture propreties name: ");
        Dictionary<string, string> md5dic = new Dictionary<string, string>();
        string[] paths = AssetDatabase.FindAssets("t:Material", new string[] {"Assets"});
        foreach (var prefabGuid in paths)
        {
            Material goTemp = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(prefabGuid));
            if (goTemp)
            {
                SerializedObject psSource = new SerializedObject(goTemp);
                SerializedProperty emissionProperty = psSource.FindProperty("m_SavedProperties");
                SerializedProperty texEnvs = emissionProperty.FindPropertyRelative("m_TexEnvs");
                SerializedProperty floats = emissionProperty.FindPropertyRelative("m_Floats");
                SerializedProperty colos = emissionProperty.FindPropertyRelative("m_Colors");

                bool isCount = false;
                if (CleanMaterialSerializedProperty(texEnvs, goTemp))
                {
                    if (!isCount && iCounts < 1000)
                    {
                        sb.Append(" /Texture- ");
                        sb.Append(goTemp.name);
                    }

                    isCount = true;
                }
                //if (CleanMaterialSerializedProperty(floats, mats[i]))
                //{
                //	if (!isCount && iCounts < 1000)
                //	{
                //		sb.Append(" /Value- ");
                //		sb.Append(mats[i].name);
                //	}
                //	isCount = true;
                //}
                //if (CleanMaterialSerializedProperty(colos, mats[i]))
                //{
                //	if (!isCount && iCounts < 1000)
                //	{
                //		sb.Append(" /Color- ");
                //		sb.Append(mats[i].name);
                //	}
                //	isCount = true;
                //}

                if (isCount)
                {
                    iCounts++;
                }

                psSource.ApplyModifiedProperties();
                EditorUtility.SetDirty(goTemp);
            }
        }
        Debug.Log(
            $"<color=green>CheckMaterialPropertyDependency success counts: {(iCounts > 1000 ? 999 : iCounts)}</color>");
        Debug.Log(
            $"<color=green>CheckMaterialPropertyDependency success useless propeties names: {sb.ToString()}</color>");

        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/Report/Single/选中Prefab重新序列化")]
    public static void CheckSelectionCommonPrefab()
    {
        Object[] gos = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        foreach (var item in gos)
        {
            // Filter non-prefab type,
            if (PrefabUtility.GetPrefabAssetType(item) != PrefabAssetType.NotAPrefab)
            {
                GameObject go = item as GameObject;
                if (go)
                {
                    EditorUtility.SetDirty(go);
                }
            }
        }

        Debug.Log("<color=green> CheckSelectionPrefabDependency Success </color>");
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/Report/Single/删除选中Prefab粒子特效无用Mesh")]
    public static void CheckParticleSystemPrefab()
    {
        Object[] gos = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        foreach (var item in gos)
        {
            // Filter non-prefab type,
            if (PrefabUtility.GetPrefabAssetType(item) != PrefabAssetType.NotAPrefab)
            {
                GameObject go = item as GameObject;
                if (go)
                {
                    ParticleSystemRenderer[] renders = go.GetComponentsInChildren<ParticleSystemRenderer>(true);
                    foreach (var renderItem in renders)
                    {
                        if (renderItem.renderMode != ParticleSystemRenderMode.Mesh)
                        {
                            renderItem.mesh = null;
                            EditorUtility.SetDirty(go);
                        }
                    }
                }
            }
        }

        Debug.Log("<color=green> CheckParticleDependency Success </color>");
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/Report/Single/去除选中Meaterial的冗余引用")]
    public static void CheckMaterialPropertyDependency()
    {
        int iCounts = 0;
        System.Text.StringBuilder
            sb = new System.Text.StringBuilder("Find and clean useless texture propreties name: ");
        Material[] mats = Selection.GetFiltered<Material>(SelectionMode.DeepAssets);
        for (int i = 0; i < mats.Length; ++i)
        {
            if (mats[i])
            {
                SerializedObject psSource = new SerializedObject(mats[i]);
                SerializedProperty emissionProperty = psSource.FindProperty("m_SavedProperties");
                SerializedProperty texEnvs = emissionProperty.FindPropertyRelative("m_TexEnvs");
                SerializedProperty floats = emissionProperty.FindPropertyRelative("m_Floats");
                SerializedProperty colos = emissionProperty.FindPropertyRelative("m_Colors");

                bool isCount = false;
                if (CleanMaterialSerializedProperty(texEnvs, mats[i]))
                {
                    if (!isCount && iCounts < 1000)
                    {
                        sb.Append(" /Texture- ");
                        sb.Append(mats[i].name);
                    }

                    isCount = true;
                }
                //if (CleanMaterialSerializedProperty(floats, mats[i]))
                //{
                //	if (!isCount && iCounts < 1000)
                //	{
                //		sb.Append(" /Value- ");
                //		sb.Append(mats[i].name);
                //	}
                //	isCount = true;
                //}
                //if (CleanMaterialSerializedProperty(colos, mats[i]))
                //{
                //	if (!isCount && iCounts < 1000)
                //	{
                //		sb.Append(" /Color- ");
                //		sb.Append(mats[i].name);
                //	}
                //	isCount = true;
                //}

                if (isCount)
                {
                    iCounts++;
                }

                psSource.ApplyModifiedProperties();
                EditorUtility.SetDirty(mats[i]);
            }
        }

        Debug.Log(
            $"<color=green>CheckMaterialPropertyDependency success counts: {(iCounts > 1000 ? 999 : iCounts)}</color>");
        Debug.Log(
            $"<color=green>CheckMaterialPropertyDependency success useless propeties names: {sb.ToString()}</color>");

        AssetDatabase.SaveAssets();
    }

    private static bool CleanMaterialSerializedProperty(SerializedProperty property_, Material mat_)
    {
        bool isFind = false;

        for (int i = property_.arraySize - 1; i >= 0; i--)
        {
            string propertyName = property_.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue;

            if (!mat_.HasProperty(propertyName))
            {
                if (propertyName.Equals("_MainTex"))
                {
                    if (property_.GetArrayElementAtIndex(i).FindPropertyRelative("second")
                        .FindPropertyRelative("m_Texture").objectReferenceValue != null)
                    {
                        property_.GetArrayElementAtIndex(i).FindPropertyRelative("second")
                            .FindPropertyRelative("m_Texture").objectReferenceValue = null;
                        isFind = true;
                    }
                }
                else
                {
                    property_.DeleteArrayElementAtIndex(i);
                    isFind = true;
                }
            }
        }

        return isFind;
    }
}