//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;
//using XMLib;

//namespace AliveCell
//{
//    public class RecordService : IServiceInitialize
//    {
//        private SuperLogHandler LogHandler = SuperLogHandler.Create("RS");
//        public string dir { get; private set; } = string.Empty;
//        public bool isRecording { get; protected set; } = false;

//        private RecordBuffer _buffer = null;
//        private List<RecordData> _recordDatas = null;
//        public IReadOnlyList<RecordData> recordDatas => _recordDatas;
//        public IEnumerator OnServiceInitialize()
//        {
//            _buffer = new RecordBuffer();
//            _recordDatas = new List<RecordData>();
//            dir = Path.Combine(UnityEngine.Application.persistentDataPath, "Recorders").Replace('\\', '/');
//            if (!Directory.Exists(dir))
//            {
//                Directory.CreateDirectory(dir);
//            }
//            初始化录像数据
//            InitRecordData();
//            yield break;
//        }

//        private void InitRecordData()
//        {
//            string[] filePaths = Directory.GetFiles(dir, "*.bin", SearchOption.TopDirectoryOnly);
//            for (int i = 0; i < filePaths.Length; i++)
//            {
//                RecordData data = RecordData.Create(filePaths[i]);
//                _recordDatas.Add(data);
//            }
//            排序
//            _recordDatas.Sort((t1, t2) => string.Compare(t2.fileName, t1.fileName));
//        }
//        / <summary>
//        / 删除录像
//        / </summary>
//        public void RemoveRecord(string fileName)
//        {
//            int index = _recordDatas.FindIndex(t => 0 == string.Compare(t.fileName, fileName));
//            if (index < 0)
//            {
//                LogHandler.LogWarning("未找到录像对象:{0}", fileName);
//                return;
//            }
//            RecordData data = _recordDatas[index];

//            _recordDatas.RemoveAt(index);//删除对象
//            File.Delete(data.filePath);//删除文件
//        }
//        public RecordPlayer CreatePlayer(string fileName, RecordBuffer data)
//        {
//            DataUtility.FromJson();
//            FlatBuffersUtility.ReadFromFile(data.filePath);
//            return RecordPlayer.Create(fileName, data);
//        }
//        public RecordPlayer CreatePlayer(string fileName)
//        {
//            string jsonContent = ResourceIOTool.ReadStringByFile(fileName);
//            RecordBuffer data = DataUtility.FromJson<RecordBuffer>(jsonContent);
//            return CreatePlayer(fileName, data);
//        }

//        public RecordProxy CreateRecorderProxy()
//        {
//            if (isRecording)
//            {
//                LogHandler.LogError("已经开始记录，创建代理失败");
//                return null;
//            }
//            return new RecordProxy(this);
//        }

//        public class RecordBuffer
//        {
//            public bool isFinsished { get; private set; } = false;

//            public FlatBufferBuilder builder = null;
//            public int mapId = 0;
//            public int selfId = 0;
//            public GameMode gameMode = GameMode.None;
//            public List<FrameData> frames = null;
//            public List<PlayerData> players = null;

//            public RecordBuffer(int fbSize = 256 * 1024, int playerCnt = 8, int frameCnt = 9000)
//            {
//                builder = new FlatBufferBuilder(fbSize);
//                frames = new List<FrameData>(frameCnt); //5min时长帧数
//                players = new List<PlayerData>(playerCnt);
//                mapId = 0;
//                isFinsished = false;
//            }

//            public void Clear()
//            {
//                isFinsished = false;
//                builder.Clear();
//                frames.Clear();
//                players.Clear();
//                mapId = 0;
//                gameMode = GameMode.None;
//            }

//            public void SetMapId(int mapId)
//            {
//                SuperLog.Assert(!isFinsished, "已结束，不许继续设置");
//                this.mapId = mapId;
//            }

//            public void SetGameMode(GameMode mode)
//            {
//                SuperLog.Assert(!isFinsished, "已结束，不许继续设置");
//                this.gameMode = mode;
//            }

//            public void SetSelfId(int id)
//            {
//                SuperLog.Assert(!isFinsished, "已结束，不许继续设置");
//                this.selfId = id;
//            }

//            public void AddPlayer(int playerType, string playerId, int groupId, int uid, int birthIndex)
//            {
//                SuperLog.Assert(!isFinsished, "已结束，不许继续设置");
//                PlayerData player = new PlayerData();
//                player.playerType = playerType;
//                player.PlayerID = playerId;
//                player.id = uid;
//                player.birthIndex = birthIndex;
//                player.group = groupId;

