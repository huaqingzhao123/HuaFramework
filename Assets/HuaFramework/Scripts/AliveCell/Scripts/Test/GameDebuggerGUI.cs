/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/11/5 1:23:43
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using XMLib;
using System.Linq;

namespace AliveCell
{
    /// <summary>
    /// GameDebuggerGUI
    /// </summary>
    public class GameDebuggerGUI : MonoBehaviour
    {
        public bool enable = false;

        [Header("Style")]
        [Range(0.1f, 1)]
        public float uiScale = 1f;

        #region Click

        [Header("Click")]
        public float clickWaitTime = 3f;

        public int clickMaxCnt = 2;

        private float clickTimer = 0f;
        private int clickCnt = 0;

        #endregion Click

        #region Net

        [Header("Net")]
        public bool enablePing = false;


        #endregion Net

        #region FPS

        [Header("FPS")]
        public bool enableFPS = false;

        private float deltaTime = 0.0f;
        private float msec => deltaTime * 1000.0f;
        private float fps => 1.0f / deltaTime;
        private string fpsInfo => string.Format("{0:0.0} ms ({1:0.} fps)", msec, Mathf.Round(fps));

        #endregion FPS

        #region Log

        [Header("Log")]
        public bool enableLog = false;

        public int logMaxSize = 4096;

        [Range(0f, 1f)]
        public float logClearScale = 0.2f;

        private StringBuilder logMsg = new StringBuilder(4096);

        #endregion Log

        private int selectIndex = 0;
        private GUILayoutOption[] openButtonOptions = { GUILayout.Width(50), GUILayout.Height(50) };

        private Matrix4x4 matrix = default;
        private Rect areaRt = default;
        private const string Key = "GDGUIScale";
        private GUIStyle textStyle = default;
        private Vector2 gwObjsPos = default;
        private Vector2 gwInfoPos = default;
        private Vector2 logVIewPos = default;
        private TObject selectObj = default;
        private string[] scaleLevelText = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        private static GameDebuggerGUI _inst;

        #region Test

        private string[] resolutionNames = new string[] { "480p", "720p", "1080p" };
        private int[] resolutions = new int[] { 480, 720, 1080 };
        private int resolutionIndex = 0;
        private string[] frameRateNames = new string[] { "15fps", "30fps", "45fps", "60fps", "120fps" };
        private int[] frameRates = new int[] { 15, 30, 45, 60, 120 };
        private int frameRateIndex = 0;

        #endregion Test

        protected void Awake()
        {
            if (_inst != null)
            {
                SuperLog.LogWarning("GameDebuggerGUI 已存在");
                Destroy(this);
                return;
            }

            _inst = this;
            DontDestroyOnLoad(gameObject);
        }

        protected void Start()
        {
            if (PlayerPrefs.HasKey(Key))
            {
                uiScale = PlayerPrefs.GetFloat(Key);
            }

            textStyle = new GUIStyle();
            textStyle.alignment = TextAnchor.UpperLeft;
            textStyle.normal.textColor = Color.white;
            textStyle.fontSize = 10;
            UnityEngine.Application.logMessageReceived += OnLogMessageReceived;

            UpdateMatrix();
        }

        protected void OnDestroy()
        {
            UnityEngine.Application.logMessageReceived -= OnLogMessageReceived;
            PlayerPrefs.SetFloat(Key, uiScale);
        }

        protected void OnValidate()
        {
            PlayerPrefs.SetFloat(Key, uiScale);
        }

        protected void Update()
        {
            if (enableFPS)
            {
                deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            }
        }

        protected void OnGUI()
        {
            GUI.matrix = matrix;

            EnableButton();

            if (!enable)
            {
                using (var area = new GUILayout.AreaScope(areaRt, GUIContent.none))
                {
                    using (var hor = new GUILayout.HorizontalScope())
                    {
                        AlwaysDrawTools();
                    }
                }
            }
            else
            {
                using (var area = new GUILayout.AreaScope(areaRt, GUIContent.none))
                {
                    using (var hor = new GUILayout.HorizontalScope())
                    {
                        DrawTools();
                    }
                }
            }
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            lock (logMsg)
            {
                logMsg.Append(">");
                logMsg.AppendLine(condition);

                int clearSize = (int)(logMaxSize * logClearScale);
                if (logMsg.Length - logMaxSize > clearSize)
                {//限制最大日志字数, 移除超过的部分
                    for (int i = clearSize - 1; i < logMsg.Length; i++)
                    {
                        if (logMsg[i] == '\n')
                        {
                            logMsg.Remove(0, i + 1);
                            break;
                        }
                    }
                }
            }
        }

