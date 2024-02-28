using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Nireus.Editor
{
    // 创建bmfont
    public class CreateFontEditor : UnityEditor.Editor
    {
        [MenuItem("Assets/CreateBMFont")]
        static void CreateFont()
        {
            Object obj = Selection.activeObject;
            string fntPath = AssetDatabase.GetAssetPath(obj);
            Debug.Log("fnt:" + fntPath);
            if (fntPath.IndexOf(".fnt") == -1)
            {
                // 不是字体文件
                Debug.LogError("不是字体文件");
                return;
            }

            string customFontPath = fntPath.Replace(".fnt", ".fontsettings");
            if (!File.Exists(customFontPath))
            {
                Debug.LogError("CustomFont doesnt exist!");
                return;
            }
            Debug.Log("Start reading fnt file ...");

            Debug.Log(fntPath);
            var fs = new FileStream(fntPath, FileMode.Open);
            StreamReader reader = new StreamReader(fs);

            List<CharacterInfo> charList = new List<CharacterInfo>();

            Regex reg = new Regex(@"char id=(?<id>\d+)\s+x=(?<x>\d+)\s+y=(?<y>\d+)\s+width=(?<width>\d+)\s+height=(?<height>\d+)\s+xoffset=(?<xoffset>\d+)\s+yoffset=(?<yoffset>\d+)\s+xadvance=(?<xadvance>\d+)\s+");
            string line = reader.ReadLine();
            int lineHeight = 0;
            int texWidth = 1;
            int texHeight = 1;

            while (line != null)
            {
                if (line.IndexOf("char id=") != -1)
                {
                    Match match = reg.Match(line);
                    if (match != Match.Empty)
                    {
                        var id = System.Convert.ToInt32(match.Groups["id"].Value);
                        var x = System.Convert.ToInt32(match.Groups["x"].Value);
                        var y = System.Convert.ToInt32(match.Groups["y"].Value);
                        var width = System.Convert.ToInt32(match.Groups["width"].Value);
                        var height = System.Convert.ToInt32(match.Groups["height"].Value);
                        var xoffset = System.Convert.ToInt32(match.Groups["xoffset"].Value);
                        var yoffset = System.Convert.ToInt32(match.Groups["yoffset"].Value);
                        var xadvance = System.Convert.ToInt32(match.Groups["xadvance"].Value);

                        CharacterInfo info = new CharacterInfo();
                        info.index = id;

                        Rect uv = new Rect();
                        uv.x = 1.0f * x / texWidth;
                        uv.y = 1.0f * (texHeight - height - y) / texHeight;
                        uv.width = 1.0f * width / texWidth;
                        uv.height = 1.0f * height / texHeight;
                        //Debug.Log(string.Format("id:{0}, uvx:{1}, uvy:{2}, uvw:{3}, uvh:{4}, x:{5}, y:{6}, width:{7}, height:{8}", id, uvx, uvy, uvw, uvh, x, y, width, height));

                        info.uvBottomLeft = new Vector2(uv.xMin, uv.yMin);
                        info.uvBottomRight = new Vector2(uv.xMax, uv.yMin);
                        info.uvTopLeft = new Vector2(uv.xMin, uv.yMax);
                        info.uvTopRight = new Vector2(uv.xMax, uv.yMax);

                        info.minX = xoffset;
                        info.maxX = xoffset + width;
                        info.minY = (yoffset - height) / 2;
                        info.maxY = (yoffset + height) / 2;
                        //info.glyphWidth = width;
                        //info.glyphHeight = height;
                        info.advance = xadvance;

                        charList.Add(info);
                    }
                }
                else if (line.IndexOf("scaleW=") != -1)
                {
                    Regex reg2 = new Regex(@"common lineHeight=(?<lineHeight>\d+)\s+.*scaleW=(?<scaleW>\d+)\s+scaleH=(?<scaleH>\d+)");
                    Match match = reg2.Match(line);
                    if (match != Match.Empty)
                    {
                        lineHeight = System.Convert.ToInt32(match.Groups["lineHeight"].Value);
                        texWidth = System.Convert.ToInt32(match.Groups["scaleW"].Value);
                        texHeight = System.Convert.ToInt32(match.Groups["scaleH"].Value);
                    }
                }
                line = reader.ReadLine();
            }


            Font customFont = AssetDatabase.LoadAssetAtPath<Font>(customFontPath);

            var list = charList.ToArray();
            Debug.Log(list.Length);
            customFont.characterInfo = list;



            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            fs.Close();
            Debug.Log(customFont);
        }
    }
}

