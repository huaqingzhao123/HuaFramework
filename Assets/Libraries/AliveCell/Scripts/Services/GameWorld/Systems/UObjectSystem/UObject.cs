/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/10/28 11:02:09
 */

using System;

using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// UObject
    /// </summary>
    public class UObject : IPoolItem<Type>
    {
        public virtual int ID { get; set; }
        public virtual GameWorld world { get; set; }

        /// <summary>
        /// 调试模式
        /// </summary>
        public virtual bool isDebug { get; set; } = false;

        /// <summary>
        /// 不能手动修改
        /// </summary>
        public virtual bool isDestroyed { get; set; }

        public UObject()
        {
            OnReset();
        }

        #region 运算符重载

        public override int GetHashCode()
        {
            return ID;
        }

        public override bool Equals(object other)
        {
            UObject obj = other as UObject;
            if (obj == null && other != null && !(other is UObject))
            {
                return false;
            }
            return CompareBaseObjects(this, obj);
        }

        private static bool CompareBaseObjects(UObject lhs, UObject rhs)
        {
            bool flag = (object)lhs == null;
            bool flag2 = (object)rhs == null;
            if (flag2 && flag)
            {
                return true;
            }
            if (flag2)
            {
                return !IsNativeObjectAlive(lhs);
            }
            if (flag)
            {
                return !IsNativeObjectAlive(rhs);
            }
            return lhs.ID == rhs.ID;
        }

        private static bool IsNativeObjectAlive(UObject o)
        {
            if (o.isDestroyed || o.ID == UObjectSystem.noneID)
            {
                return false;
            }

            return true;
        }

        public static bool operator ==(UObject a, UObject b)
        {
            return CompareBaseObjects(a, b);
        }

        public static bool operator !=(UObject a, UObject b)
        {
            return !CompareBaseObjects(a, b);
        }

        #endregion 运算符重载

        protected void TriggerPropertyChanged<T>(PropertyType type, T oldValue, T newValue)
        {
            //SuperLog.Log($"TriggerPropertyChanged:ID:{ID}, {type}, {oldValue}=>{newValue}");
            App.Trigger(EventTypes.Game_PropertyChanged, ID, type, oldValue, newValue);
        }

        /// <summary>
        /// 销毁对象
        /// </summary>
        public void Destory()
        {
            world.uobj.Destroy(this);
        }

        public virtual void OnInitialized()
        {
        }

        public virtual void OnDestroyed()
        {
        }

        public virtual void OnReset()
        {
            isDebug = false;
            world = null;
            ID = UObjectSystem.noneID;
            isDestroyed = false;
        }

        public virtual string GetMessage()
        {
            string message = "----U----\n";
            message += $"ID:\t{ID}\n";
            return message;
        }

        public virtual void Log(string msg)
        {
            if (!isDebug)
            {
                return;
            }

            string str = $"<{world.frameIndex}>[{ID}]{GetType().Name}:{msg}";

            GameObject go = null;

#if UNITY_EDITOR
            if (this is IAssetObject target)
            { //绑定资源
                IAssetView view = world.uview.GetView(target.ID);
                if (view != null)
                {
                    go = view.gameObject;
                }
            }
#endif

            UnityEngine.Debug.Log(str, go);
        }

        #region pool

        public virtual Type poolTag => GetType();

        /// <summary>
        /// 不能手动修改
        /// </summary>
        public virtual bool inPool { get; set; }

        public virtual void OnPopPool()
        {
        }

        public virtual void OnPushPool()
        {
            OnReset();
        }

        #endregion pool
    }
}