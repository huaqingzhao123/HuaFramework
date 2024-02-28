//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Nireus;
//using UnityEngine;
//using UnityEngine.U2D;

//namespace Nireus
//{
//    public class MultiLanguageSpriteManager : Singleton<MultiLanguageSpriteManager>
//    {
//        public const string SUFFIX = ".spriteatlas";
//        public const string MULTI_LANGUAGE_ATLAS_PATH = PathConst.BUNDLE_RES_TEXTURES + "UI/MultiLanguages/";
//        public MultiLanguageSpriteManager()
//        {
//            Load();
//        }

//#region For MultiLangImage
//        private SpriteAtlas _ui_atlas;

//        private void Load()
//        {
//            var full_path = MULTI_LANGUAGE_ATLAS_PATH;

//            var langType = Lang.getLangType();

//            switch (langType)
//            {
//                case LanguageType.CNS:
//                    full_path += $"CNS/atlas_MultiLanguages_CNS" + SUFFIX;
//                    break;
//                case LanguageType.CNT:
//                     full_path += $"CNT/atlas_MultiLanguages_CNT" + SUFFIX;
//                     break;
//                // case LanguageType.EN:
//                //     full_path += $"EN/atlas_MultiLanguages_EN" + SUFFIX;
//                //     break;
//                case LanguageType.KOR:
//                    full_path += $"KOR/atlas_MultiLanguages_KOR" + SUFFIX;
//                    break;
//                case LanguageType.JP:
//                    full_path += $"JP/atlas_MultiLanguages_JP" + SUFFIX;
//                    break;
//                case LanguageType.VN:
//                    full_path += $"VN/atlas_MultiLanguages_VN" + SUFFIX;
//                    break;
//                case LanguageType.TH:
//                    full_path += $"TH/atlas_MultiLanguages_TH" + SUFFIX;
//                    break;
//                case LanguageType.EN:
//                    full_path += $"EN/atlas_MultiLanguages_EN" + SUFFIX;
//                    break;
//                default:
//                    full_path += $"CNS/atlas_MultiLanguages_CNS" + SUFFIX;
//                    break;
//            }
//            _ui_atlas = AssetManager.Instance.loadSync<SpriteAtlas>(full_path);
//            //AssetManager.Instance.UnloadAssetDeferred(full_path, true);

//            if (!_ui_atlas)
//            {
//                GameDebug.LogError("[MultiLanguageSpriteManager] Load ui_atlas failed.");
//            }
//        }

//        //专用于MultiLangImage，其他地方请勿乱用
//        public Sprite GetUISprite(string name)
//        {
//            return _ui_atlas.GetSprite(name);
//        }
//#endregion

//#region For CodeLoadImage
//        private Dictionary<string, SpriteAtlas> _code_load_atlas_map = new Dictionary<string, SpriteAtlas>();

//        public Sprite GetCodeLoadSprite(string type, string sprite_name)
//        {
//            SpriteAtlas atlas = null;
//            if (!_code_load_atlas_map.TryGetValue(type, out atlas))
//            {
//                atlas = Load(type);
//                _code_load_atlas_map.Add(type, atlas);
//            }
//            return atlas?.GetSprite(sprite_name);
//        }

//        private SpriteAtlas Load(string type)
//        {
//            var full_path = PathConst.BUNDLE_RES_TEXTURES + "UI/";

//            var langType = Lang.getLangType();
//            //full_path += $"MultiLanguages/{nameof(currentLangType)}/Treasure/atlas_MultiLanguages_{nameof(currentLangType)}_{nameof(type)}" + SUFFIX;
//            switch (langType)
//            {
//                // case LanguageType.CNS:
//                //     full_path += $"MultiLanguages/CNS/Treasure/atlas_MultiLanguages_CNS_{type}" + SUFFIX;
//                //     break;
//                // case LanguageType.CNT:
//                //     full_path += $"MultiLanguages/CNT/Treasure/atlas_MultiLanguages_CNT_{type}" + SUFFIX;
//                //     break;
//                // case LanguageType.EN:
//                //     full_path += $"MultiLanguages/EN/Treasure/atlas_MultiLanguages_EN_{type}" + SUFFIX;
//                //     break;
//                // case LanguageType.KOR:
//                //     full_path += $"MultiLanguages/KOR/Treasure/atlas_MultiLanguages_KOR_{type}" + SUFFIX;
//                //     break;
//                // case LanguageType.JP:
//                //     full_path += $"MultiLanguages/JP/Treasure/atlas_MultiLanguages_JP_{type}" + SUFFIX;
//                //     break;
//                default:
//                    full_path += $"MultiLanguages/CNS/Treasure/atlas_MultiLanguages_CNS_{type}" + SUFFIX;
//                    break;
//            }

//            var atlas = AssetManager.Instance.loadSync<SpriteAtlas>(full_path);
//            //AssetManager.Instance.UnloadAssetDeferred(full_path, true);

//            if (!atlas)
//            {
//                GameDebug.LogError($"[MultiLanguageSpriteManager] Load {type} failed.");
//            }
//            return atlas;
//        }
//#endregion
//    }
//}
