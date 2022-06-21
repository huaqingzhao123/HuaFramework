using HuaFramework.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HuaFramework.Managers
{

    public enum UILayer
    {
        Bottom = 1,
        Middle,
        Top
    }
    public class GUIManager : MonoSingleton<GUIManager>
    {
        private Dictionary<string, GameObject> _uiPanelCaches = new Dictionary<string, GameObject>();
        private RectTransform _bottom;
        private RectTransform _middle;
        private RectTransform _top;
        private RectTransform _uiRoot;
        public RectTransform UIRoot
        {
            get
            {
                if (!_uiRoot)
                {
                    _uiRoot = Instantiate(Resources.Load<GameObject>("UIRoot")).transform as RectTransform;
                    _uiRoot.name = "UIRoot";
                }
                return _uiRoot;
            }
        }
        void Start()
        {
            _bottom = UIRoot.Find("Bottom") as RectTransform;
            _middle = UIRoot.Find("Middle") as RectTransform;
            _top = UIRoot.Find("Top") as RectTransform;
            var panel = LoadUIPanel("BeginPanel");
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.anchoredPosition = Vector2.zero;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            SetResoultion(new Vector2(1920, 1080), 0.5f);
            _uiRoot.anchorMin = Vector2.zero;
            _uiRoot.anchorMax = Vector2.one;
            _uiRoot.anchoredPosition = Vector2.zero;
        }


        /// <summary>
        /// 设置画布的标准分辨率
        /// </summary>
        /// <param name="resoultion"></param>
        public void SetResoultion(Vector2 resoultion, float matchWidthOrHeight)
        {
            var canvasScaler = _uiRoot.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = resoultion;
            canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
        }

        /// <summary>
        /// 加载UIPanel
        /// </summary>
        /// <param name="panelName"></param>
        /// <param name="uILayer"></param>
        public RectTransform LoadUIPanel(string panelName, UILayer uILayer = UILayer.Middle)
        {
            if (_uiPanelCaches.ContainsKey(panelName))
            {
                //初始化
                return _uiPanelCaches[panelName].transform as RectTransform;
            }
            else
            {
                var panel = Instantiate(Resources.Load<GameObject>(panelName));
                panel.name = panelName;
                //初始化
                _uiPanelCaches.Add(panelName, panel);
                switch (uILayer)
                {
                    case UILayer.Bottom:
                        panel.transform.SetParent(_bottom);
                        break;
                    case UILayer.Middle:
                        panel.transform.SetParent(_middle);
                        break;
                    case UILayer.Top:
                        panel.transform.SetParent(_top);
                        break;
                    default:
                        break;
                }
                return panel.transform as RectTransform;
            }
        }

        /// <summary>
        /// 卸载UIPanel
        /// </summary>
        public void UnLoadPanel(string panelName)
        {
            if (_uiPanelCaches.TryGetValue(panelName, out GameObject panel))
                Destroy(panel);
        }
    }

}
