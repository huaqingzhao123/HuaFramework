/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/10/28 11:20:49
 */

using AliveCell.Commons;

namespace AliveCell
{
    /// <summary>
    /// IUObjectUpdate
    /// </summary>
    public interface IUObjectUpdate : IUpdate
    {
        bool isDestroyed { get; }
    }
}