/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2021/1/2 13:56:04
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// SceneDisplayController
    /// </summary>
    public class SceneDisplayController : System.IDisposable
    {
        private ActionMachineView _player;

        public float GetAnimPlayTime()
        {
            return _player == null ? 0f : _player.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        }

        public void UpdatePlayer(Vector3 pos, Quaternion rotation)
        {
            CreatePlayer();
            _player.transform.position = pos;
            _player.transform.rotation = rotation;
        }

        public void CreatePlayer()
        {
            if (_player == null)
            {
                GameObject target = App.CreateGO(20000001);
                _player = target.GetComponent<ActionMachineView>();
                _player.animator.enabled = true;
                _player.animator.Play("Idle_Weapon");
            }
        }

        public void DeletePlayer()
        {
            if (_player != null)
            {
                App.DestroyGO(_player);
                _player = null;
            }
        }

        public void Dispose()
        {
            DeletePlayer();
        }
    }
}