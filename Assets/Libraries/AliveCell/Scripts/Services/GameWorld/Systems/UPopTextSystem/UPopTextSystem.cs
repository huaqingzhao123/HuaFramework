/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2021/1/13 16:40:42
 */

using AliveCell.Commons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    public partial class GameWorld
    {
        public partial class Setting
        {
            [SerializeField]
            private PopTextStyleSetting _popTextStyle = null;

            public PopTextStyleSetting popText => _popTextStyle;
        }
    }

    /// <summary>
    /// UPopTextSystem
    /// </summary>
    public class UPopTextSystem : ISystem, ICreate, IDestroy, ILateUpdate
    {
        public readonly SuperLogHandler LogHandler;
        public readonly GameWorld world;
        public PopTextStyleSetting setting => GlobalSetting.gameWorld.popText;

        protected Transform root;
        protected LinkedList<PopText> popTexts = new LinkedList<PopText>();
        protected List<PopTextSetting> createTexts = new List<PopTextSetting>(32);

        public UPopTextSystem(GameWorld world)
        {
            this.world = world;
            this.LogHandler = world.LogHandler.CreateSub("UPT");
        }

        public PopText Create(string name, string text, Vector3 position, float value = 0)
        {
            return Create(new PopTextSetting(setting.GetStyle(name), position, text, value, value));
        }

        public PopText Create(PopTextSetting setting)
        {
            PopText target = App.CreateGO(10000005).GetComponent<PopText>();
            popTexts.AddLast(target);
            target.transform.SetParent(root);
            target.Init(setting);
            return target;
        }

        public void DestroyAll()
        {
            foreach (var target in popTexts)
            {
                App.DestroyGO(target);
            }
            popTexts.Clear();
        }

        public void OnInitialize(List<UObject> preObjs)
        {
        }

        public void OnCreate()
        {
            var obj = new GameObject("[AC]PopText");
            GameObject.DontDestroyOnLoad(obj);
            root = obj.transform;

            App.On<InjuredResult, InjuredInfo>(EventTypes.Game_Injured, OnGameInjured)
                .SetFilter((args) =>
                {
                    int id = ((InjuredResult)args[0]).id;
                    InjuredInfo info = (InjuredInfo)args[1];

                    if (world.uplayer.IsPlayer(info.info.id) && world.ucamera.CheckFollow(info.info.id))
                    {
                        return true;
                    }
                    return false;
                });
        }

        public void OnDestroy()
        {
            App.Off(this);
            DestroyAll();
            if (root != null)
            {
                GameObject.Destroy(root.gameObject);
                root = null;
            }
        }

        public void OnLateUpdate(float deltaTime)
        {
            var nextNode = popTexts.First;
            while (nextNode != null)
            {
                var currentNode = nextNode;
                nextNode = currentNode.Next;

                currentNode.Value.OnUpdate(deltaTime);
                if (currentNode.Value.isDie)
                {
                    App.DestroyGO(currentNode.Value);
                    popTexts.Remove(currentNode);
                }
            }
        }

        private void OnGameInjured(InjuredResult result, InjuredInfo info)
        {
            TObject obj = world.uobj.Get<TObject>(result.id);
            if (obj == null)
            {
                return;
            }

            float value = Mathf.Clamp01(result.attackScale / (float)result.maxAttackScale);

            Create("style01", result.loseHp.ToString(), obj.position + Vector3.up * 1f, value);
        }
    }
}