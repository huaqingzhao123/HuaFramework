using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RedDotManager
{
    private static RedDotManager _instance;
    public static RedDotManager getInstance()
    {
        if(_instance == null)
        {
            _instance = new RedDotManager();
        }
        return _instance;
    }

    //private Dictionary<RedDotType, bool> _cache = new Dictionary<RedDotType, bool>();
    private bool _checkShow(RedDot red_dot)
    {
        bool need_show = false;
    
        //if(_cache.TryGetValue(red_dot.type, out need_show))
        //{
        //    return need_show;
        //}
    
        foreach (var observe_type in red_dot.observeTypeList)
        {
            if (_getActiveCount(observe_type) > 0)
            {
                need_show = true;
                break;
            }
        }
        //_cache[red_dot.type] = need_show;
        return need_show;
    }

    private int _getActiveCount(RedDotType type)
    {
        List<RedDot> list = null;
        if (_type_red_dot_list__dic.TryGetValue(type, out list) == false)
        {
            return 0;
        }
        if(list == null)
        {
            return 0;
        }
        int count = 0;

        foreach(var node in list)
        {
            if(node.gameObject.activeSelf == true)
            {
                count++;
            }
        }
        return count;
    }

    public void postEvent(RedDotType type, bool value)
    {
        //首先，所有类型为Type的RedDot要亮起
        List<RedDot> list = null;
        if (_type_red_dot_list__dic.TryGetValue(type, out list)  && list != null)
        {
            foreach (var red_dot in list)
            {
                red_dot.show(value, false); //不post，理由稍后介绍↓
            }
        }

        //如果类型为目标Type的RedDot一个都没有（有可能尚未Awake所以还没向RedDotManager regist），此时会导致↓
        //那些Obeserve了此Type的其他高层RedDot无法亮起，因为没有任意一个低层被Obeserve的RedDot亮起
        //所以还要主动让那些Obeserve了此Type的其他高层RedDot亮起（因为主动点亮它们，所以上面就无需再postEvent了）
        List<RedDot> observe_red_dot_list = null;
        if (_ob_dic.TryGetValue(type, out observe_red_dot_list) && observe_red_dot_list != null)
        {
            foreach (var observe_red_dot in observe_red_dot_list)
            {
                observe_red_dot.show(value, true);
            }
        }
    }

    //某个Type下有哪些结点
    private Dictionary<RedDotType, List<RedDot>> _type_red_dot_list__dic = new Dictionary<RedDotType, List<RedDot>>();

    public void postEvent(RedDot red_dot, bool value)
    {
        if(red_dot == null)
        {
            return;
        }

        //通知那些关注该RedDotType的RedDot根据计数更新各自的active状态
        List<RedDot> observe_red_dot_list = null;
        if (_ob_dic.TryGetValue(red_dot.type, out observe_red_dot_list) == false)
        {
            return;
        }
        foreach(var observe_red_dot in observe_red_dot_list)
        {
            _updateActive(observe_red_dot);
        }
    }

    private void _updateActive(RedDot red_dot)
    {
        red_dot.show(_checkShow(red_dot));
    }

    //某个Type有哪些RedDot关心？
    private Dictionary<RedDotType, List<RedDot>> _ob_dic = new Dictionary<RedDotType, List<RedDot>>();

    private Dictionary<RedDot, bool> _dic = new Dictionary<RedDot, bool>();

    public void regist(RedDot red_dot)
    {
        //防止重复regist
        if(_dic.ContainsKey(red_dot))
        {
            return;
        }
        _dic[red_dot] = true;

        //更新计数
        List<RedDot> red_dot_list = null;
        if(_type_red_dot_list__dic.TryGetValue(red_dot.type, out red_dot_list) == false || red_dot_list == null)
        {
            _type_red_dot_list__dic[red_dot.type] = red_dot_list = new List<RedDot>();
        }
        if(red_dot_list.Contains(red_dot) == false)
        {
            red_dot_list.Add(red_dot);
        }
        

        foreach (var type in red_dot.observeTypeList)
        {
            List<RedDot> list = null;

            if (_ob_dic.TryGetValue(type, out list) == false)
            {
                list = new List<RedDot>();

                _ob_dic[type] = list;
            }
            list.Add(red_dot);
        }

        //根据自己关注的Type，设定自身的active状态
        //设置自身状态的时候也会递归的导致关注自身类型的其他RedDot的状态发生变化
        //所以注意不要产生环形依赖！
        _updateActive(red_dot);
    }

    public void unregist(RedDot red_dot)
    {
        if (_dic.ContainsKey(red_dot) == false)
        {
            return;
        }
        _dic.Remove(red_dot);

        //更新计数
        List<RedDot> red_dot_list = null;
        if (_type_red_dot_list__dic.TryGetValue(red_dot.type, out red_dot_list) == true || red_dot_list != null)
        {
            red_dot_list.Remove(red_dot);
        }

        foreach (var type in red_dot.observeTypeList)
        {
            List<RedDot> list = null;

            if (_ob_dic.TryGetValue(type, out list))
            {
                list.Remove(red_dot);
            }
        }

        //更新引用技术，更新依赖自己状态的其他RedDot的状态
        postEvent(red_dot, false);
    }
}


