using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nireus
{
	public class CSVService
	{
		static private CSVService _instance;
		static public CSVService getInstance() { return _instance == null ? _instance = new CSVService() : _instance; }
        private StringBuilder str_value = new StringBuilder();
        private char[] _separator1 = {'\n'};
        private char[] _separator2 = {'\r',' ','\t'};
        public DataTable FetchRows(string file_name, string asset_bundle_name = "config",bool is_editor= false)
        {
            if (string.IsNullOrEmpty(asset_bundle_name) == false)
            {
                file_name = asset_bundle_name + "/" + file_name;
            }

            TextAsset ta = null;
            if (!is_editor)
            {
				GameDebug.LogWarning($"FetchRows name start = {file_name}");
	            ta = AssetManager.Instance.loadSync<TextAsset>(PathConst.BUNDLE_RES_CFG + file_name + ".csv");
                GameDebug.LogWarning($"FetchRows name end = {file_name}");
			}
			else
            {
#if UNITY_EDITOR
                ta = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(PathConst.BUNDLE_RES_CFG + file_name + ".csv");
#endif
            }

			string csv_data = ta == null ? string.Empty : ta.text;
            //string []lines = csv_data.Split(new char[]{'\n'}, StringSplitOptions.RemoveEmptyEntries);
            string[] lines = csv_data.Split(_separator1, StringSplitOptions.RemoveEmptyEntries);
            DataTable table = new DataTable();
            if (lines.Length < 1) return table;
            string field_str = lines[0].Trim(_separator2);
            //string field_str = lines[0].Trim(new char[]{'\r',' ', '\t'});
            table.setFileds(parseLine(field_str));

			for (int i = 1; i < lines.Length; ++i)
			{
				string value_str = lines[i].Trim(_separator2);
                //string value_str = lines[i].Trim(new char[]{'\r',' ', '\t'});
                DataRow row = new DataRow();
				row.setValues(parseLine(value_str));
				table.insertRow(row);
			}

			return table;
		}

		enum PaseLineState
		{
			STATE__NORMAL,		// 普通字符串;
			STATE__QUOTA,		// 进入双引号;
		};

		private List<string> parseLine(string line)
		{
			List<string> ret = new List<string>();
			if (line.Length == 0) return ret;

			char comma = ',';
			char quota = '"';
			PaseLineState state = PaseLineState.STATE__NORMAL;
			int pos = 0;
            str_value.Clear();
            do
            {
                char chr = line[pos];

                switch (state)
                {
                    case PaseLineState.STATE__NORMAL:
                        {
                            if (chr == quota)
                            {
                                state = PaseLineState.STATE__QUOTA;
                            }
                            else if (chr == comma)
                            {
                                ret.Add(str_value.ToString().Trim(' '));
                                str_value.Clear();
                            }
                            else
                            {
                                if (chr == '\t') continue;
                                str_value.Append(chr);
                            }
                        }
                        break;

                    case PaseLineState.STATE__QUOTA:
                        {
                            if (chr == quota)
                            {
                                state = PaseLineState.STATE__NORMAL;
                            }
                            else
                            {
                                str_value.Append(chr);
                            }
                        }
                        break;
                    default:
                        break;
                }
            } while (++pos < line.Length);

            if (state == PaseLineState.STATE__QUOTA)
            {
                GameDebug.Log("csv line format error");
            }
            ret.Add(str_value.ToString().Trim(' '));


            return ret;
		}
	}
}
