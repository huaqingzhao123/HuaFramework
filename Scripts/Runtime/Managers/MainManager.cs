using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.Managers
{
    public enum GameState
    {
        //开发中（编辑器）
        Developing,
        //测试
        Test,
        //发布
        Release,
    }
    /// <summary>
    /// 框架入口,游戏中的入口Manager继承此脚本
    /// </summary>
    public abstract class MainManager : MonoBehaviour
    {
        public GameState GameState;
        private static bool _isInitGameState;
        private static GameState _gameState;
        void Start()
        {
            if (!_isInitGameState)
                _gameState = GameState;
            switch (_gameState)
            {
                case GameState.Developing:
                    LauncherInDeveloping();
                    break;
                case GameState.Test:
                    LauncherInTest();
                    break;
                case GameState.Release:
                    LauncherInRelease();
                    break;
            }
        }
        /// <summary>
        /// 编辑器逻辑
        /// </summary>
        protected abstract void LauncherInDeveloping();
        /// <summary>
        /// 测试包逻辑
        /// </summary>
        protected abstract void LauncherInTest();
        /// <summary>
        /// 正式包逻辑
        /// </summary>
        protected abstract void LauncherInRelease();
    }
}

