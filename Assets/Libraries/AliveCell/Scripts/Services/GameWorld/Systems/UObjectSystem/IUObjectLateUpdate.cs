/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/5/7 17:08:31
 */

using AliveCell.Commons;

namespace AliveCell
{
    /// <summary>
    /// IUObjectLateUpdate
    /// </summary>
    public interface IUObjectLateUpdate : ILateUpdate
    {
        bool isDestroyed { get; }
    }
}