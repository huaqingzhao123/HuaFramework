using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Nireus;
using AliveCell;

public delegate void CallBack();
public delegate void CallBack<T>(T t);
public class DevelopReplayManager 
{
    const string c_directoryName     = "DevelopReplay";
    public const string c_eventExpandName = "inputEvent";
    public const string c_randomExpandName = "random";
    public const string c_randomVector2ExpandName = "randomVector2";

    const string c_recordName    = "DevelopReplay";
    const string c_qucikLunchKey = "qucikLunch";

    const string c_eventStreamKey = "EventStream";
    const string c_randomListKey = "RandomList";

    const string c_eventNameKey = "e";
    const string c_serializeInfoKey = "s";

    private static bool s_isReplay = false;
    public static string ReplayPath="";  // 缓存游戏刚开始的缓存路径
    public static bool IsReplay
    {
        get
        {
            return s_isReplay;
        }
    }


    public static bool s_isProfile = true;

    //static List<Dictionary<string, string>> s_eventStreamSerialize;
    static List<IInputEventBase> s_eventStream;

    private static Dictionary<int, List<float>> s_randomDic;
    static Dictionary<int,List<Vector2>> s_randomVector2Dic;
    static List<int> s_randomList;
    static List<Vector2> s_randomVector2List;

    private static Action s_onLunchCallBack;

    private static StreamWriter m_EventWriter = null;
    private static StreamWriter m_RandomWriter = null;
    private static StreamWriter m_RandomVectorWriter = null;
    public static CallBack s_ProfileGUICallBack;

    public static Action OnLunchCallBack
    {
        get { return s_onLunchCallBack; }
        set { s_onLunchCallBack = value; }
    }

    #region Init

    public static void Init(bool isQuickLunch)
    {

#if UNITY_EDITOR
        phonePath = Application.dataPath.Replace("Assets", "Logs") + "/";
#else
        phonePath = "/storage/emulated/0/" + Application.productName + "/";
#endif

        if (isQuickLunch)
        {
            //复盘模式可以手动开关
            int is_open = EncryptLocalSave.GetInt(c_recordName + c_qucikLunchKey, 0);
            // 1 为开启后台操作
            if (is_open == 1)
            {
                isQuickLunch = false;
            }
        }

        if (isQuickLunch)
        {
            ChoseReplayMode(false);
        }
        else
        {
            Time.timeScale = 0;
        }
    }

    static void ChoseReplayMode(bool isReplay,string replayFileName = null)
    {
        s_isReplay = isReplay;
        UnityEngine.Random.InitState(999);
        if (s_isReplay)
        {
            ReplayPath = replayFileName;
            LoadReplayFile(replayFileName);
            
            GUIConsole.onGUICallback += ReplayModeGUI;
            GUIConsole.onGUICloseCallback += ProfileGUI;

            //传入随机数列
            RandomService.SetRandomDic(s_randomDic);
            //RandomService.SetRandomList(s_randomList);
            RandomVector2Service.SetRandomList(s_randomVector2Dic);
            //关闭正常输入，保证回放数据准确
            IInputProxyBase.IsActive = false;
        }
        else
        {
            GUIConsole.onGUICallback += RecordModeGUI;
            GUIConsole.onGUICloseCallback += ProfileGUI;

       
        }

        Time.timeScale = 1;

        try
        {
            s_onLunchCallBack?.Invoke();
            //s_onLunchCallBack();
        }
        catch (Exception e)
        {
            GameDebug.LogError(e.ToString());
        }
    }

    public static void OnEveryEventCallBack(string eventName, IInputEventBase inputEvent)
    {
        Dictionary<string, object> tmp = new Dictionary<string, object>();

        tmp.Add(c_eventNameKey, inputEvent.GetType().Name);
        tmp.Add(c_serializeInfoKey, inputEvent.Serialize());

        try
        {
            WriteInputEvent(Serializer.Serialize(tmp));
        }
        catch(Exception e)
        {
            GameDebug.LogError("Write Dev Log Error! : " + e.ToString());
        }
    }

