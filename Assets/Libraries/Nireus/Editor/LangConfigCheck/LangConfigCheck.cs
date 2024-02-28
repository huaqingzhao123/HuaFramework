//using System;
//using UnityEngine;
//using UnityEditor;
//using System.Collections;
//using System.IO;
//using System.Collections.Generic;
//using System.Text.RegularExpressions;
//using Sirenix.Utilities;

//namespace Nireus.Editor
//{
//	public static class LangConfigCheck
//	{
//		[MenuItem("Nireus/CheckAllLangConfig",false, 1202)]
//		public static void CheckAllLangConfig()
//		{
//			foreach (LanguageType lang_type in Enum.GetValues(typeof(LanguageType)))
//			{
//				Debug.LogError("-----------------------start check lang config,languagetype = "+ lang_type.ToString() );
//				checkOneLangCsv(lang_type);
//				Debug.LogError("-----------------------end check lang config,languagetype = "+ lang_type.ToString() );
//				Debug.LogError("-------------------------------------------------------------------------------------");
//			}
//		}
//		static void checkOneLangCsv(LanguageType lang_type)
//		{
//			var _dic_lang = new Dictionary<string, string>();
//			var table = CSVService.getInstance().FetchRows(getCsvName(lang_type), null,true);

//			var row_count = table.getRowCount();
//			for (var i = 0; i < row_count; i++)
//			{
//				var row = table.getDataRow(i);

//				var key = row.getKeyString("name");
//				var value = row.getString("value");
//				_dic_lang[key] = Utils.FixWhiteSpaceLineBreak(value);
//				var temp_str = _dic_lang[key];
//				if (temp_str !="0" && (IsWholeNumber(temp_str) || IsWholeBD(temp_str)))
//				{
//					//纯数字无视
//					continue;
//				}
//				if (check_language(temp_str, lang_type) == false)
//				{
//					Debug.LogError("lang config error lang_type,key = "+ key );
//					Debug.LogError("lang config error lang_type,value = " + temp_str);
//					continue;
//				}
//				if (temp_str.Contains("<color"))
//				{
//					var k_v = getAttrs(temp_str);
//					var ret = GetTitleContent(temp_str,"color","");
//					if (ret.IsNullOrWhitespace() || temp_str.Contains(@"</color>") == false)
//					{
//						Debug.LogError("lang config error color,key = "+ key );
//						Debug.LogError("lang config error color,value = " + temp_str);
//					}
//				}
//			}
//		}

//		public static bool check_language(String s,LanguageType lang_type)
//		{
//			s = Utils.GetNoLineBreakSymbol(s);
//			foreach (LanguageType lang_type_d in Enum.GetValues(typeof(LanguageType)))
//			{
//				if (lang_type_d != lang_type)
//				{
//					switch (lang_type_d)
//					{
//						case LanguageType.CNS:
//							if (lang_type == LanguageType.CNT) break;
//							if(check_is_chinese(s)) return false;
//							break;
//						case LanguageType.CNT:
//							if (lang_type == LanguageType.CNS) break;
//							if(check_is_chinese(s)) return false;
//							break;
//						case LanguageType.KOR:
//							if(check_is_kor(s)) return false;
//							break;
//						case LanguageType.JP:
//							if(check_is_jp(s)) return false;
//							break;
//						case LanguageType.VN:
//							if(check_is_vt(s)) return false;
//							break;
//						case LanguageType.TH:
//							if(check_is_th(s)) return false;
//							break;
//					}
//				}
//			}

//			return true;
//		}
//		//判断内容里有没有中文-UTF8(包含繁体)
//		public static bool check_is_chinese(String s)
//		{
//			bool ret = false;
//			try
//			{
//				ret =  Regex.IsMatch(@"(?=\D)([\u3400-\u4db5])", s);
//				return ret;
//			}
//			catch(ArgumentException)
//			{
//				return false;
//			}
//		}
		
//		//判断内容里有没有韩文-UTF8
//		public static bool check_is_kor(String s)
//		{
//			try
//			{
//				return Regex.IsMatch(@"(?=\D)([\x3130-\x318F])", s) || Regex.IsMatch(@"(?=\D)([/xAC00-/xD7A3])", s);
//			}
//			catch (ArgumentException e)
//			{
//				return false;
//			}
//		}
		
//		//判断内容里有没有日文-UTF8
//		public static bool check_is_jp(String s)
//		{
//			try
//			{
//				return Regex.IsMatch(@"(?=\D)([\u0800-\u4e00])", s);
//			}
//			catch(ArgumentException)
//			{
//				return false;
//			}
//		}
		
