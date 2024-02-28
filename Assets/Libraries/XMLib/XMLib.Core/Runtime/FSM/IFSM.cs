
using System;

namespace XMLib.FSM
{
    /// <summary>
    /// IFSM
    /// </summary>
    public interface IFSM<T>
    {
        /// <summary>
        /// 当前状态
        /// </summary>
        Type currentState { get; }

        /// <summary>
        /// 上一个状态
        /// </summary>
        Type previousState { get; }

        /// <summary>
        /// 状态更新
        /// </summary>
        void Update(T target, float deltaTime);

        /// <summary>
        /// 切换状态
        /// </summary>
        void ChangeState(Type stateType);
    }
}