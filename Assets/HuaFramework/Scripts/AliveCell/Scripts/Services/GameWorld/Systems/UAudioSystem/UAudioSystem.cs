/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/5/12 18:05:33
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using AliveCell.Commons;

using FPPhysics;
using Vector2 = FPPhysics.Vector2;
using Vector3 = FPPhysics.Vector3;
using Quaternion = FPPhysics.Quaternion;
using Matrix4x4 = FPPhysics.Matrix4x4;
using UVector2 = UnityEngine.Vector2;
using UVector3 = UnityEngine.Vector3;
using UQuaternion = UnityEngine.Quaternion;
using UMatrix4x4 = UnityEngine.Matrix4x4;
using Mathf = FPPhysics.FPUtility;
using Single = FPPhysics.Fix64;

namespace AliveCell
{
    public partial class GameWorld
    {
        public partial class Setting
        {
            [SerializeField]
            private UAudioSystem.Setting _audio = null;

            public UAudioSystem.Setting audio => _audio;
        }
    }

    /// <summary>
    /// UAudioSystem
    /// </summary>
    public class UAudioSystem : ISystem, ILogicUpdate, ICreate
    {
        [Serializable]
        public class Setting
        {
            public int normalBgm;
            public int combatBgm;
            public float combatHoldMinTime;

            [Range(0.0f, 1.0f)]
            public float playerSelfVolume = 1f;

            [Range(0.0f, 1.0f)]
            public float playerOtherVolume = 1f;

            [Range(0.0f, 1.0f)]
            public float enemyVolume = 1f;

            [Range(0.0f, 1.0f)]
            public float otherVolume = 1f;
        }

        public struct AudioData
        {
            public int id;
            public int audioId;
            public Vector3 position;
        }

        public readonly SuperLogHandler LogHandler;
        public readonly GameWorld world;

        protected readonly List<AudioData> audioBuffer = new List<AudioData>();
        public Setting setting => GlobalSetting.gameWorld.audio;

        private bool _isCombatBgm = false;
        private Single _combatPlayTime = 0;

        public UAudioSystem(GameWorld world)
        {
            this.world = world;
            this.LogHandler = world.LogHandler.CreateSub("UAudio");
        }

        public void Play(int id, int audioId, Vector3 position)
        {
            audioBuffer.Add(new AudioData()
            {
                id = id,
                audioId = audioId,
                position = position
            });
        }

        public void OnCreate()
        {
            world.audio.PlayBGM(setting.normalBgm);
        }

        public void OnLogicUpdate(Single deltaTime)
        {
            if (audioBuffer.Count > 0)
            {
                foreach (var audio in audioBuffer)
                {
                    Play(audio);
                }
                audioBuffer.Clear();
            }

            BGMUpdate(deltaTime);
        }

        private void Play(in AudioData data)
        {
            AudioBase ab = world.audio.Play(data.audioId, data.position);

            if (world.uplayer.IsPlayer(data.id))
            {
                if (world.ucamera.CheckFollow(data.id))
                {
                    ab.volume = setting.playerSelfVolume;
                }
                else
                {
                    ab.volume = setting.playerOtherVolume;
                }
            }
            else if (world.uenemy.IsEnemy(data.id))
            {
                ab.volume = setting.enemyVolume;
            }
            else
            {
                ab.volume = setting.otherVolume;
            }
        }

        private void BGMUpdate(Single deltaTime)
        {
            bool needChange = false;
            ActionMachineObject amObj = world.ucamera.followObj as ActionMachineObject;
            if (amObj == null)
            {
                if (_isCombatBgm)
                {
                    _isCombatBgm = false;
                    needChange = true;
                }
            }
            else
            {
                bool combatMode = amObj.CheckCombatMode();
                needChange = combatMode != _isCombatBgm;
                if (needChange)
                {
                    if (combatMode)
                    {
                        _isCombatBgm = combatMode;
                        _combatPlayTime = (Single)Time.time;
                    }
                    else
                    {
                        needChange = Time.time - _combatPlayTime > setting.combatHoldMinTime;
                        if (needChange)
                        {
                            _isCombatBgm = combatMode;
                        }
                    }
                }
            }

            if (needChange)
            {
                world.audio.PlayBGM(_isCombatBgm ? setting.combatBgm : setting.normalBgm);
            }
        }

        public void OnInitialize(List<UObject> preObjs)
        {
        }
    }
}