    public static void OnGetRandomCallBack(int ID,float random)
    {
        WriteRandomRecord(ID,random);
    }

    public static void OnGetRandomVector2CallBack(int ID,Vector2 random)
    {
        WriteRandomVector2Record(ID,random);
    }

    #endregion

    #region SaveReplayFile

    public static string GetFightFilePath(string fileName,string ExpandName)
    {
        string path = PathTool.GetAbsolutePath(ResLoadLocation.Persistent,
            PathTool.GetRelativelyPath(
                c_directoryName,
                fileName,
                ExpandName));
        return path;
    }
    
    static void OpenWriteFileStream(string fileName)
    {

        try
        {
            ReplayPath = fileName;
            string path = PathTool.GetAbsolutePath(ResLoadLocation.Persistent,
                                         PathTool.GetRelativelyPath(
                                                        c_directoryName,
                                                        fileName,
                                                        c_randomExpandName));

            string dirPath = Path.GetDirectoryName(path);

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);


            //GameDebug.Log("EventStream Name: " + PathTool.GetAbsolutePath(ResLoadLocation.Persistent,
            //                             PathTool.GetRelativelyPath(
            //                                            c_directoryName,
            //                                            fileName,
            //                                            c_randomExpandName)));
            m_RandomVectorWriter = new StreamWriter(PathTool.GetAbsolutePath(ResLoadLocation.Persistent,
                PathTool.GetRelativelyPath(
                    c_directoryName,
                    fileName,
                    c_randomVector2ExpandName)));
            m_RandomVectorWriter.AutoFlush = true;
            

            m_RandomWriter = new StreamWriter(PathTool.GetAbsolutePath(ResLoadLocation.Persistent,
                                         PathTool.GetRelativelyPath(
                                                        c_directoryName,
                                                        fileName,
                                                        c_randomExpandName)));
            m_RandomWriter.AutoFlush = true;

            m_EventWriter = new StreamWriter(PathTool.GetAbsolutePath(ResLoadLocation.Persistent,
                                         PathTool.GetRelativelyPath(
                                                        c_directoryName,
                                                        fileName,
                                                        c_eventExpandName)));
            m_EventWriter.AutoFlush = true;

        }
        catch(Exception e)
        {
            GameDebug.LogError(e.ToString());
        }
    }

    static void WriteRandomRecord(int ID,float random)
    {
        if (m_RandomWriter != null)
        {
            m_RandomWriter.WriteLine(ID+":"+random.ToString());
        }
    }

    static void WriteRandomVector2Record(int ID,Vector2 random)
    {
        if (m_RandomVectorWriter != null)
        {
            m_RandomVectorWriter.WriteLine(ID+":"+random.x +","+random.y);
        }
    }

    static void WriteInputEvent(string EventSerializeContent)
    {
        //GameDebug.Log("EventSerializeContent: " + EventSerializeContent);

        if (m_EventWriter != null)
        {
            m_EventWriter.WriteLine(EventSerializeContent);
        }
    }

    #endregion 

    #region LoadReplayFile

    public static void LoadReplayFile(string fileName)
    {
        string eventContent = ResourceIOTool.ReadStringByFile(GetReplayEventFilePath(fileName));
        string randomContent = ResourceIOTool.ReadStringByFile(GetReplayRandomFilePath(fileName));
        string randomVector2Content = ResourceIOTool.ReadStringByFile(GetReplayRandomVector2FilePath(fileName));
        
        LoadEventStream(eventContent.Split('\n'));
        LoadRandomDic(randomContent.Split('\n'));
        LoadRandomVector2List(randomVector2Content.Split('\n'));
    }

    public static string GetReplayEventFilePath(string fileName)
    {
        return PathTool.GetAbsolutePath(ResLoadLocation.Persistent,
                                     PathTool.GetRelativelyPath(
                                                    c_directoryName,
                                                    fileName,
                                                    c_eventExpandName));
    }

    public static string GetReplayRandomFilePath(string fileName)
    {
        return PathTool.GetAbsolutePath(ResLoadLocation.Persistent,
                                     PathTool.GetRelativelyPath(
                                                    c_directoryName,
                                                    fileName,
                                                    c_randomExpandName));
    }
    public static string GetReplayRandomVector2FilePath(string fileName)
    {
        return PathTool.GetAbsolutePath(ResLoadLocation.Persistent,
            PathTool.GetRelativelyPath(
                c_directoryName,
                fileName,
                c_randomVector2ExpandName));
    }

    public static Deserializer Deserializer = new Deserializer();
    static void LoadEventStream(string[] content)
    {
        s_eventStream = new List<IInputEventBase>();
        for (int i = 0; i < content.Length; i++)
        {
            if (content[i] != "")
            {
                Dictionary<string, object> info = (Deserializer.Deserialize<Dictionary<string, object>> (content[i]));
                IInputEventBase eTmp = (IInputEventBase)Deserializer.Deserialize(Type.GetType(info[c_eventNameKey].ToString()),info[c_serializeInfoKey].ToString());
                s_eventStream.Add(eTmp);
            }
        }
    }

    static void LoadRandomDic(string[] content)
    {
        //s_randomList = new List<int>();
        s_randomDic = new Dictionary<int, List<float>>();
        for (int i = 0; i < content.Length; i++)
        {
            if (content[i] != "")
            {
                string[] tempS = content[i].Split(':');
                if (tempS.Length >= 2)
                {
                    int ID = int.Parse(tempS[0]);
                    float Value = float.Parse(tempS[1]);

                    if (s_randomDic.ContainsKey(ID))
                    {
                        s_randomDic.TryGetValue(ID, out List<float> tempList);
                        if (tempList == null)
                        {
                            List<float> newList = new List<float>();
                            newList.Add(Value);
                            s_randomDic.Add(ID,newList);
                        }
                        else
                        {
                            tempList.Add(Value);
                        }
                        
                    }
                    else
                    {
                        List<float> newList = new List<float>();
                        newList.Add(Value);
                        s_randomDic.Add(ID,newList);
                    }
                }
            }
        }
    }
    static void LoadRandomVector2List(string[] content)
    {
        //s_randomVector2List = new List<Vector2>();

        s_randomVector2Dic = new Dictionary<int,List<Vector2>>();
        for (int i = 0; i < content.Length; i++)
        {
            if (content[i] != "")
            {
                string[] tempS = content[i].Split(':');
                if (tempS.Length >= 2)
                {
                    int ID = int.Parse(tempS[0]);

                    if (s_randomVector2Dic.ContainsKey(ID))
                    {
                        s_randomVector2Dic.TryGetValue(ID, out List<Vector2> tempList);
                        if (tempList == null)
                        {
                            List<Vector2> newList = new List<Vector2>();
                            string[] tempV = tempS[1].Split(',');
                            if (tempV.Length > 1)
                            {
                                newList.Add(new Vector2(float.Parse(tempV[0]),float.Parse(tempV[1])) );
                                s_randomVector2Dic.Add(ID,newList);
                            }
                           
                        }
                        else
                        {
                            string[] tempV = tempS[1].Split(',');
                            if (tempV.Length > 1)
                            {
                                tempList.Add(new Vector2(float.Parse(tempV[0]),float.Parse(tempV[1])) );
                            }

                        }
                        
                    }
                    else
                    {
                        List<Vector2> newList = new List<Vector2>();
                        
                        string[] tempV = tempS[1].Split(',');
                        if (tempV.Length > 1)
                        {
                            newList.Add(new Vector2(float.Parse(tempV[0]),float.Parse(tempV[1])) );
                            s_randomVector2Dic.Add(ID,newList);
                        }
                        
                    }
                }
                
                
                
                
               
            }
        }
    }
    #endregion

    #region SaveScreenShot

    static void SaveScreenShot(string fileName)
    {
        FileTool.CreatFilePath(fileName);
        UnityEngine.ScreenCapture.CaptureScreenshot(fileName);
    }

    #endregion

    #region GUI

    static int margin = 3;
    static Rect consoleRect = new Rect(margin, margin, Screen.width * 0.2f - margin, Screen.height - 2 * margin);

    #region Develop Menu

    static Rect windowRect = new Rect(Screen.width * 0.2f, Screen.height * 0.05f, Screen.width * 0.6f, Screen.height * 0.9f);

    static void ReplayMenuGUI()
    {
        GUIUtil.SetGUIStyle();

        GUILayout.Window(1, windowRect, MenuWindow, "Develop Menu");

        if(s_isOpenWarnPanel)
        {
            GUILayout.Window(2, s_warnPanelRect, WarnWindow, "Warning");
        }
    }

    static DevMenuEnum MenuStatus = DevMenuEnum.MainMenu;
    //static bool isWatchLog = true;
    static string[] FileNameList = new string[0];
    static Vector2 scrollPos = Vector2.one;
    static void MenuWindow(int windowID)
    {
        if (MenuStatus == DevMenuEnum.MainMenu)
        {
            if (GUILayout.Button("正常启动", GUILayout.ExpandHeight(true)))
            {
                ChoseReplayMode(false);
            }

            if (GUILayout.Button("复盘模式", GUILayout.ExpandHeight(true)))
            {
                MenuStatus = DevMenuEnum.Replay;
                FileNameList = GetRelpayFileNames();
            }

            // if (GUILayout.Button("查看日志", GUILayout.ExpandHeight(true)))
            // {
            //     MenuStatus = DevMenuEnum.Log;
            //     FileNameList = LogOutPutThread.GetLogFileNameList();
            // }
            //
            // if (GUILayout.Button("查看持久文件", GUILayout.ExpandHeight(true)))
            // {
            //     MenuStatus = DevMenuEnum.PersistentFile;
            //     FileNameList = PersistentFileManager.GetFileList();
            // }
        }
        else if (MenuStatus  == DevMenuEnum.Replay)
        {
            ReplayListGUI();
        }
        else if (MenuStatus == DevMenuEnum.Log)
        {
            LogGUI();
        }
        else if (MenuStatus == DevMenuEnum.PersistentFile)
        {
            PersistentFileGUI();
        }
    }

    #region ReplayListGUI

    static bool isUploadReplay;

    static void ReplayListGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < FileNameList.Length; i++)
        {
            if (!isUploadReplay)
            {
                if (GUILayout.Button(FileNameList[i]))
                {
                    ChoseReplayMode(true, FileNameList[i]);
                }
            }
            else
            {
                if (GUILayout.Button("上传 " + FileNameList[i]))
                {
                    string replayPath = PathTool.GetAbsolutePath(ResLoadLocation.Persistent,
                        c_directoryName);
                    DirectoryInfo dir = new DirectoryInfo(replayPath);
                    FileInfo[] file = dir.GetFiles();
                    for (int t = 0; t < file.Length; t++)
                    {
                        if (file[t].Name.Contains(FileNameList[i]))
                        {
                            CoroutineRunner.Instance.StartCoroutine(WWWUpLoadUtils.Upload(file[t].FullName));
                        }
                    }
                    
                    //string replayPath = GetReplayEventFilePath(FileNameList[i]);
                    //string randomPath = GetReplayRandomFilePath(FileNameList[i]);
                    //string randomVector2Path = GetReplayRandomVector2FilePath(FileNameList[i]);

                    //HTTPTool.Upload_Request_Thread(URLManager.GetURL("ReplayFileUpLoadURL"), replayPath, UploadCallBack);
                    // HTTPTool.Upload_Request_Thread(URLManager.GetURL("ReplayFileUpLoadURL"), randomPath, UploadCallBack);
                    // HTTPTool.Upload_Request_Thread(URLManager.GetURL("ReplayFileUpLoadURL"), randomVector2Path, UploadCallBack);
                }
            }
        }

        GUILayout.EndScrollView();

        if (GUILayout.Button("清除记录"))
        {
            OpenWarnWindow("确定要删除所有记录吗？", () =>
            {
                GameDebug.Log("已删除所有记录");
                FileTool.SafeDeleteDirectory(PathTool.GetAbsolutePath(ResLoadLocation.Persistent, c_directoryName));
                FileNameList = new string[0];
            });
        }

        if (URLManager.GetURL("ReplayFileUpLoadURL") != null)
        {
            if (GUILayout.Button("上传模式 ： " + isUploadReplay))
            {
                isUploadReplay = !isUploadReplay;
            }
        }
        else
        {
            GUILayout.Label("上传持久数据需要在 URLConfig -> ReplayFileUpLoadURL 配置上传目录");
        }

        if (GUILayout.Button("返回上层"))
        {
            MenuStatus = DevMenuEnum.MainMenu;
        }
    }

    #endregion

    #region LogGUI

    static bool isShowLog = false;

    static void LogGUI()
    {
        if (isShowLog)
        {
            ShowLog();
        }
        else
        {
            ShowLogList();
        }
    }

    static string showContent = "";
    static string LogPath = "";
    static string LogName = "";
    //#if UNITY_EDITOR
    //    static string phonePath =  Application.dataPath.Replace("Assets","Logs") + "/";
    //#else
    //    static  string phonePath = "/storage/emulated/0/" + Application.productName + "/";
    //#endif
    static string phonePath;
    static void ShowLogList()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < FileNameList.Length; i++)
        {
            LogName = FileNameList[i];
            if (GUILayout.Button(LogName))
            {
                isShowLog = true;
                scrollPos = Vector2.zero;
                showContent = LogOutPutThread.LoadLogContent(FileNameList[i]);
                LogPath = LogOutPutThread.GetPath(FileNameList[i]);
            }
        }

        GUILayout.EndScrollView();
        if (GUILayout.Button("复制到设备"))
        {
            for (int i = 0; i < FileNameList.Length; i++)
            {
               string name = FileNameList[i];
                string path = phonePath + name + ".txt";
              string  LogPath = LogOutPutThread.GetPath(name);

                FileTool.CreatFilePath(path);
                File.Copy(LogPath, path, true);
                
            }
            GUIUtil.ShowTips("复制成功");
        }

        if (GUILayout.Button("清除日志"))
        {
            OpenWarnWindow("确定要删除所有日志吗？", () =>
             {
                 GameDebug.Log("已删除所有日志");
                 FileTool.SafeDeleteDirectory(PathTool.GetAbsolutePath(ResLoadLocation.Persistent,LogOutPutThread.LogPath));
                 FileNameList = new string[0];
             });
        }

        if (GUILayout.Button("返回上层"))
        {
            MenuStatus = DevMenuEnum.MainMenu;
        }
    }

    static void ShowLog()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        try
        {
            GUIUtil.SafeTextArea(showContent);
        }
        catch(Exception e)
        {
            GUILayout.TextArea(e.ToString());
        }
        
        GUILayout.EndScrollView();

        if(URLManager.GetURL("LogUpLoadURL") != null)
        {
            if (GUILayout.Button("上传日志"))
            {
                HTTPTool.Upload_Request_Thread(URLManager.GetURL("LogUpLoadURL"), LogPath, UploadCallBack);
            }
        }
        else
        {
            GUILayout.Label("上传日志需要在 URLConfig -> LogUpLoadURL 配置上传目录");
        }

