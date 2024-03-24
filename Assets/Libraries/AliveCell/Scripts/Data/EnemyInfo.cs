/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/9/15 15:44:26
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using XMLib.DataHandlers;

namespace AliveCell
{
    /// <summary>
    /// EnemyConfig
    /// </summary>
    [System.Serializable]
    [DataContract(genericAllField = true)]
    public class EnemyInfo
    {
        public int id;
        public int prefabID;
        public string config;
        public int maxHp;
        public int attack;
    }
}