//                players.Add(player);
//            }

//            public void AddFrame(int frameIndex, IReadOnlyDictionary<int, InputData> playerInputs)
//            {
//                SuperLog.Assert(!isFinsished, "已结束，不许继续设置");
//                if (playerInputs.Count > 0)
//                {
//                    FrameData frameData = new FrameData();
//                    frameData.frameIndex = frameIndex;
//                    foreach (var data in playerInputs)
//                    {
//                        InputRecordData inputData = new InputRecordData();
//                        inputData.id = data.Key;
//                        inputData.keyCode = (int)data.Value.keyCode;
//                        inputData.axisValue = data.Value.axisValue;
//                        frameData.Inputs.Add(inputData);
//                    }
//                    GameDebug.LogError(frameIndex + "  " + frameData.ToString());
//                    frames.Add(frameData);
//                }
//            }

//            public void WriteToFile(string filePath)
//            {
//                if (!isFinsished)
//                {
//                    Finish();
//                }

//                builder.WriteToFile(filePath);

//                //Debug Data
//#if UNITY_EDITOR
//                string json = FrameRecorderT.DeserializeFromBinary(builder.SizedByteArray()).SerializeToJson();
//                File.WriteAllText(filePath + ".json", json);
//#endif

//            }

//            public void Finish()
//            {
//                SuperLog.Assert(!isFinsished, "已结束");
//                isFinsished = true;

//                builder.StartVector(4, players.Count, 4);
//                for (int i = players.Count - 1; i >= 0; i--)
//                {
//                    builder.AddOffset(players[i].Value);
//                }
//                VectorOffset playersOffset = builder.EndVector();

//                builder.StartVector(4, frames.Count, 4);
//                for (int i = frames.Count - 1; i >= 0; i--)
//                {
//                    builder.AddOffset(frames[i].Value);
//                }
//                VectorOffset framesOffset = builder.EndVector();

//                FrameRecorder.StartFrameRecorder(builder);
//                FrameRecorder.AddSelfId(builder, selfId);
//                FrameRecorder.AddMapId(builder, mapId);
//                FrameRecorder.AddGameMode(builder, (int)gameMode);
//                FrameRecorder.AddPlayers(builder, playersOffset);
//                FrameRecorder.AddFrames(builder, framesOffset);
//                var record = FrameRecorder.EndFrameRecorder(builder);
//                FrameRecorder.FinishFrameRecorderBuffer(builder, record);
//            }
//        }

//        public class RecordProxy : IDisposable
//        {
//            private RecordService rs;
//            private SuperLogHandler LogHandler;
//            public RecordBuffer buffer => rs._buffer;
//            internal RecordProxy(RecordService rs)
//            {
//                this.rs = rs;
//                this.LogHandler = rs.LogHandler.CreateSub("RC");
//            }
//            public void Start()
//            {
//                LogHandler.Log("Start");

//                rs.isRecording = true;
//                buffer.Clear();
//            }
//            public void SetMapId(int mapId)
//            {
//                if (!rs.isRecording)
//                {
//                    return;
//                }
//                buffer.SetMapId(mapId);
//            }
//            public void SetGameMode(GameMode gameMode)
//            {
//                if (!rs.isRecording)
//                {
//                    return;
//                }
//                buffer.SetGameMode(gameMode);
//            }
//            public void SetSelfId(int uid)
//            {
//                if (!rs.isRecording)
//                {
//                    return;
//                }
//                buffer.SetSelfId(uid);
//            }
//            public void AddPlayer(int playerType, string playerId, int groupId, int uid, int birthIndex)
//            {
//                if (!rs.isRecording)
//                {
//                    return;
//                }
//                buffer.AddPlayer(playerType, playerId, groupId, uid, birthIndex);
//            }
//            public void AddFrame(int frameIndex, IReadOnlyDictionary<int, InputData> playerInputs)
//            {
//                if (!rs.isRecording)
//                {
//                    return;
//                }
//                buffer.AddFrame(frameIndex, playerInputs);
//            }
//            public void Stop()
//            {
//                if (!rs.isRecording)
//                {
//                    return;
//                }

//                LogHandler.Log("Stop");
//                rs.isRecording = false;
//                buffer.Finish();
//            }
//            public string GenerateFullFilePath()
//            {
//                string fileName = RecordData.CreateFileName(DateTime.Now, buffer.mapId, buffer.frames.Count);
//                return Path.Combine(rs.dir, fileName).Replace('\\', '/');
//            }
//            public void Save(string file_time)
//            {
//                if (rs.isRecording)
//                {//保存时先停止
//                    Stop();
//                }

