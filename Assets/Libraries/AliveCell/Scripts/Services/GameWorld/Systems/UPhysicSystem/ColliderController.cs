/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/6/19 14:13:07
 */

using System.Collections.Generic;
using UnityEngine;
using XMLib.AM;
using XMLib.AM.Ranges;

namespace AliveCell
{
    /// <summary>
    /// ColliderController
    /// </summary>
    public class ColliderController : ResourceItem
    {
        public GameObject bodyRoot;
        public GameObject volumeRoot;

        private List<BoxCollider> boxColliders = new List<BoxCollider>();
        private List<SphereCollider> sphereColliders = new List<SphereCollider>();
        public CapsuleCollider volumeCollider { get; private set; }

        public MovementObject target { get; protected set; }

        public bool bodyActive => bodyRoot != null ? bodyRoot.activeSelf : false;
        public bool volumeActive => volumeCollider != null ? volumeRoot.activeSelf : false;

        protected int beginBodyLayer;
        protected int beginVolumeLayer;

        protected override void Awake()
        {
            base.Awake();
            volumeCollider = volumeRoot.GetComponent<CapsuleCollider>();
        }

        public override void OnPushPool()
        {
            base.OnPushPool();

            target = null;
            beginBodyLayer = 0;
            beginVolumeLayer = 0;
            bodyRoot.SetActive(true);
            volumeRoot.SetActive(true);
        }

        public void BeginIgnore()
        {
            beginBodyLayer = bodyRoot.layer;
            beginVolumeLayer = volumeRoot.layer;

            bodyRoot.layer = Physics.IgnoreRaycastLayer;
            volumeRoot.layer = Physics.IgnoreRaycastLayer;
        }

        public void EndIgnore()
        {
            bodyRoot.layer = beginBodyLayer;
            volumeRoot.layer = beginVolumeLayer;
        }

        public void SetBodyActive(bool active)
        {
            if (bodyRoot != null)
            {
                bodyRoot.SetActive(active);
            }
        }

        public void SetVolumeActive(bool active)
        {
            if (volumeRoot != null)
            {
                volumeRoot.SetActive(active);
            }
        }

        public void Initialization(MovementObject target)
        {
            this.target = target;

            bodyRoot.SetActive(true);
            volumeRoot.SetActive(true);

            foreach (var item in boxColliders)
            {
                item.enabled = false;
            }
            foreach (var item in sphereColliders)
            {
                item.enabled = false;
            }

            UpdateCollider();
        }

        public bool IsSelf(Collider col)
        {
            if (col.transform.parent != null && gameObject == col.transform.parent.gameObject)
            {
                return true;
            }

            return false;
        }

        public void UpdateCollider()
        {
            if (target != null)
            {
                UpdateCollider(target);
            }
        }

        protected void UpdateCollider(MovementObject obj)
        {
            transform.position = obj.position;//new Vector3(obj.position.x, Mathf.Clamp(obj.position.y, 0, 1), obj.position.z); ;//限制上升高度
            bodyRoot.layer = obj.layer;
            bodyRoot.transform.rotation = obj.rotation;

            //设置移动检测用
            volumeCollider.center = obj.center;
            volumeCollider.height = obj.height;
            volumeCollider.radius = obj.radius;

            //TODO 等待修改为定点
            /*
            if (obj is ActionMachineObject am)
            {
                //设置身体
                FrameConfig frameConfig = am.machine.GetStateFrame();
                if (frameConfig == null)
                {
                    frameConfig = am.machine.GetStateConfig().frames[0];
                }
                UpdateBody(frameConfig);
            }
            */
        }

        protected void UpdateBody(FrameConfig frameConfig)
        {
            if (frameConfig.stayBodyRange)
            {
                return;
            }

            int boxCnt = 0;
            int sphereCnt = 0;
            for (int i = 0; i < frameConfig.bodyRanges.Count; i++)
            {
                RangeConfig config = frameConfig.bodyRanges[i];

                switch (config.value)
                {
                    case BoxItem v:
                        boxCnt++;
                        break;

                    case SphereItem v:
                        sphereCnt++;
                        break;
                }
            }

            FixedCollision(boxColliders, boxCnt, bodyRoot);
            FixedCollision(sphereColliders, sphereCnt, bodyRoot);

            for (int i = 0; i < frameConfig.bodyRanges.Count; i++)
            {
                RangeConfig config = frameConfig.bodyRanges[i];

                switch (config.value)
                {
                    case BoxItem v:
                        boxCnt--;
                        BoxCollider box = boxColliders[boxCnt];
                        box.center = v.offset;
                        box.size = v.size;
                        box.enabled = true;
                        break;

                    case SphereItem v:
                        sphereCnt--;
                        SphereCollider circle = sphereColliders[sphereCnt];
                        circle.center = v.offset;
                        circle.radius = v.radius;
                        circle.enabled = true;
                        break;
                }
            }
        }

        protected void FixedCollision<T>(List<T> list, int cnt, GameObject target) where T : Collider
        {
            if (cnt > list.Count)
            {//缺少补充
                while (cnt > list.Count)
                {
                    T collision = target.AddComponent<T>();
                    list.Add(collision);
                }
            }
            else
            {//多余的禁用
                for (int i = cnt; i < list.Count; i++)
                {
                    list[i].enabled = false;
                }
            }
        }
    }
}