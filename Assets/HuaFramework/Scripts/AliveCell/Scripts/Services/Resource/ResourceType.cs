/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/10/12 16:54:07
 */

namespace AliveCell
{
    /// <summary>
    /// ResourceType
    /// </summary>
    public enum ResourceType
    {
        All = 0b1111_1111,
        Prefab = 0b0001,
        Material = 0b0010,
        Sprite = 0b0100,
        Text = 0b1000,
    }
}