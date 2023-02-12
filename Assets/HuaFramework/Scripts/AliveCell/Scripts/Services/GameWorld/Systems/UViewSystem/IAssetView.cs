/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/11/20 14:53:45
 */

using System;

using Single = FPPhysics.Fix64;

namespace AliveCell
{
    /// <summary>
    /// IAssetView
    /// </summary>
    public interface IAssetView : IResourceItem
    {
        UViewSystem system { get; set; }

        int objID { get; set; }

        string name { get; set; }

        void OnViewBind();

        void OnViewUnbind();

        void LogicUpdateView(Single deltaTime);

        void UpdateView(float deltaTime);
    }
}