#if UNITY_ANDROID
        if (GUILayout.Button("导出到设备"))
        {
            try
            {
                string path = phonePath+ LogName + ".txt";
                FileTool.CreatFilePath(path);
                File.Copy(LogPath, path,true);
                GUIUtil.ShowTips("复制成功");
            }
            catch (Exception e)
            {
                GUIUtil.ShowTips(e.ToString());
            }

        }
#endif

        if (GUILayout.Button("复制到剪贴板"))
        {
            TextEditor tx = new TextEditor();
            tx.text = showContent;
            tx.OnFocus();
            tx.Copy();
        }

        if (GUILayout.Button("返回上层"))
        {
            isShowLog = false;
        }
    }

#endregion

#region PersistentFileGUI

    static bool isShowPersistentFile = false;
    static void PersistentFileGUI()
    {
        if (isShowPersistentFile)
        {
            ShowPersistentFile();
        }
        else
        {
            ShowPersistentFileList();
        }
    }

    static void ShowPersistentFile()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        try
        {
            GUIUtil.SafeTextArea(showContent);
        }
        catch (Exception e)
        {
            GUILayout.TextArea(e.ToString());
        }

        GUILayout.EndScrollView();

        if (URLManager.GetURL("PersistentFileUpLoadURL") != null)
        {
            if (GUILayout.Button("上传持久数据"))
            {
                HTTPTool.Upload_Request_Thread(URLManager.GetURL("PersistentFileUpLoadURL"), LogPath, UploadCallBack);
            }
        }
        else
        {
            GUILayout.Label("上传持久数据需要在 URLConfig -> PersistentFileUpLoadURL 配置上传目录");
        }

        if (GUILayout.Button("复制到剪贴板"))
        {
            TextEditor tx = new TextEditor();
            tx.text = showContent;
            tx.OnFocus();
            tx.Copy();
        }

        if (GUILayout.Button("返回上层"))
        {
            isShowPersistentFile = false;
        }
    }

    static void ShowPersistentFileList()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < FileNameList.Length; i++)
        {
            if (GUILayout.Button(FileNameList[i]))
            {
                //isShowPersistentFile = true;
                //scrollPos = Vector2.zero;
                //showContent = PersistentFileManager.GetData(FileNameList[i]);
                //LogPath = PersistentFileManager.GetPath(FileNameList[i]);

                string path = PersistentFileManager.GetPath(FileNameList[i]);
                HTTPTool.Upload_Request_Thread(URLManager.GetURL("PersistentFileUpLoadURL"), path, UploadCallBack);
            }
        }

        GUILayout.EndScrollView();

        if (URLManager.GetURL("PersistentFileUpLoadURL") != null)
        {
            if (GUILayout.Button("上传所有持久数据文件"))
            {
                for (int i = 0; i < FileNameList.Length; i++)
                {
                    string path = PersistentFileManager.GetPath(FileNameList[i]);
                    HTTPTool.Upload_Request_Thread(URLManager.GetURL("PersistentFileUpLoadURL"), path, UploadCallBack);
                }
            }
        }
        else
        {
            GUILayout.Label("上传持久数据文件需要在 URLConfig -> PersistentFileUpLoadURL 配置上传目录");
        }

        if (GUILayout.Button("清除持久数据文件"))
        {
            OpenWarnWindow("确定要删除所有持久数据文件吗？", () =>
            {
                GameDebug.Log("已删除所有持久数据文件");
                FileTool.SafeDeleteDirectory(PathTool.GetAbsolutePath(ResLoadLocation.Persistent, PersistentFileManager.c_directoryName));
                FileNameList = new string[0];
            });
        }

        if (GUILayout.Button("返回上层"))
        {
            MenuStatus = DevMenuEnum.MainMenu;
        }
    }

