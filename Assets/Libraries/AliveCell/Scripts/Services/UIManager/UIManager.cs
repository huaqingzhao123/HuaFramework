/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/9/8 10:44:09
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// UIManager
    /// </summary>
    public class UIManager : IServiceInitialize, IDisposable, IMonoUpdate
    {
        public const int uiRootId = 50000001;
        [InjectObject] protected ResourceService res { get; set; }

        protected UIRoot uiRoot;

        protected List<UIPanel> panels = new List<UIPanel>();

        public bool isInited { get; private set; } = false;

        public Canvas canvas => uiRoot.canvas;

        public bool interactable
        {
            get => uiRoot.canvasGroup.interactable;
            set
            {
                interactableCount += (value ? 1 : -1);
                uiRoot.canvasGroup.interactable = interactableCount >= 0;
            }
        }

        public bool blocksRaycasts { get => uiRoot.canvasGroup.blocksRaycasts; set => uiRoot.canvasGroup.blocksRaycasts = value; }

        public SuperLogHandler LogHandler = SuperLogHandler.Create("UI");

        protected int interactableCount = 0;

        public void Dispose()
        {
            if (uiRoot != null)
            {
                foreach (var panel in uiRoot.uiPanels)
                {
                    panel.Dispose();
                }

                GameObject.Destroy(uiRoot.gameObject);
                uiRoot = null;
                panels.Clear();
            }
        }

        public void OnMonoUpdate()
        {
            foreach (var panel in panels)
            {
                if (panel.isRunning)
                {
                    panel.OnUpdate();
                }
            }
        }

        public IEnumerator OnServiceInitialize()
        {
            Input.multiTouchEnabled = true;

            GameObject uiRootObj = App.CreateGO(uiRootId);
            uiRootObj.name = "[AC]UI";
            GameObject.DontDestroyOnLoad(uiRootObj);
            uiRoot = uiRootObj.GetComponent<UIRoot>();
            foreach (var panel in uiRoot.uiPanels)
            {
                panel.Initialize(this);
            }
            isInited = true;
            yield break;
        }

        public T Get<T>() where T : UIPanel
        {
            return uiRoot.Get<T>();
        }

        public WaitUntil Show<T>() where T : UIPanel
        {
            T showPanel = Get<T>();
            if (panels.Contains(showPanel))
            {
                LogHandler.LogWarning($"Invalid call, {typeof(T)} Panel is Showing!");
                return new WaitUntil(() => true);
            }

            HashSet<UIPanel> changedPanels = new HashSet<UIPanel>();

            UIPanel oldTopPanel = null;
            if (panels.Count > 0)
            {
                oldTopPanel = panels[panels.Count - 1];
            }
            panels.Add(showPanel);
            SortPanel();
            UIPanel newTopPanel = panels[panels.Count - 1];

            if (oldTopPanel != null && oldTopPanel != newTopPanel)
            {
                oldTopPanel.ChangeState(UIPanelStatus.Pause);
                changedPanels.Add(oldTopPanel);
            }

            if (newTopPanel != null && oldTopPanel != newTopPanel && showPanel != newTopPanel)
            {
                newTopPanel.ChangeState(UIPanelStatus.Resume);
                changedPanels.Add(newTopPanel);
            }

            showPanel.ChangeState(UIPanelStatus.Enter);
            if (showPanel != newTopPanel)
            {
                showPanel.ChangeState(UIPanelStatus.Pause);
            }
            changedPanels.Add(showPanel);

            return CreateWaitUntil(changedPanels);
        }

        public WaitUntil Hide<T>() where T : UIPanel
        {
            T hidePanel = uiRoot.Get<T>();

            HashSet<UIPanel> changedPanels = new HashSet<UIPanel>();

            int hideIndex = panels.IndexOf(hidePanel);
            if (hideIndex < 0)
            {
                LogHandler.LogWarning($"{typeof(T)} 已经关闭");
                return new WaitUntil(() => true);
            }

            bool isTop = hideIndex == panels.Count - 1;
            UIPanel prevPanel = hideIndex > 0 ? panels[hideIndex - 1] : null;
            panels.RemoveAt(hideIndex);

            hidePanel.ChangeState(UIPanelStatus.Exit);
            changedPanels.Add(hidePanel);
            if (isTop && prevPanel != null)
            {
                prevPanel.ChangeState(UIPanelStatus.Resume);
                changedPanels.Add(prevPanel);
            }

            return CreateWaitUntil(changedPanels);
        }

        public WaitUntil HideAll(params Type[] withoutPanelType)
        {
            HashSet<UIPanel> changedPanels = new HashSet<UIPanel>();

            UIPanel topPanel = panels.Count > 0 ? panels[panels.Count - 1] : null;
            for (int i = 0; i < panels.Count; i++)
            {
                UIPanel panel = panels[i];
                if (withoutPanelType.Length > 0 && Array.Exists(withoutPanelType, t => t == panel.GetType()))
                {
                    continue;
                }

                panels.RemoveAt(i);
                i--;

                panel.ChangeState(UIPanelStatus.Exit);
                changedPanels.Add(panel);
            }

            UIPanel newTopPanel = panels.Count > 0 ? panels[panels.Count - 1] : null;
            if (newTopPanel != null && newTopPanel != topPanel)
            {//原本不是顶级UI，则需要唤醒
                newTopPanel.ChangeState(UIPanelStatus.Resume);
                changedPanels.Add(newTopPanel);
            }

            return CreateWaitUntil(changedPanels);
        }

        protected WaitUntil CreateWaitUntil(IEnumerable<UIPanel> panels)
        {
            return new WaitUntil(() =>
            {
                foreach (var panel in panels)
                {
                    if (!panel.isAllAnimCompleted)
                    {
                        return false;
                    }
                }
                return true;
            });
        }

        protected void SortPanel()
        {
            panels.Sort((t1, t2) => t1.sortWeight.CompareTo(t2.sortWeight));
            for (int i = 0; i < panels.Count; i++)
            {
                panels[i].transform.SetSiblingIndex(i);
            }
        }
    }
}