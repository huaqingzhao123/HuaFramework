/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/9/8 10:44:29
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using System.Linq;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace AliveCell
{
    /// <summary>
    /// UIRoot
    /// </summary>
    public class UIRoot : ResourceItem
    {
        [SerializeField]
        protected CanvasGroup _canvasGroup;

        [SerializeField]
        protected Canvas _canvas;

        [SerializeField]
        protected List<UIPanel> _uiPanels;

        public List<UIPanel> uiPanels => _uiPanels;
        public CanvasGroup canvasGroup => _canvasGroup;
        public Canvas canvas => _canvas;

        protected Dictionary<string, UIPanel> nameDict = new Dictionary<string, UIPanel>();
        protected Dictionary<Type, UIPanel> typeDict = new Dictionary<Type, UIPanel>();

        protected override void Awake()
        {
            base.Awake();
            foreach (var panel in uiPanels)
            {
                nameDict.Add(panel.panelName, panel);
                typeDict.Add(panel.GetType(), panel);
            }
        }

        public T Get<T>() where T : UIPanel
        {
            return typeDict.TryGetValue(typeof(T), out UIPanel panel) ? (T)panel : null;
        }

        public UIPanel Get(string name)
        {
            return nameDict.TryGetValue(name, out UIPanel panel) ? panel : null;
        }

#if UNITY_EDITOR

        [CustomEditor(typeof(UIRoot))]
        public class UIRootEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                UIRoot uiRoot = target as UIRoot;

                using (var v = new GUILayout.VerticalScope(GUI.skin.box))
                {
                    GUILayout.Label("工具");
                    if (GUILayout.Button("初始化", GUILayout.Height(30)))
                    {
                        InitRoot(uiRoot);
                    }

                    DrawEditList(uiRoot);
                }
            }

            private void DrawEditList(UIRoot uiRoot)
            {
                if (uiRoot.uiPanels.Count > 0)
                {
                    using (var v = new GUILayout.VerticalScope(GUI.skin.box))
                    {
                        GUILayout.Label("编辑面板");
                        string[] names = uiRoot.uiPanels.Select(t => t.GetType().Name).ToArray();
                        int index = GUILayout.SelectionGrid(-1, names, 2);
                        if (index >= 0)
                        {
                            for (int i = 0; i < uiRoot.uiPanels.Count; i++)
                            {
                                UIPanel panel = uiRoot.uiPanels[i];
                                SetPanelActive(panel, i == index);
                                if (i == index)
                                {
                                    //Selection.activeObject = panel.gameObject;
                                }
                            }
                        }
                    }
                }
            }

            private void SetPanelActive(UIPanel panel, bool isActive)
            {
                panel.canvasGroup.alpha = isActive ? 1f : 0f;
                panel.canvasGroup.blocksRaycasts = isActive;
                panel.canvasGroup.interactable = isActive;
            }

            private void InitRoot(UIRoot uiRoot)
            {
                uiRoot._uiPanels.Clear();
                uiRoot.GetComponentsInChildren(uiRoot._uiPanels);
                foreach (var panel in uiRoot._uiPanels)
                {
                    SetPanelActive(panel, false);
                }

                if (uiRoot._canvasGroup == null)
                {
                    uiRoot._canvasGroup = uiRoot.GetComponentInChildren<CanvasGroup>();
                }
                if (uiRoot._canvas == null)
                {
                    uiRoot._canvas = uiRoot.GetComponentInChildren<Canvas>();
                }

                EditorUtility.SetDirty(uiRoot);
            }
        }

#endif
    }
}