#endregion

#region WarnPanel

    static Rect s_warnPanelRect = new Rect(Screen.width * 0.1f, Screen.height * 0.25f, Screen.width * 0.8f, Screen.height * 0.5f);

    static bool s_isOpenWarnPanel = false;
    static string s_warnContent = "";
    static CallBack s_warnCallBack;

    static void OpenWarnWindow(string content,CallBack callBack)
    {
        s_isOpenWarnPanel = true;
        s_warnContent = content;
        s_warnCallBack = callBack;
    }

    static void WarnWindow(int windowID)
    {
        GUILayout.Label(s_warnContent, GUILayout.ExpandHeight(true));

        if(GUILayout.Button("取消", GUILayout.ExpandHeight(true)))
        {
            s_isOpenWarnPanel = false;
            s_warnContent = "";
        }

        if (GUILayout.Button("确定",GUILayout.ExpandHeight(true)))
        {
            s_isOpenWarnPanel = false;
            s_warnContent = "";

            if (s_warnCallBack != null)
            {
                s_warnCallBack();
            }
        }
    }

#endregion

#endregion

#region ProfileGUI

    static void SwitchProfileGUI()
    {
        if (s_isProfile)
        {
            s_ProfileGUICallBack?.Invoke();

            if (GUILayout.Button("关闭 性能数据" + GetHotKey(2), GUILayout.ExpandHeight(true)))
            {
                s_isProfile = false;
            }
        }
        else
        {
            if (GUILayout.Button("开启 性能数据" + GetHotKey(2), GUILayout.ExpandHeight(true)))
            {
                s_isProfile = true;
            }
        }
    }

    static void ProfileGUI()
    {
        if(s_isProfile)
        {
            s_ProfileGUICallBack?.Invoke();
        }
    }

    static void DevelopHotKeyLogic()
    {
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                s_isProfile = !s_isProfile;
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                if (Time.timeScale != 0)
                {
                    Time.timeScale = 0;
                }
                else
                {
                    Time.timeScale = 1;
                }
            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                if (Time.timeScale == 0)
                {
                    Time.timeScale = 1;
                }

                Time.timeScale *= 2;
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                GameDebug.Log("Time.timeScale " + Time.timeScale);

                if (Time.timeScale == 0)
                {
                    Time.timeScale = 1;
                }

                Time.timeScale *= 0.5f;
            }

            if (Input.GetKeyDown(KeyCode.F10))
            {
                string name = GetScreenshotFileName();
                GameDebug.Log("已保存 屏幕截图 " + name);

                SaveScreenShot(name);
            }
        }
        
    }

