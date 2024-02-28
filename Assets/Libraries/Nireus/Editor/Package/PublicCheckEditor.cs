using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Nireus;
using Nireus.Editor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// OUTER_TEST: 连接游戏测试服
/// OVERSEA：海外版本
/// U8SDK：u8
/// ASSET_BUNDLE：ab资源加载
/// TEST_SDK：搭配 OUTER_TEST，连接测试sdk
/// FORCE_LOGIN_GAME：1、不走分服，强制连测试服；2、GetU8Channel()返回2；3、把输入的uid设置成openid;
/// LOGIN_PANEL：输入账号模式
/// FORBID_FUNCTION：屏蔽未开放的功能
/// ALLOWUNSAFE：FPPhysics 需要
/// MOREMOUNTAINS_NICEVIBRATIONS：MoreMountains 插件开关
/// MOREMOUNTAINS_NICEVIBRATIONS_RUMBLE：MoreMountains 插件开关
/// MOREMOUNTAINS_FEEDBACKS：MoreMountains 插件开关，Injured3D.cs 中有用到
/// UNITY_POST_PROCESSING_STACK_V2：系统宏
/// ODIN_INSPECTOR：odin 插件要求
/// MOREMOUNTAINS_TOOLS：MoreMountains.Tools.Cinemachine 要求
/// MOREMOUNTAINS_INTERFACE：MoreMountains.Interface.Editor 要求
/// INNER_DEVELOP：开发模式下会开启一些功能，可能会无视 OUTER_TEST
/// USE_SUB_PACKAGE：分包
/// ENABLE_DEBUG_LOG: 日志开关
/// INGAME_DEBUG_CONSOLE: 日志悬浮窗
/// </summary>
public class PublicCheckEditor : UnityEditor.Editor
{
	public readonly static string TEST_BUILD_DEFINE_SYMBOLS = "OUTER_TEST;OVERSEA;U8SDK;ASSET_BUNDLE;#TEST_SDK;FORCE_LOGIN_GAME;LOGIN_PANEL;FORBID_FUNCTION;ALLOWUNSAFE;MOREMOUNTAINS_NICEVIBRATIONS;MOREMOUNTAINS_NICEVIBRATIONS_RUMBLE;UNITY_POST_PROCESSING_STACK_V2;ODIN_INSPECTOR;MOREMOUNTAINS_FEEDBACKS;MOREMOUNTAINS_TOOLS;MOREMOUNTAINS_INTERFACE;";
	public readonly static string RELEASE_BUILD_DEFINE_SYMBOLS = "#OUTER_TEST;OVERSEA;U8SDK;ASSET_BUNDLE;#TEST_SDK;#FORCE_LOGIN_GAME;#LOGIN_PANEL;FORBID_FUNCTION;ALLOWUNSAFE;MOREMOUNTAINS_NICEVIBRATIONS;MOREMOUNTAINS_NICEVIBRATIONS_RUMBLE;UNITY_POST_PROCESSING_STACK_V2;ODIN_INSPECTOR;MOREMOUNTAINS_FEEDBACKS;MOREMOUNTAINS_TOOLS;MOREMOUNTAINS_INTERFACE;";

	public static string GetBuildDefineSymbols(bool is_test, bool debug_log, bool debug_console)
	{
		string symbols = TEST_BUILD_DEFINE_SYMBOLS;
		if (!is_test)
			symbols = RELEASE_BUILD_DEFINE_SYMBOLS;
		if (debug_log)
			symbols += "ENABLE_DEBUG_LOG;";
		if (debug_console)
			symbols += "INGAME_DEBUG_CONSOLE;";
		return symbols;
	}

	public readonly static string UI_MODULE_CONFIG_DIR = "Assets/Res/BundleRes/Config/module/package_config.asset";//分模块AssetBundle加载配置文件
    public static void ZhengShiBuildCheck()
    {
        //1.检查宏定义
        ScriptingDefineSymbolsCheck();
        //2.检查分包配置
        CheckFenBaoConfig();
        //3.输出包相关配置
        OutPutProjectConfig();
    }

	private static void ScriptingDefineSymbolsCheck()
    {
		string tar_symbols = GetBuildDefineSymbols(false, false, false);

		string symbols = "";
#if UNITY_ANDROID
        symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
        if (symbols != tar_symbols)
        {
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, tar_symbols);
        }
#elif UNITY_IOS
        symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
        if (symbols != "ASSET_BUNDLE;ODIN_INSPECTOR;BEHAVIAC_RELEASE;#OUTER_TEST;USE_SUB_PACKAGE")
        {
            throw new Exception("宏定义配置错误");
        }
#endif
		if (symbols == "")
        {
            throw new Exception("宏定义配置错误");
        }
        
        Debug.LogFormat("宏定义检查通过");
    }

    private static void CheckFenBaoConfig()
    {
        PackageEditor.CheckModuleConfig();
    }

    private static void OutPutProjectConfig()
    {
        var productName = PlayerSettings.productName;
        Debug.LogFormat("游戏展示名字 :"+productName);

        string bundleVersion = PlayerSettings.bundleVersion;
        Debug.LogFormat("游戏版本号 :"+bundleVersion);
        
        Debug.LogFormat("Package Name :"+Application.identifier);
#if UNITY_ANDROID
        Debug.LogFormat("VersionCode :"+PlayerSettings.Android.bundleVersionCode);
#endif

#if UNITY_IOS
        Debug.LogFormat("Bundle Identifer :"+PlayerSettings.iOS.buildNumber);
#endif
		//Debug.LogFormat("游戏内部版本号 :" + VersionConfig.Client_Version);

	}

	public static void TestBuildCheck()
	{
        //OpenTimeLine();
        //1.检查宏定义
        TestScriptingDefineSymbolsCheck();
		//2.输出包相关配置
		OutPutProjectConfig();
	}

	private static void OpenTimeLine()
	{
		string path = "Assets/Res/BundleRes/Prefabs/Common/PlayGameFightScene.prefab";
		GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
		var timeline = go.transform.FindRecursively("TimeLine");
		if (null == timeline)
			return;
		timeline.gameObject.SetActiveIfNeeded(true);
		timeline.gameObject.SetActiveIfNeeded(false);
		UnityEditor.EditorUtility.SetDirty(go);
		AssetDatabase.Refresh();
		UnityEditor.AssetDatabase.SaveAssets();
	}

	private static void TestScriptingDefineSymbolsCheck()
	{
		string tar_symbols = GetBuildDefineSymbols(true, true, true);
		string symbols = "";
#if UNITY_ANDROID 
        symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
        if (symbols != tar_symbols)
        {
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, tar_symbols);
        }
#elif UNITY_IOS
        symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
        if (symbols != "ASSET_BUNDLE;ODIN_INSPECTOR;BEHAVIAC_RELEASE;#OUTER_TEST;USE_SUB_PACKAGE")
        {
            throw new Exception("宏定义配置错误");
        }
#endif
		if (symbols == "")
		{
			throw new Exception("宏定义配置错误");
		}

		Debug.LogFormat("宏定义检查通过");
	}
}