        private void UpdateMatrix()
        {
            float w = Screen.width * uiScale;
            float h = Screen.height * uiScale;
            float halfScale = uiScale / 2f;
            float matrixScale = 1.0f / uiScale;

            Rect safeArea = Screen.safeArea;

            float left = safeArea.xMin * -1 * halfScale;
            float right = (Screen.width - safeArea.xMax) * -1 * halfScale;
            float top = (Screen.height - safeArea.yMax) * -1 * halfScale;
            float bottom = safeArea.yMin * -1 * halfScale;
            RectOffset offset = new RectOffset((int)left, (int)right, (int)top, (int)bottom);

            matrix = Matrix4x4.Scale(new Vector3(matrixScale, matrixScale, 1));
            areaRt = new Rect(0, 0, w, h);
            areaRt = offset.Add(areaRt);
        }

        private void EnableButton()
        {
            if (!enable && GUILayout.Button(GUIContent.none, GUI.skin.label, openButtonOptions))
            {
                if (clickCnt > 0 && Time.unscaledTime - clickTimer < clickWaitTime || clickCnt <= 0)
                {
                    clickCnt++;
                    clickTimer = Time.unscaledTime;
                }

                if (clickCnt >= clickMaxCnt)
                {
                    clickMaxCnt = 1;//第一次启动后，之后都只用点一次
                    enable = true;
                }
            }

            if (enable || !enable && Time.unscaledTime - clickTimer > clickWaitTime)
            {
                clickCnt = 0;
                clickTimer = 0f;
            }
        }

        private void AlwaysDrawTools()
        {
            GUILayout.BeginVertical();

            if (enableFPS || enablePing)
            {
                StringBuilder builder = new StringBuilder();

                if (enableFPS)
                {
                    builder.Append(fpsInfo);
                }

                if (enablePing)
                {
                    builder.Append("    ");
                }

                GUILayout.Label(builder.ToString(), textStyle);
            }

            if (enableLog)
            {
                lock (logMsg)
                {
                    GUIContent logMsgContent = new GUIContent(logMsg.ToString());
                    using (var scroll = new GUILayout.ScrollViewScope(logVIewPos, false, false))
                    {
                        logVIewPos = scroll.scrollPosition;

                        GUILayout.Label(logMsgContent, textStyle);
                        GUILayout.FlexibleSpace();
                    }

                    logVIewPos = Vector2.up * textStyle.CalcSize(logMsgContent).y;
                }
            }

            GUILayout.EndVertical();
        }

        private void DrawTools()
        {
            if (!enable)
            {
                return;
            }

            using (var hor = new GUILayout.HorizontalScope())
            {
                using (var ver = new GUILayout.VerticalScope(GUI.skin.box, GUILayout.Width(100)))
                {
                    if (GUILayout.Button("关闭工具"))
                    {
                        enable = false;
                    }

                    selectIndex = GUILayout.SelectionGrid(selectIndex, new string[] { "公共", "游戏世界", "日志", "测试" }, 1);
                }

                switch (selectIndex)
                {
                    case 0:
                        DrawCommonView();
                        break;

                    case 1:
                        DrawGameWorldView();
                        break;

                    case 2:
                        DrawLogView();
                        break;

                    case 3:
                        DrawTestView();
                        break;
                }
            }
        }

        private void DrawTestView()
        {
            if (!App.isInited)
            {
                GUILayout.Label("App 未初始化");
                return;
            }
            using (var ver = new GUILayout.VerticalScope(GUI.skin.box))
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("游戏胜利"))
                {
                    App.game.CompletedGame(GameState.Successed);
                }
                if (GUILayout.Button("游戏失败"))
                {
                    App.game.CompletedGame(GameState.Failed);
                }

                GUILayout.EndHorizontal();

                int index = -1;

                GUILayout.Label("Haptic");
                string[] hapticNames = GlobalSetting.device.hapticInfos.Select(t => t.name).ToArray();
                index = GUILayout.SelectionGrid(index, hapticNames, 5);
                if (index >= 0)
                {
                    App.device.Haptic(hapticNames[index]);
                }

                index = -1;
                GUILayout.Label("Shake");
                string[] shakeNames = GlobalSetting.camera.shakeInfos.Select(t => t.name).ToArray();
                index = GUILayout.SelectionGrid(index, shakeNames, 5);
                if (index >= 0)
                {
                    App.camera.Shake(shakeNames[index]);
                }

