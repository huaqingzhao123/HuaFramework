/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/10/23 15:38:59
 */

using System.Collections.Generic;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// ISystem
    /// </summary>
    public interface ISystem
    {
        void OnInitialize(List<UObject> preObjs);
    }
}