//                string filePath = DevelopReplayManager.GetFightFilePath(DevelopReplayManager.ReplayPath, file_time);
//                DevelopReplayManager.ReplayPath + "." + file_time;
//                GenerateFullFilePath();
//                LogHandler.Log($"保存录像到 :{filePath}");

//                using (var watcher = new TimeWatcher("存储帧记录管理器数据"))
//                {
//                    string json = DataUtility.ToJson(buffer);
//                    File.WriteAllText(filePath, json);
//                    buffer.WriteToFile(filePath);

//                    RecordData data = RecordData.Create(filePath);
//                    if (rs._recordDatas.Count > 0)
//                    {
//                        rs._recordDatas.Insert(0, data);
//                    }
//                    else
//                    {
//                        rs._recordDatas.Add(data);
//                    }
//                }
//            }
//            #region IDisposable Support

//            private bool disposedValue = false; // 要检测冗余调用

//            protected virtual void Dispose(bool disposing)
//            {
//                if (!disposedValue)
//                {
//                    if (disposing)
//                    {
//                    }

//                    rs.isRecording = false;
//                    buffer.Clear();

//                    disposedValue = true;
//                }
//            }

//            ~RecordProxy()
//            {
//                Dispose(false);
//            }

//            public void Dispose()
//            {
//                Dispose(true);
//                GC.SuppressFinalize(this);
//            }

//            #endregion IDisposable Support
//        }


//    }
//    public class RecordPlayer
//    {
//        public string fileName { get; private set; }
//        private RecordService.RecordBuffer recorder = default;

//        private RecordPlayer()
//        {
//        }

//        public static RecordPlayer Create(string fileName, in RecordService.RecordBuffer recorder)
//        {
//            return new RecordPlayer()
//            {
//                fileName = fileName,
//                recorder = recorder
//            };
//        }

//        public int mapId => recorder.mapId;

//        public int frameCount => recorder.frames.Count;

//        public int playerCount => recorder.players.Count;

//        public int selfId => recorder.selfId;

//        public GameMode gameMode => (GameMode)recorder.gameMode;

//        public PlayerData GetPlayer(int index)
//        {
//            return recorder.players[index];
//        }

//        public FrameData GetFrame(int index)
//        {
//            return recorder.frames[index];
//        }

//        public bool HasFrame(int index)
//        {
//            return recorder.frames.Count > 0 && index < recorder.frames.Count && index >= 0;
//        }

//        public override string ToString()
//        {
//            return $"[RecordPlayer]fileName:{fileName}";
//        }
//    }

//    public class RecordData
//    {
//        public string filePath { get; private set; }
//        public string fileName { get; private set; }

//        public DateTime date { get; private set; }
//        public string mapName => GlobalSetting.scene.FromID(mapIndex);
//        public int mapIndex { get; private set; }
//        public TimeSpan gameTime { get; private set; }

//        public string displayName => GetDisplayName();

//        public string GetDisplayName()
//        {
//            return string.Format("{1} {2}:{3}\n{0}", date.ToString("yyyy-MM-dd HH:mm:ss"), mapName, gameTime.Minutes, gameTime.Seconds);
//        }
//        private RecordData()
//        {
//        }
//        public const string formatDateTime = "yyyyMMddHHmmss";

//        public static string CreateFileName(DateTime date, int mapid, int frameCnt)
//        {
//            int secends = (int)(frameCnt * GameWorld.logicFrameRate);
//            //14 + 2 + 8
//            return string.Format("{0}{1:00}{2:00000000}.bin", date.ToString(formatDateTime), mapid, secends);
//        }
//        public static RecordData Create(string filePath)
//        {
//            string fileName = Path.GetFileNameWithoutExtension(filePath);
//            RecordData data = new RecordData();
//            data.filePath = filePath;
//            data.fileName = fileName;

//            string dateStr = fileName.Substring(0, 14);
//            string mapIndexStr = fileName.Length >= 16 ? fileName.Substring(14, 2) : "-1";
//            string secendsStr = fileName.Length >= 24 ? fileName.Substring(16, 8) : "0";

//            data.mapIndex = int.Parse(mapIndexStr);
//            int seceneds = int.Parse(secendsStr);

//            data.date = DateTime.ParseExact(dateStr, formatDateTime, null);
//            data.gameTime = TimeSpan.FromSeconds(seceneds);

//            return data;
//        }
//        public override string ToString()
//        {
//            return $"[{fileName}]{filePath}";
//        }
//    }
//}