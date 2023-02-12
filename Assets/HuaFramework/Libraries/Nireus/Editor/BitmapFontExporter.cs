using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;
using System;

namespace Nireus.Editor
{
    public class BitmapFontExporter : ScriptableWizard
    {
        [MenuItem("Nireus/BitmapFontExporter/Create")]
        private static void CreateFont()
        {
            ScriptableWizard.DisplayWizard<BitmapFontExporter>("Create Font");
        }


        public TextAsset fontFile;
        public Texture2D textureFile;

        private void OnWizardCreate()
        {
            if (fontFile == null || textureFile == null)
            {
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject("Save Font", fontFile.name, "", "Please");

            if (!string.IsNullOrEmpty(path))
            {
                ResolveFont(path);
            }
        }


        private void ResolveFont(string exportPath)
        {
            if (!fontFile) throw new UnityException(fontFile.name + "is not a valid font-xml file");

            Font font = new Font();

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(fontFile.text);

            XmlNode bmInfo = xml.GetElementsByTagName("info")[0];
            XmlNodeList chars = xml.GetElementsByTagName("chars")[0].ChildNodes;

            CharacterInfo[] charInfos = new CharacterInfo[chars.Count];

            for (int cnt = 0; cnt < chars.Count; cnt++)
            {
                XmlNode node = chars[cnt];
                CharacterInfo info = new CharacterInfo();

                int id = ToInt(node, "id");
                float x = ToFloat(node, "x");
                float y = ToFloat(node, "y");
                int width = ToInt(node, "width");
                int height = ToInt(node, "height");
                float texWidth = textureFile.width;
                float texHeight = textureFile.height;
                int offsetX = ToInt(node, "xoffset");
                int offsetY = ToInt(node, "yoffset");
                int xadvance = ToInt(node, "xadvance");

                info.index = id;
                float uvx = 1f * x / texWidth;
                float uvy = 1 - (1f * y / texHeight);
                float uvw = 1f * width / texWidth;
                float uvh = -1f * height / texHeight;

                info.uvBottomLeft = new Vector2(uvx, uvy);
                info.uvBottomRight = new Vector2(uvx + uvw, uvy);
                info.uvTopLeft = new Vector2(uvx, uvy + uvh);
                info.uvTopRight = new Vector2(uvx + uvw, uvy + uvh);

                //Unity5 api 参考网上文章
                info.minX = offsetX;
                info.minY = offsetY + height / 2;
                info.glyphWidth = width;
                info.glyphHeight = -height;
                info.advance = xadvance;

                charInfos[cnt] = info;
            }


            Shader shader = Shader.Find("GUI/Text Shader");
            Material material = new Material(shader);
            material.mainTexture = textureFile;
            AssetDatabase.CreateAsset(material, exportPath + ".mat");


            font.material = material;
            font.name = bmInfo.Attributes.GetNamedItem("face").InnerText;
            font.characterInfo = charInfos;

            //不知道为什么,字体文件要重新导入才能用..
            string fontFullPath = exportPath + ".fontsettings";
            AssetDatabase.CreateAsset(font, fontFullPath);
            AssetDatabase.ExportPackage(fontFullPath, "tmp.unitypackage");
            AssetDatabase.DeleteAsset(fontFullPath);
            AssetDatabase.ImportPackage("tmp.unitypackage", true);

            AssetDatabase.Refresh();

        }



        private int ToInt(XmlNode node, string name)
        {
            return Convert.ToInt32(node.Attributes.GetNamedItem(name).InnerText);
        }


        private float ToFloat(XmlNode node, string name)
        {
            return (float) ToInt(node, name);
        }
    }
}