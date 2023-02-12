/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/9/15 6:46:31
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// GlobalSetting
    /// </summary>
    [CreateAssetMenu(menuName = "AliveCell/GlobalSetting")]
    [System.Serializable]
    public partial class GlobalSetting : ScriptableObject
    {
        protected static GlobalSetting _Inst;
        public static GlobalSetting Inst => _Inst != null ? _Inst : _Inst = Resources.Load<GlobalSetting>("GlobalSetting");
    }
}