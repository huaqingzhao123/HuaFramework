

namespace XMLib.FSM
{
    /// <summary>
    /// IFSMState
    /// </summary>
    public interface IFSMState<T>
    {
        /// <summary>
        /// 进入
        /// </summary>
        void Enter(T target);

        /// <summary>
        /// 退出
        /// </summary>
        void Exit(T target);

        /// <summary>
        /// 更新
        /// </summary>
        void Update(T target, float deltaTime);
    }
}