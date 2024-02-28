using Nireus;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Nireus.Editor
{
    public class AtlasPackEditor : OdinEditorWindow
    {
        [MenuItem("Tools/打包图集", false, 1100)]
        public static AtlasPackEditor OpenWindow()
        {
            AtlasPackEditor window = GetWindow<AtlasPackEditor>();
            window.Show();
            return window;
        }

        const string TEXTURE_COMMON_FULL_DIR = "Textures/UI";
        private static int[] Padding_Array = new int[] { 2, 4, 8 };
        private static int[] Texture_Size = new int[] { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };

        private float Progress_Max = 100;
        private string _Current_Atlas_Name = "";

        [Title("源目录")]
        [InfoBox("输入源目录", InfoMessageType.Error, VisibleIf = "GetSourceDirLength")]
        public string _SourceDir = PathConst.ASSETS + TEXTURE_COMMON_FULL_DIR;

        public bool GetSourceDirLength()
        {
            return string.IsNullOrEmpty(_SourceDir);
        }

        [Title("目标目录")]
        [InfoBox("输入目标目录", InfoMessageType.Error, VisibleIf = "GetTargetDirLength")]
        public string _TargetDir = PathConst.BUNDLE_RES + TEXTURE_COMMON_FULL_DIR;
        public bool _IncludeInBuild = false;

        public bool GetTargetDirLength()
        {
            return string.IsNullOrEmpty(_TargetDir);
        }

        [Title("Packing配置")]
        public bool _AllowRotation = true;
        public bool _TightPacking;
        [ValueDropdown("Padding_Array")]
        public int _Padding = 4;

        [Title("Texture配置")]
        public bool _ReadWriteEnabled;
        public bool _GenerateMipMaps;
        public bool _sRGB = true;
        [EnumPaging]
        public FilterMode _FilterMode = FilterMode.Bilinear;

        //[Title("DefaultTexturePlatform平台配置")]
        //[ValueDropdown("Texture_Size")]
        //[LabelText("MaxTextureSize")]
        //public int _MaxTextureSize = 2048;
        //[LabelText("Format")]
        //public TextureImporterFormat _Format = TextureImporterFormat.Automatic;
        //[LabelText("Compression")]
        //public TextureImporterCompression _Compression = TextureImporterCompression.Compressed;
        //[LabelText("UseCrunchCompress")]
        //public bool _UseCrunchCompress = false;


        [Title("Android平台配置")]
        [ValueDropdown("Texture_Size")]
        [LabelText("MaxTextureSize")]
        public int Android_MaxTextureSize = 2048;
        [LabelText("Format")]
        public TextureImporterFormat Android_Format = TextureImporterFormat.ETC_RGB4;
        [LabelText("Compression")]
        public TextureImporterCompression Android_Compression = TextureImporterCompression.Compressed;
        [LabelText("UseCrunchCompress")]
        public bool Android_UseCrunchCompress = false;

        [Title("iPhone平台配置")]
        [ValueDropdown("Texture_Size")]
        [LabelText("MaxTextureSize")]
        public int iPhone_MaxTextureSize = 1024;
        [LabelText("Format")]
        public TextureImporterFormat iPhone_Format = TextureImporterFormat.PVRTC_RGBA4;
        [LabelText("Compression")]
        public TextureImporterCompression iPhone_Compression = TextureImporterCompression.Compressed;
        [LabelText("UseCrunchCompress")]
        public bool iPhone_UseCrunchCompress = false;

        [Title("打包进度")]
        [ProgressBar("Pregress_Min","Progress_Max")]
        public float _Progress = 0;

        [Title("命令")]
        [Button("开始打包", ButtonSizes.Large)]
        public void PackTextures2Atlas()
        {
            if (_SourceDir.Length <= 0 || _TargetDir.Length <= 0)
            {
                GameDebug.LogError("路径错误");
                return;
            }
            if (!Directory.Exists(_SourceDir))
            {
                GameDebug.LogError("路径错误");
                return;
            }
            _Progress = 0;
            Progress_Max = 0;
            _Current_Atlas_Name = "";

            DirectoryInfo folder1 = new DirectoryInfo(_SourceDir);
            Progress_Max = GetNeedPackNum(folder1);
            Debug.Log("需要打包图集数量-----------》" + Progress_Max);


            SpriteAtlasPackingSettings packingSettings = new SpriteAtlasPackingSettings()
            {
                enableRotation = _AllowRotation,
                enableTightPacking = _TightPacking,
                padding = _Padding,
                blockOffset = 1,
            };

            SpriteAtlasTextureSettings textureSettings = new SpriteAtlasTextureSettings()
            {
                anisoLevel = 1,
                readable = _ReadWriteEnabled,
                generateMipMaps = _GenerateMipMaps,
                sRGB = _sRGB,
                filterMode = _FilterMode
            };

            TextureImporterPlatformSettings Android_platformSettings = new TextureImporterPlatformSettings()
            {
                name = "Android",
                maxTextureSize = Android_MaxTextureSize,
                resizeAlgorithm = 0,
                format = Android_Format,
                textureCompression = Android_Compression,
                compressionQuality = 100,
                crunchedCompression = Android_UseCrunchCompress,
                allowsAlphaSplitting = true,
                overridden = true,
                androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings,
            };

            TextureImporterPlatformSettings iPhone_platformSettings = new TextureImporterPlatformSettings()
            {
                name = "iPhone",
                maxTextureSize = iPhone_MaxTextureSize,
                resizeAlgorithm = 0,
                format = iPhone_Format,
                textureCompression = iPhone_Compression,
                compressionQuality = 100,
                crunchedCompression = iPhone_UseCrunchCompress,
                allowsAlphaSplitting = false,
                overridden = true,
                androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings,
                
            };

            DirectoryInfo folder = new DirectoryInfo(_SourceDir);
            var dirs = folder.GetDirectories();
            List<string> pre_path = new List<string>();
            if (dirs.Length > 0)
            {
                for (int i = 0; i < dirs.Length; i++)
                {
                    pre_path.Clear();
                    DoPackInDir(pre_path, dirs[i], packingSettings, textureSettings, Android_platformSettings, iPhone_platformSettings);
                }
            }
            Debug.Log("------------------------Pack Success----------------------");
        }

        private int GetNeedPackNum(DirectoryInfo folder)
        {
            int atlas_num = 0;
            var pngs = folder.GetFiles("*.png");
            if(pngs.Length > 0)
            {
                string target_path = folder.FullName.Replace("\\","/").Replace(_SourceDir, _TargetDir);
                if(!Directory.Exists(target_path))
                {
                    atlas_num++;
                }
            }
            var dirs = folder.GetDirectories();
            if (dirs.Length > 0)
            {
                for (int i = 0; i < dirs.Length; i++)
                {
                    atlas_num += GetNeedPackNum(dirs[i]);
                }
            }
            return atlas_num;
        }

        private void DoPackInDir(List<string> pre_path, DirectoryInfo current_dir, SpriteAtlasPackingSettings packingSettings, SpriteAtlasTextureSettings textureSettings,
                                TextureImporterPlatformSettings Android_platformSettings, TextureImporterPlatformSettings iPhone_platformSettings)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            for (int i = 0; i < pre_path.Count; i++)
            {
                builder.Append(pre_path[i]);
                builder.Append("/");
            }
            string dir_name = $"{_TargetDir}/{builder.ToString()}{current_dir.Name}";            
			if (!Directory.Exists(dir_name))
            {
                Debug.Log($"打包目录{dir_name}");
                var pics = current_dir.GetFiles("*.png");
                if (pics.Length > 0)
                {

                    Directory.CreateDirectory(dir_name);

                    SpriteAtlas atlas = new SpriteAtlas();
                    atlas.SetIncludeInBuild(_IncludeInBuild);
                    atlas.SetPackingSettings(packingSettings);
                    atlas.SetTextureSettings(textureSettings);
                    atlas.SetPlatformSettings(iPhone_platformSettings);
                    atlas.SetPlatformSettings(Android_platformSettings);


                    System.Text.StringBuilder name_builder = new System.Text.StringBuilder();
                    for (int i = 0; i < pre_path.Count; i++)
                    {
                        name_builder.Append(pre_path[i]);
                        name_builder.Append("_");
                    }
                    string atlas_name = $"atlas_{name_builder.ToString()}{current_dir.Name}.spriteatlas";
                    AssetDatabase.CreateAsset(atlas, $"{dir_name}/{atlas_name}");

                    //foreach (var pic in pics)
                    //{
                    //    Debug.Log($"{PathConst.ASSETS}{_SourceDir}/{current_dir.Name}/{pic.Name}");
                    //    atlas.Add(new[] { AssetDatabase.LoadAssetAtPath<Sprite>($"{_SourceDir}/{current_dir.Name}/{pic.Name}") });
                    //}

                    Object obj = AssetDatabase.LoadMainAssetAtPath($"{_SourceDir}/{builder.ToString()}{current_dir.Name}");
                    atlas.Add(new[] { obj });

                    AssetDatabase.SaveAssets();
                    _Progress++;
                    _Current_Atlas_Name = atlas_name;
                }
            }
            
            var dirs = current_dir.GetDirectories();
            if (dirs.Length > 0)
            {
                pre_path.Add(current_dir.Name);
                for (int i = 0; i < dirs.Length; i++)
                {
                    DoPackInDir(pre_path, dirs[i], packingSettings, textureSettings, Android_platformSettings, iPhone_platformSettings);
                }
            }
        }
    }
}