#endregion

#region RecordMode
    static void RecordModeGUI()
    {
        GUILayout.Window(2, consoleRect, RecordModeGUIWindow, "Develop Control Panel");
    }
    static void RecordModeGUIWindow(int id)
    {
        SwitchProfileGUI();
        int flag = EncryptLocalSave.GetInt(c_recordName + c_qucikLunchKey, 0);
        if (flag == 0)
        {
            if (GUILayout.Button("开启后台", GUILayout.ExpandHeight(true)))
            {
                EncryptLocalSave.SetInt(c_recordName + c_qucikLunchKey,1);
            }
        }
        else
        {
            if (GUILayout.Button("关闭后台", GUILayout.ExpandHeight(true)))
            {
                EncryptLocalSave.SetInt(c_recordName + c_qucikLunchKey,0);
            }
        }
    }

#endregion

#region ReplayMode

    /// <summary>
    /// 回放模式的GUI
    /// </summary>
    static void ReplayModeGUI()
    {
        GUILayout.Window(2, consoleRect, ReplayModeGUIWindow, "Replay Control Panel");
    }

    static void ReplayModeGUIWindow(int id)
    {
        ReplayProgressGUI();

        SwitchProfileGUI();

        if (GUILayout.Button("暂停" + GetHotKey(3), GUILayout.ExpandHeight(true)))
        {
            Time.timeScale = 0f;
        }

        if (GUILayout.Button("正常速度" + GetHotKey(3,false), GUILayout.ExpandHeight(true)))
        {
            Time.timeScale = 1;
        }

        if (GUILayout.Button("速度加倍" + GetHotKey(4), GUILayout.ExpandHeight(true)))
        {
            if(Time.timeScale == 0)
            {
                Time.timeScale = 1;
            }
            Time.timeScale *= 2f;
        }

        if (GUILayout.Button("速度减半" + GetHotKey(5), GUILayout.ExpandHeight(true)))
        {
            if (Time.timeScale == 0)
            {
                Time.timeScale = 1;
            }

            Time.timeScale *= 0.5f;
        }
    }

    static void ReplayProgressGUI()
    {
        string timeContent = "当前时间：" + Time.time.ToString("F");
        timeContent = timeContent.PadRight(20);

        string eventContent = "剩余输入：" + s_eventStream.Count;
        eventContent = eventContent.PadRight(20);

        string RandomContent = "剩余随机数：" + RandomService.GetRandomListCount();
        RandomContent = RandomContent.PadRight(20);

        string speedContent = "当前速度：X" + Time.timeScale;
        speedContent = speedContent.PadRight(20);

        GUILayout.TextField(timeContent + eventContent + RandomContent + speedContent);
    }

    static string GetHotKey(int fn,bool isShowInRun = true)
    {

#if UNITY_EDITOR

        if (fn == 3)
        {
            if ((Time.timeScale != 0 && isShowInRun)
                ||(Time.timeScale == 0 && !isShowInRun))
            {
                return "(F" + fn + ")";
            }
            else
            {
                return "";
            }
        }
        else
        {
            return "(F" + fn + ")";
        }
#else
        return "";
#endif
    }

