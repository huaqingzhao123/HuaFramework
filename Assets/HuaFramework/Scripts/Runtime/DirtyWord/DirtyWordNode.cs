using System;
using System.Collections.Generic;

namespace HuaFramework
{
    internal class DirtyWordNode
    {
        private readonly Dictionary<char, DirtyWordNode> _node;

        private byte _is_end;

        public DirtyWordNode() { _node = new Dictionary<char, DirtyWordNode>(); }

        public void Add(string word)
        {
            if(word.Length <= 0)
            {
                return;
            }

            var key = word[0];
            if(!_node.TryGetValue(key, out var sub_node))
            {
                sub_node = new DirtyWordNode();
                _node.Add(key, sub_node);
            }

            if(word.Length > 1)
            {
                sub_node.Add(word[1..]);
            }
            else
            {
                sub_node._is_end = 1;
            }
        }

        public int CheckAndGetEndIndex(ReadOnlySpan<char> source_dbc_text, int cursor, Func<char, bool> check_special_sym)
        {
            //检测下位字符如果不是汉字 数字 字符 偏移量加1  
            for(int i = cursor; i < source_dbc_text.Length; i++)
            {
                if(check_special_sym != null && check_special_sym(source_dbc_text[i]))
                {
                    cursor++;
                }
                else
                {
                    break;
                }
            }

            if(cursor >= source_dbc_text.Length)
            {
                return-1;
            }

            var key = source_dbc_text[cursor];

            _node.TryGetValue(key, out var group);

            if(group is null)
            {
                return-1;
            }

            return group._is_end == 1
                ? cursor
                : group.CheckAndGetEndIndex(source_dbc_text, cursor + 1, check_special_sym);
        }
    }
}
