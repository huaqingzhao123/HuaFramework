/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/9/15 17:52:09
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using XMLib.DataHandlers;

namespace AliveCell
{
    /// <summary>
    /// PlayerInfo
    /// </summary>
    [System.Serializable]
    [DataContract(genericAllField = true)]
    public class PlayerInfo
    {
        public int id;
        public int prefabID;
        public string config;
        public int maxHp;
        public int attack;
        public int maxPower;
    }
}