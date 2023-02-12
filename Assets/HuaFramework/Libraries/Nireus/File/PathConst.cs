using System;

namespace Nireus
{
    public static partial class PathConst
    {
        public const string ASSETS = "Assets/";

        /// <summary>
        /// 此目录下的资源会打成AssetBundle
        /// </summary>
        public const string BUNDLE_RES = ASSETS + "Res/BundleRes/";
        public const string BUNDLE_RES_META = ASSETS + "Res/BundleRes.meta";

        /// <summary>
        /// Assets/Resources
        /// </summary>
        public const string RESOURCES = ASSETS + "Resources/";

        /// <summary>
        /// Assets/Resources/BundleRes
        /// </summary>
        public const string RESOURCES_BUNDLE_RES = RESOURCES + "BundleRes/";
        public const string RESOURCES_BUNDLE_RES_META = RESOURCES + "BundleRes.meta";

        // Audio
        public const string BUNDLE_RES_AUDIO = BUNDLE_RES + "Audio/";
        public const string BUNDLE_RES_AUDIO_MUSIC = BUNDLE_RES_AUDIO + "Music@/";
        public const string BUNDLE_RES_AUDIO_SFX = BUNDLE_RES_AUDIO + "SFX/";
        public const string BUNDLE_RES_AUDIO_VOICE = BUNDLE_RES_AUDIO + "Voice/";

        // Config
        public const string BUNDLE_RES_CFG = BUNDLE_RES + "Config/";
        public const string CFG_LANG_CNS = "lang/cfg_lang_cns"; //语言包【中文简体】
        public const string CFG_LANG_CNT = "lang/cfg_lang_cnt"; //语言包【中文繁体】
        public const string CFG_LANG_EN = "lang/cfg_lang_en"; //语言包【英文】
        public const string CFG_LANG_KOR = "lang/cfg_lang_kor"; //语言包【韩文】
        public const string CFG_LANG_JP = "lang/cfg_lang_jp"; //语言包【日文】
        public const string CFG_LANG_VN = "lang/cfg_lang_vn"; //语言包【越南】
        public const string CFG_LANG_TH = "lang/cfg_lang_th"; //语言包【泰语】
        public const string CFG_LANG_DEF = CFG_LANG_CNS;
        public const string CFG_LANG_UI_CNS = "lang/cfg_lang_ui_cns"; //UI语言包【中文简体】
        public const string CFG_LANG_UI_CNT = "lang/cfg_lang_ui_cnt"; //UI语言包【中文繁体】
        public const string CFG_LANG_UI_EN = "lang/cfg_lang_ui_en"; //UI语言包【英文】
        public const string CFG_LANG_UI_KOR = "lang/cfg_lang_ui_kor"; //UI语言包【韩文】
        public const string CFG_LANG_UI_JP = "lang/cfg_lang_ui_jp"; //UI语言包【日文】
        public const string CFG_LANG_UI_VN = "lang/cfg_lang_ui_vn"; //UI语言包【越南】
        public const string CFG_LANG_UI_DEF = CFG_LANG_UI_CNS;
        public const string CFG_CNS = "cfg_cns"; //配置表【中文简体】
        public const string CFG_CNT = "cfg_cnt"; //配置表【中文繁体】
        public const string CFG_EN = "cfg_en"; //配置表【英文】
        public const string CFG_KOR = "cfg_kor"; //配置表【韩文】
        public const string CFG_JP = "cfg_jp"; //配置表【日文】
        public const string CFG_DEF = CFG_CNS;

        public const string BUNDLE_RES_PREFABS = BUNDLE_RES + "Prefabs/";
        public const string BUNDLE_RES_UI_PREFABS = BUNDLE_RES + "UIPrefabs/";
        public const string PATH_UI_EFFECT = BUNDLE_RES + "EffPrefabs/UIEffect@/";
        public const string BUNDLE_RES_TEXTURES = BUNDLE_RES + "Textures/";
        public const string BUNDLE_RES_TEXT = BUNDLE_RES + "Text/";
        public const string BUNDLE_RES_SPINE = BUNDLE_RES + "Spine/";
        public const string SCRIPTS_PATH= "Assets/Scripts/";
        public const string SCRIPTS_UIBINDER_GENERATED = SCRIPTS_PATH + "UIBinderGenerated/";
        public const string SCRIPTS_UIBINDER_SUB_GENERATED = SCRIPTS_PATH + "UIBinderGeneratedSubclassTemp/";

        public const string ASSETS_VERSION_INFO_FILENAME = "assets_version_info.txt";
        public const string CFG_COMMON_GLOBAL = "common/cfg_global_game";
        
        public const string INGAME_DEBUG_CONSOLE_PATH = "Prefabs/IngameDebugConsole";

    }
}