                resolutionIndex = GUILayout.SelectionGrid(resolutionIndex, resolutionNames, 5);
                frameRateIndex = GUILayout.SelectionGrid(frameRateIndex, frameRateNames, 5);
                if (GUILayout.Button("应用分辨率与帧率"))
                {
                    App.device.SetResolution(resolutions[resolutionIndex], frameRates[frameRateIndex]);
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("关闭Terrain"))
                {
                    Terrain t = GameObject.FindObjectOfType<Terrain>();
                    if (t != null)
                    {
                        t.enabled = false;
                    }
                }
                if (GUILayout.Button("打开Terrain"))
                {
                    Terrain t = GameObject.FindObjectOfType<Terrain>();
                    if (t != null)
                    {
                        t.enabled = true;
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("关闭Terrain树"))
                {
                    Terrain t = GameObject.FindObjectOfType<Terrain>();
                    if (t != null)
                    {
                        t.drawTreesAndFoliage = false;
                    }
                }
                if (GUILayout.Button("打开Terrain树"))
                {
                    Terrain t = GameObject.FindObjectOfType<Terrain>();
                    if (t != null)
                    {
                        t.drawTreesAndFoliage = true;
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawLogView()
        {
            using (var ver = new GUILayout.VerticalScope(GUI.skin.box))
            {
                lock (logMsg)
                {
                    using (var scroll = new GUILayout.ScrollViewScope(logVIewPos, false, true))
                    {
                        logVIewPos = scroll.scrollPosition;

                        GUILayout.Label(logMsg.ToString(), textStyle);
                        GUILayout.FlexibleSpace();
                    }
                }
            }
        }

        private void DrawCommonView()
        {
            using (var ver = new GUILayout.VerticalScope(GUI.skin.box))
            {
                using (var ver1 = new GUILayout.VerticalScope())
                {
                    GUILayout.Label($"UI 缩放 x{uiScale}");

                    int select = GUILayout.SelectionGrid(-1, scaleLevelText, 4);
                    if (select != -1)
                    {
                        uiScale = (select + 1) * (1.0f / scaleLevelText.Length);
                        UpdateMatrix();
                        PlayerPrefs.SetFloat(Key, uiScale);
                    }
                }

                using (var ver1 = new GUILayout.VerticalScope())
                {
                    using (var ver2 = new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label($"时间缩放 x{Time.timeScale}");
                        if (GUILayout.Button("重置", GUILayout.Width(50f)))
                        {
                            Time.timeScale = 1f;
                        }
                    }

                    Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, 0f, 5f);
                }

                using (var ver1 = new GUILayout.VerticalScope())
                {
                    GUILayout.Label($"帧率限制 x{UnityEngine.Application.targetFrameRate}");
                    UnityEngine.Application.targetFrameRate = (int)GUILayout.HorizontalSlider((int)UnityEngine.Application.targetFrameRate, 15, 120);
                }

                enableFPS = GUILayout.Toggle(enableFPS, "显示FPS");
                enablePing = GUILayout.Toggle(enablePing, "显示Ping");
                enableLog = GUILayout.Toggle(enableLog, "显示日志");

                Color color = textStyle.normal.textColor;
                GUILayout.Label($"日志显示Alpha x{color.a}");
                color.a = GUILayout.HorizontalSlider(color.a, 0f, 1f);
                textStyle.normal.textColor = color;

                GUILayout.Label($"日志字体大小 x{ textStyle.fontSize}");
                textStyle.fontSize = (int)GUILayout.HorizontalSlider(textStyle.fontSize, 1, 20);

                if (GUILayout.Button("输出录像列表"))
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine("录像列表:");
                    //for (int i = 0; i < App.record.recordDatas.Count; i++)
                    //{
                    //    builder.AppendFormat("{0}:", i);
                    //    builder.AppendLine(App.record.recordDatas[i].ToString());
                    //}
                    SuperLog.Log(builder.ToString());
                }
            }
        }

        private void DrawGameWorldView()
        {
            if (!App.isInited || App.game == null)
            {
                GUILayout.Label("游戏世界未创建");
                return;
            }

            GameWorld world = App.game;

            using (var ver = new GUILayout.VerticalScope(GUI.skin.box, GUILayout.Width(150)))
            {
                using (var scroll = new GUILayout.ScrollViewScope(gwObjsPos, false, true))
                {
                    gwObjsPos = scroll.scrollPosition;
                    foreach (var item in world.uobj.Foreach<TObject>())
                    {
                        if (GUILayout.Button($"[{item.ID}]{item.GetType().Name}".Substring(0, 8)))
                        {
                            selectObj = item;
                        }
                    }

                    GUILayout.FlexibleSpace();
                }
            }

            if (selectObj != null)
            {
                using (var ver = new GUILayout.VerticalScope(GUI.skin.box, GUILayout.MinWidth(400)))
                {
                    using (var scroll = new GUILayout.ScrollViewScope(gwInfoPos, true, true))
                    {
                        gwInfoPos = scroll.scrollPosition;
                        GUILayout.Label(selectObj.GetMessage());
                        GUILayout.FlexibleSpace();
                    }
                }
            }
        }
    }
}