#endregion

#endregion

    #region Update

    static float s_currentTime = 0;

    public static float CurrentTime
    {
        get { return DevelopReplayManager.s_currentTime; }
        //set { DevelopReplayManager.s_currentTime = value; }
    }



    public static void OnReplayUpdate()
    {
        DevelopHotKeyLogic();

        for (int i = 0; i < s_eventStream.Count; i++)
        {
            if (s_eventStream[i].m_t < Time.time)
            {
                InputManagerV2.Dispatch(s_eventStream[i].GetType().Name, s_eventStream[i]);

                s_eventStream.RemoveAt(i);
                i--;
            }
        }
    }

    static void OnRecordUpdate()
    {
        DevelopHotKeyLogic();

        s_currentTime += Time.deltaTime;
    }

    #endregion

    #region Tool

    public static string[] GetRelpayFileNames()
    {
        FileTool.CreatPath(PathTool.GetAbsolutePath(ResLoadLocation.Persistent, c_directoryName));

        List<string> relpayFileNames = new List<string>();
        string[] allFileName = Directory.GetFiles(PathTool.GetAbsolutePath(ResLoadLocation.Persistent, c_directoryName));
        foreach (var item in allFileName)
        {
            if (item.EndsWith("." + c_eventExpandName))
            {
                string configName = FileTool.RemoveExpandName(FileTool.GetFileNameByPath(item));
                relpayFileNames.Add(configName);
            }
        }

        return relpayFileNames.ToArray() ?? new string[0];
    }

    static string GetReplayFileName()
    {
        DateTime now = System.DateTime.Now;
        string logName = string.Format("Replay{0}-{1:D2}-{2:D2}#{3:D2}-{4:D2}-{5:D2}",
            now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

        return logName;
    }

    static string GetScreenshotFileName()
    {
        DateTime now = System.DateTime.Now;
        //string screenshotName = string.Format("Screenshot{0}-{1:D2}-{2:D2}#{3:D2}-{4:D2}-{5:D2}",
        //    now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

        string screenshotName = string.Format("ScreenShot_{0}x{1}_{2}", Screen.width, Screen.height, System.DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);


        string path = PathTool.GetAbsolutePath(ResLoadLocation.Persistent,
                                         PathTool.GetRelativelyPath(
                                                        "ScreenShot",
                                                        screenshotName,
                                                        "jpg"));

        return path;
    }

    static void UploadCallBack(string result)
    {
        GUIUtil.ShowTips(result);
    }

    enum DevMenuEnum
    {
        MainMenu,
        Replay,
        Log,
        PersistentFile
    }

    #endregion
}