//		//判断内容里有没有越南语-UTF8
//		public static bool check_is_vt(String s)
//		{
//			try
//			{
//				return Regex.IsMatch(@"\b\S*[AĂÂÁẮẤÀẰẦẢẲẨÃẴẪẠẶẬĐEÊÉẾÈỀẺỂẼỄẸỆIÍÌỈĨỊOÔƠÓỐỚÒỒỜỎỔỞÕỖỠỌỘỢUƯÚỨÙỪỦỬŨỮỤỰYÝỲỶỸỴAĂÂÁẮẤÀẰẦẢẲẨÃẴẪẠẶẬĐEÊÉẾÈỀẺỂẼỄẸỆIÍÌỈĨỊOÔƠÓỐỚÒỒỜỎỔỞÕỖỠỌỘỢUƯÚỨÙỪỦỬŨỮỤỰYÝỲỶỸỴAĂÂÁẮẤÀẰẦẢẲẨÃẴẪẠẶẬĐEÊÉẾÈỀẺỂẼỄẸỆIÍÌỈĨỊOÔƠÓỐỚÒỒỜỎỔỞÕỖỠỌỘỢUƯÚỨÙỪỦỬŨỮỤỰYÝỲỶỸỴAĂÂÁẮẤÀẰẦẢẲẨÃẴẪẠẶẬĐEÊÉẾÈỀẺỂẼỄẸỆIÍÌỈĨỊOÔƠÓỐỚÒỒỜỎỔỞÕỖỠỌỘỢUƯÚỨÙỪỦỬŨỮỤỰYÝỲỶỸỴAĂÂÁẮẤÀẰẦẢẲẨÃẴẪẠẶẬĐEÊÉẾÈỀẺỂẼỄẸỆIÍÌỈĨỊOÔƠÓỐỚÒỒỜỎỔỞÕỖỠỌỘỢUƯÚỨÙỪỦỬŨỮỤỰYÝỲỶỸỴAĂÂÁẮẤÀẰẦẢẲẨÃẴẪẠẶẬĐEÊÉẾÈỀẺỂẼỄẸỆIÍÌỈĨỊOÔƠÓỐỚÒỒỜỎỔỞÕỖỠỌỘỢUƯÚỨÙỪỦỬŨỮỤỰYÝỲỶỸỴA-Z]+\S*\b", s);

//			}
//			catch(ArgumentException)
//			{
//				return false;
//			}
//		}
		
//		//判断内容里有没有泰语-UTF8
//		public static bool check_is_th(String s)
//		{
//			try
//			{
//				return Regex.IsMatch(@"(?=\D)([\u0E00-\u0E7F])", s);
//			}
//			catch(ArgumentException)
//			{
//				return false;
//			}
//		}
		
//		/// <summary>
//		/// 获取字符中指定标签的值
//		/// </summary>
//		/// <param name="str">字符串</param>
//		/// <param name="title">标签</param>
//		/// <param name="attrib">属性名</param>
//		/// <returns>属性</returns>
//		public static string GetTitleContent(string str, string title, string attrib)
//		{
//			string tmpStr = string.Format("<{0}[^>]*?{1}=(['\"\"]?)(?<url>[^'\"\"\\s>]+)\\1[^>]*>", title, attrib); //获取<title>之间内容
//			Match TitleMatch = Regex.Match(str, tmpStr, RegexOptions.IgnoreCase);
//			string result = TitleMatch.Groups["url"].Value;
//			return result;
//		}
        
//		/// <summary>
//		/// 解析控件的属性返回键值对
//		/// </summary>
//		/// <param name="HtmlElement"></param>
//		/// <returns></returns>
//		public static System.Collections.Hashtable getAttrs(string HtmlElement)
//		{
//			System.Collections.Hashtable ht = new System.Collections.Hashtable();
//			MatchCollection mc = Regex.Matches(HtmlElement, "(?<name>[\\S^=]+)\\s*=\\s*\"(?<value>[^\"\"]+)\"|(?<name>[\\S^=]+)\\s*=\\s*'(?<value>[^'']+)'|(?<name>\\w+)=(?<value>[^\"])(?=[\\s])");
//			foreach (Match m in mc)
//			{
//				ht[m.Groups[1].Value] = m.Groups[2].Value;
//			}
//			return ht;
//		}
		
//		/// <summary>
//		/// 数字匹配
//		/// </summary>
//		/// <param name="strNumber"></param>
//		/// <returns></returns>
//		public static bool IsWholeNumber(string strNumber)
//		{
//			System.Text.RegularExpressions.Regex g = new System.Text.RegularExpressions.Regex(@"^[0-9]\d*$");
//			return g.IsMatch(strNumber);
//		}
		
//		/// <summary>
//		/// 标点符号匹配
//		/// </summary>
//		/// <param name="strNumber"></param>
//		/// <returns></returns>
//		public static bool IsWholeBD(string strNumber)
//		{
//			System.Text.RegularExpressions.Regex g = new System.Text.RegularExpressions.Regex(@"^[\,\.\?\!\']*$");
//			return g.IsMatch(strNumber);
//		}
		
//		private static string getCsvName(LanguageType lang_type)
//		{
//			switch (lang_type)
//			{
//				case LanguageType.CNS:
//					return PathConst.CFG_LANG_CNS;
//				case LanguageType.CNT:
//					return PathConst.CFG_LANG_CNT;
//				//case LanguageType.EN:
//				//    return PathConst.CFG_LANG_EN;
//				case LanguageType.KOR:
//					return PathConst.CFG_LANG_KOR;
//				case LanguageType.JP:
//					return PathConst.CFG_LANG_JP;
//				case LanguageType.VN:
//					return PathConst.CFG_LANG_VN;
//				case LanguageType.TH:
//					return PathConst.CFG_LANG_TH;
//			}

//			return PathConst.CFG_LANG_DEF;
//		}
//	}
//}