using HuaFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HuaFramework
{
    public static class DirtyWordHelper
    {
        private static readonly Dictionary<LanguageType, DirtyWordNode> _LANG_NODES = new();

        private static readonly DirtyWordNode _ALL = new();

        private static LanguageType _CURRENT;

        static DirtyWordHelper()
        {
            _CURRENT = Lang.getLangType();
        }

        public static void ChangeCurrentLanguage()
        {
            _CURRENT = Lang.getLangType();
        }

        private static string getDirtyWordCsvName(LanguageType language_type)
        {
            switch (language_type)
            {
                case LanguageType.CN:
                {
                    return PathConst.CFG_DIRTY_CN;
                }
                case LanguageType.TC:
                {
                    return PathConst.CFG_DIRTY_TC;
                }
                case LanguageType.DE:
                {
                    return PathConst.CFG_DIRTY_DE;
                }
                case LanguageType.EN:
                {
                    return PathConst.CFG_DIRTY_EN;
                }
                case LanguageType.KOR:
                {
                    return PathConst.CFG_DIRTY_KOR;
                }
                case LanguageType.JP:
                {
                    return PathConst.CFG_DIRTY_JP;
                }
                case LanguageType.VN:
                {
                    return PathConst.CFG_DIRTY_VN;
                }
                case LanguageType.TH:
                {
                    return PathConst.CFG_DIRTY_TH;
                }
                case LanguageType.FR:
                {
                    return PathConst.CFG_DIRTY_FR;
                }
                case LanguageType.ID:
                {
                    return PathConst.CFG_DIRTY_ID;
                }
                case LanguageType.RU:
                {
                    return PathConst.CFG_DIRTY_RU;
                }
                case LanguageType.SP:
                {
                    return PathConst.CFG_DIRTY_SP;
                }
                case LanguageType.PT:
                {
                    return PathConst.CFG_DIRTY_PT;
                }
            }
            return PathConst.CFG_DIRTY_DEF;
        }

        public static void Initialize()
        {
            _CURRENT = Lang.getLangType();
            _LANG_NODES.Clear();
            
            foreach (var language_type in (LanguageType[])Enum.GetValues(typeof(LanguageType)))
            {
                _LANG_NODES.TryAdd(language_type, new DirtyWordNode());
                var table = CSVService.getInstance().FetchRows(getDirtyWordCsvName(language_type), null);

                var row_count = table.getRowCount();
                for (var i = 0; i < row_count-1; ++i)
                {
                    var row = table.getDataRow(i);
                    var dirty_word = row.getString("content");

                    var dbc_word = _ToDBC(dirty_word);

                    if (dbc_word.Length <= 0)
                    {
                        continue;
                    }

                    var str = dbc_word.ToString();
                    _LANG_NODES[language_type].Add(str);
                    _ALL.Add(str);
                }
            }
        }

        public static bool IsDirtyInAll(string source_text, int filter_num = 0)
        {
            _ThorwIfNotInit();

            return _ALL._IsDirty(source_text, filter_num);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDirty(string source_text, int filter_num = 0)
        {
            _ThorwIfNotInit();

            return IsDirty(_CURRENT, source_text, filter_num);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDirty(LanguageType language, string source_text, int filter_num = 0)
        {
            _ThorwIfNotInit();
            _LANG_NODES.TryGetValue(language, out var node);

            return node?._IsDirty(source_text, filter_num) ?? IsDirtyInAll(source_text, filter_num);
        }

        private static bool _IsDirty(this DirtyWordNode node, string source_text, int filter_num = 0)
        {
            if(string.IsNullOrEmpty(source_text))
            {
                return false;
            }

            var source_dbc_text = _ToDBC(source_text);
            for(int i = 0; i < source_dbc_text.Length; i++)
            {
                int bad_word_len;
                if(filter_num > 0 && _IsNum(source_dbc_text[i]))
                {
                    bad_word_len = _CheckNumberSeq(source_dbc_text, i, filter_num);
                    if(bad_word_len > 0)
                    {
                        return true;
                    }
                }

                //查询以该字为首字符的词组  
                bad_word_len = node._Check(source_dbc_text, i);
                if(bad_word_len > 0)
                {
                    return true;
                }
            }
            return false;
        }


        public static string ReplaceInAll(string source_text,
                                          char   replace_char,
                                          int    filter_num  = 0,
                                          string num_replace = null)
        {
            _ThorwIfNotInit();

            foreach(var node in _LANG_NODES.Values)
            {
                var result = node._Replace(
                    source_text,
                    replace_char,
                    filter_num,
                    num_replace
                );

                if(!string.Equals(result, source_text))
                {
                    return result;
                }
            }

            return source_text;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Replace(string source_text,
                                     char   replace_char,
                                     int    filter_num  = 0,
                                     string num_replace = null)
        {
            _ThorwIfNotInit();
            return _LANG_NODES[_CURRENT]?.
                   _Replace(
                       source_text,
                       replace_char,
                       filter_num,
                       num_replace
                   ) ??
                   source_text;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Replace(LanguageType language,
                                     string         source_text,
                                     char           replace_char,
                                     int            filter_num  = 0,
                                     string         num_replace = null)
        {
            _ThorwIfNotInit();
            return _LANG_NODES[language]?.
                   _Replace(
                       source_text,
                       replace_char,
                       filter_num,
                       num_replace
                   ) ??
                   source_text;
        }

        private static string _Replace(this DirtyWordNode node,
                                       string             source_text,
                                       char               replace_char,
                                       int                filter_num  = 0,
                                       string             num_replace = null)
        {
            if(string.IsNullOrEmpty(source_text))
            {
                return string.Empty;
            }

            var source_dbc_text = _ToDBC(source_text);

            char[] temp_string = source_text.ToCharArray();

            using var replace_list = ListComponent<DirtyWordRNode>.Create();

            for(int i = 0; i < source_dbc_text.Length; i++)
            {
                int bad_word_len;
                if(filter_num > 0 && _IsNum(source_dbc_text[i]))
                {
                    bad_word_len = _CheckNumberSeq(source_dbc_text, i, filter_num);
                    if(bad_word_len > 0)
                    {
                        bad_word_len += 1;
                        if(num_replace == null)
                        {
                            for(int pos = 0; pos < bad_word_len; pos++)
                            {
                                temp_string[pos + i] = replace_char;
                            }
                        }
                        else
                        {
                            replace_list.Add(
                                new DirtyWordRNode {start = i, len = bad_word_len, type = DirtyWordType.IndexReplace}
                            );
                        }

                        i = i + bad_word_len - 1;
                        continue;
                    }
                }

                //查询以该字为首字符的词组  
                bad_word_len = node._Check(source_dbc_text, i);
                if(bad_word_len > 0)
                {
                    for(int pos = 0; pos < bad_word_len; pos++)
                    {
                        temp_string[pos + i] = replace_char;
                    }

                    i = i + bad_word_len - 1;
                }
            }

            string result;
            if(replace_list.Count > 0)
            {
                result = _ReplaceString(
                    temp_string,
                    replace_list,
                    null,
                    num_replace
                );
            }
            else
            {
                result = new string(temp_string);
            }

            return result;
        }

        public static string ReplaceInAll(string source_text,
                                          string replace_str,
                                          int    filter_num  = 0,
                                          string num_replace = null)
        {
            _ThorwIfNotInit();

            foreach(var node in _LANG_NODES.Values)
            {
                var result = node._Replace(
                    source_text,
                    replace_str,
                    filter_num,
                    num_replace
                );

                if(!string.Equals(result, source_text))
                {
                    return result;
                }
            }

            return source_text;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Replace(string source_text,
                                     string replace_str,
                                     int    filter_num  = 0,
                                     string num_replace = null)
        {
            _ThorwIfNotInit();

            return _LANG_NODES[_CURRENT]?.
                   _Replace(
                       source_text,
                       replace_str,
                       filter_num,
                       num_replace
                   ) ??
                   source_text;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Replace(LanguageType language,
                                     string         source_text,
                                     string         replace_str,
                                     int            filter_num  = 0,
                                     string         num_replace = null)
        {
            _ThorwIfNotInit();

            return _LANG_NODES[language]?.
                   _Replace(
                       source_text,
                       replace_str,
                       filter_num,
                       num_replace
                   ) ??
                   source_text;
        }

        private static string _Replace(this DirtyWordNode node,
                                       string             source_text,
                                       string             replace_str,
                                       int                filter_num  = 0,
                                       string             num_replace = null)
        {
            if(string.IsNullOrEmpty(source_text))
            {
                return string.Empty;
            }

            var source_dbc_text = _ToDBC(source_text);

            using var replace_list = ListComponent<DirtyWordRNode>.Create();

            if(filter_num > 0 && num_replace == null)
            {
                num_replace = replace_str;
            }

            for(int i = 0; i < source_dbc_text.Length; i++)
            {
                int bad_word_len;
                if(filter_num > 0 && _IsNum(source_dbc_text[i]))
                {
                    bad_word_len = _CheckNumberSeq(source_dbc_text, i, filter_num);
                    if(bad_word_len > 0)
                    {
                        bad_word_len += 1;
                        int start = i;
                        replace_list.Add(
                            new DirtyWordRNode {start = start, len = bad_word_len, type = DirtyWordType.IndexReplace}
                        );
                        i = i + bad_word_len - 1;
                        continue;
                    }
                }

                bad_word_len = node._Check(source_dbc_text, i);

                if(bad_word_len <= 0)
                {
                    continue;
                }

                replace_list.Add(new DirtyWordRNode {start = i, len = bad_word_len, type = DirtyWordType.StrReplace});

                i = i + bad_word_len - 1;
            }

            string temp_str = _ReplaceString(
                source_text.ToCharArray(),
                replace_list,
                replace_str,
                num_replace
            );

            return temp_str;
        }

        private static string _ReplaceString(IEnumerable<char>             char_array,
                                             ListComponent<DirtyWordRNode> nodes,
                                             string                        replace_str,
                                             string                        num_replace)
        {
            num_replace ??= replace_str;

            replace_str ??= num_replace;

            using var char_list = ListComponent<char>.Create();
            char_list.AddRange(char_array);

            int offset = 0;
            for(int i = 0, i_max = nodes.Count; i < i_max; i++)
            {
                int    start     = nodes[i].start + offset;
                int    len       = nodes[i].len;
                string str       = nodes[i].type == 0 ? replace_str : num_replace;
                int    end_index = start + len - 1;

                if(str.Length < len)
                {
                    char_list.RemoveRange(start, len - str.Length);
                }

                for(int j = 0, j_max = str.Length; j < j_max; j++)
                {
                    char ch    = str[j];
                    int  index = start + j;


                    if(index <= end_index)
                    {
                        char_list[index] = ch;
                    }
                    else
                    {
                        char_list.Insert(index, ch);
                    }
                }

                offset += str.Length - len;
            }

            return new string(char_list.ToArray());
        }

        private static int _Check(this DirtyWordNode node, ReadOnlySpan<char> source_text, int cursor)
        {
            int endsor      = node.CheckAndGetEndIndex(source_text, cursor, _CheckSpecialSym);
            int word_length = endsor >= cursor ? endsor - cursor + 1 : 0;
            return word_length;
        }

        private static int _CheckNumberSeq(ReadOnlySpan<char> source_text, int cursor, int filter_num)
        {
            int count  = 0;
            int offset = 0;
            if(cursor + 1 >= source_text.Length)
            {
                return 0;
            }

            //检测下位字符如果不是汉字 数字 字符 偏移量加1  
            for(int i = cursor + 1; i < source_text.Length; i++)
            {
                if(!_IsNum(source_text[i]))
                {
                    break;
                }

                count++;
            }

            if(count + 1 >= filter_num)
            {
                int word_length = count + offset;
                return word_length;
            }

            return 0;
        }

        #region helper

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("DEBUG")]
        private static void _ThorwIfNotInit()
        {
            if(_LANG_NODES is null || _LANG_NODES.Count <= 0)
            {
                throw new Exception("Init first!");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool _CheckSpecialSym(char character)
        {
            return!_IsValidI18NChar(character) && !_IsNum(character) && !_IsAlphabet(character);
        }

        /// <summary>
        /// 是否为各国字符
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool _IsValidI18NChar(char character)
        {
            int char_val = character;

            // 参考链接 https://unicode-table.com/cn/alphabets/
            return
                // 中文
                char_val is>= 0x4ee0 and<= 0x9fa5
                    // 日文
                    or>= 0x0800 and<= 0x4e00
                    // 韩文
                    or>= 0xac00 and<= 0xd7ff
                    // 泰语 & 越南
                    or>= 0x0e00 and<= 0x0e4f
                    // 俄语
                    or>= 0x0410 and<= 0x044f;
        }

        /// <summary>
        /// 是否为数字
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool _IsNum(char character)
        {
            int char_val = character;
            return char_val is>= 48 and<= 57;
        }

        /// <summary>
        /// 是否为英文字母
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool _IsAlphabet(char character)
        {
            int char_val = character;
            return char_val is>= 97 and<= 122 or>= 65 and<= 90;
        }


        /// <summary>
        /// 转半角小写的函数(DBC case)
        /// </summary>
        /// <param name="input">任意字符串</param>
        /// <returns>半角字符串</returns>
        ///<remarks>
        ///全角空格为12288，半角空格为32
        ///其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        ///</remarks>
        private static ReadOnlySpan<char> _ToDBC(string input) { return _ToDBC(input.AsSpan()); }

        private static ReadOnlySpan<char> _ToDBC(char[] c) { return _ToDBC(c.AsSpan()); }

        private static ReadOnlySpan<char> _ToDBC(ReadOnlySpan<char> input)
        {
            Span<char> c = new char[input.Length];
            input.CopyTo(c);

            for(int i = 0; i < c.Length; i++)
            {
                if(c[i] == 12288)
                {
                    c[i] = (char) 32;
                    continue;
                }

                if(c[i] > 65280 && c[i] < 65375)
                {
                    c[i] = (char) (c[i] - 65248);
                    continue;
                }

                if(char.IsUpper(c[i]))
                {
                    c[i] = char.ToLowerInvariant(c[i]);
                }
            }

            return c;
        }

        #endregion
    }
}
