/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/9/11 14:18:03
 */

namespace AliveCell
{
    /// <summary>
    /// EventType
    /// </summary>
    public enum EventTypes
    {
        None = 0,

        #region UI =====================================

        /// <summary>
        /// UI 游戏开始
        /// </summary>
        UI_StartGame = 1,

        /// <summary>
        /// UI 游戏结束
        /// </summary>
        UI_EndGame = 2,

        /// <summary>
        /// UI 游戏匹配
        /// </summary>
        UI_StartMatch = 3,

        /// <summary>
        /// UI 取消匹配
        /// </summary>
        UI_CancelMatch = 4,

        /// <summary>
        /// 回放
        /// </summary>
        UI_ReplayGame = 5,

        /// <summary>
        /// 回放列表
        /// </summary>
        UI_ReplayGameList = 6,

        /// <summary>
        /// 切换关卡
        /// <para>关卡ID<see cref="int"/></para>
        /// </summary>
        UI_ChangeLevel = 7,

        /// <summary>
        /// 选择关卡
        /// </summary>
        UI_LevelList = 8,

        /// <summary>
        /// 选择关卡返回
        /// </summary>
        UI_LevelListBack = 9,

        /// <summary>
        /// 回放选择的游戏
        /// <para>录像数据<see cref="RecordData"/></para>
        /// </summary>
        UI_ReplaySelectGame = 10,

        /// <summary>
        /// 游戏菜单按钮
        /// </summary>
        UI_GameMenu = 11,

        /// <summary>
        /// 唤醒游戏
        /// </summary>
        UI_ResumeGame = 12,

        /// <summary>
        /// 游戏暂停播放
        /// <para>开始/暂停 <see cref="bool"/></para>
        /// </summary>
        UI_PlayPause = 13,

        /// <summary>
        /// 添加游戏TimeScale
        /// <para>添加的timescale <see cref="float"/></para>
        /// </summary>
        UI_AddTimeScale = 14,

        /// <summary>
        /// 玩家头像被点击
        /// <para>玩家id <see cref="int"/></para>
        /// </summary>

        UI_PlayerHeader = 15,

        #endregion UI =====================================

        #region Game ==================================================

        /// <summary>
        /// 游戏开始
        /// </summary>
        Game_Start = 10001,

        /// <summary>
        /// 游戏结束
        /// </summary>
        Game_End = 10002,

        /// <summary>
        /// 游戏完成
        /// </summary>
        Game_Complete = 10003,

        /// <summary>
        /// 受伤 - void(攻击结果:InjuredResult,受伤信息:InjuredInfo)
        /// </summary>
        Game_Injured = 11001,

        /// <summary>
        /// 攻击目标发生变化 - void(对象ID:int, 旧目标ID:int, 新目标ID:int)
        /// </summary>
        Game_AttackTargetChanged = 11002,

        /// <summary>
        /// 死亡 - void(死亡对象ID:int)
        /// </summary>
        Game_Dead = 11003,

        /// <summary>
        /// follow目标改变  - void(死亡对象:TObject)
        /// </summary>
        Game_FollowTargetChanged = 11004,

        /// <summary>
        /// 属性发生改变-void(对象ID:int, 属性类型:PropertyType, 旧值:object, 新值:object)
        /// </summary>
        Game_PropertyChanged = 11005,

        #endregion Game ==================================================

        #region Room ==================================================

        /// <summary>
        /// 房间更新
        /// </summary>
        Room_Update = 20001,

        /// <summary>
        /// 新玩家加入
        /// <para>新玩家ID <see cref="string"/></para>
        /// </summary>
        Room_JoinRoom = 20002,

        /// <summary>
        /// 玩家离开
        /// <para>离开玩家ID <see cref="string"/></para>
        /// </summary>
        Room_LeaveRoom = 20003,

        /// <summary>
        /// 玩家自定义状态发生变化
        /// <para>变化玩家ID <see cref="string"/>, 当前状态 <see cref="ulong"/></para>
        /// </summary>
        Room_ChangeCustomPlayerStatus = 20004,

        /// <summary>
        /// 玩家网络状态发生变化
        /// <para>变化玩家ID <see cref="string"/>, 当前状态 <see cref="Lagame.NetworkState"/></para>
        /// </summary>
        Room_ChangePlayerNetworkState = 20005,

        /// <summary>
        /// 房间信息变化
        /// </summary>
        Room_ChangeRoom = 20006,

        /// <summary>
        /// 房间销毁
        /// </summary>
        Room_DismissRoom = 20007,

        /// <summary>
        /// 玩家移除
        /// <para>移除玩家ID <see cref="string"/></para>
        /// </summary>
        Room_RemovePlayer = 20008,

        /// <summary>
        /// 接收到玩家消息
        /// <para>房间ID <see cref="string"/>,发送玩家ID <see cref="string"/>,消息 <see cref="string"/></para>
        /// </summary>
        Room_RecvFromClient = 20009,

        /// <summary>
        /// 开始帧同步
        /// </summary>
        Room_StartFrameSync = 20010,

        /// <summary>
        /// 停止帧同步
        /// </summary>
        Room_StopFrameSync = 20011,

        /// <summary>
        /// 帧消息
        /// <para>帧 <see cref="Dictionary[string,InputData]"/></para>
        /// </summary>
        Room_RecvFrame = 20012,

        /// <summary>
        /// 匹配被取消
        /// </summary>
        Room_CancelMatch = 20013,

        /// <summary>
        /// 匹配完成
        /// <para>错误代码 <see cref="int"/>,房间信息 <see cref="Lagame.RoomInfo"/></para>
        /// </summary>
        Room_Match = 20014,

        #endregion Room ==================================================

        #region World

        /// <summary>
        /// 场景加载
        /// <para>场景 <see cref="ISubScene"/></para>
        /// </summary>
        Scene_Initialize = 30001,

        /// <summary>
        /// 场景卸载
        /// <para>场景 <see cref="ISubScene"/></para>
        /// </summary>
        Scene_UnInitialize = 30002,

        #endregion World
    }
}