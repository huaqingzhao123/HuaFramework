#if !ASSET_RESOURCES && (!UNITY_IOS || USE_SUB_PACKAGE) 
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Nireus
{

    //queue
    public class DownloadQueue : MonoBehaviour, IWebFileObserver
    {

#region Public variables
        private IDownloadQueue _owner;
        //public WebFileCacheList CacheList { get { return cacheList; } }
        //public Action<float, int, int> OnProgress { get; set; }
#endregion

#region Private variables

        Dictionary<int, List<WebFileLoader>> queue_file_list = new Dictionary<int, List<WebFileLoader>>();
        Dictionary<int,List<WebFileLoader>> finished_file_list = new Dictionary<int,List<WebFileLoader>>();
        List<WebFileLoader> waitingStack = new List<WebFileLoader>();
        List<WebFileLoader> LoadingQueue = new List<WebFileLoader>();
        List<WebFileLoader> destoryQueue = new List<WebFileLoader>();
        //WebFileCacheList	cacheList = new WebFileCacheList();
        //最大线程数量,避免开过多线程引起卡顿
        int maxUpdateQueue = 3;
        int downloaded_bytes;
        bool starting;
#endregion

#region Public methods
        public void SetOwner(IDownloadQueue owner)
        {
            this._owner = owner;
        }

        public void SetMaxLoader(int max)
        {
            maxUpdateQueue = max;
        }

        public void StartDownload()
        {
            starting = true;
        }

        public void StopDownload()
        {
            starting = false;
        }

        /// <summary>
        /// Adds the queue.
        /// </summary>
        /// <returns>The queue.</returns>
        /// <param name="observer">Observer.</param>
        /// <param name="url">URL.</param>
        public void AddQueue(string url,long file_len, object user_data,int group_id)
        {
            IWebFileObserver observer = this;
            WebFileLoader loader;

            if (ContainsUrl(queue_file_list, url) != null)
            {
                return;
            }
            else
            {
                loader = new WebFileLoader(url, group_id, file_len, user_data);
                loader.AddObserver(observer);
                waitingStack.Add(loader);
                AddQueueFile(loader);
            }
        }


        /// <summary>
        /// Determines whether this instance cancel queue the specified observer.
        /// 假如observer数量为空,就会释放下载列队里的请求.
        /// </summary>
        /// <returns><c>true</c> if this instance cancel queue the specified observer; otherwise, <c>false</c>.</returns>
        /// <param name="observer">Observer.</param>
        public void CancelQueue()
        {
            GameDebug.Log("CancelQueue ");
            IWebFileObserver observer = this;
            List<WebFileLoader> list = new List<WebFileLoader>();
            list.AddRange(waitingStack);
            list.AddRange(LoadingQueue);
            list.AddRange(destoryQueue);

            for (int i = 0; i < list.Count; i++)
            {
                WebFileLoader loader = list[i];
                if (loader.ObserversList.IndexOf(observer) != -1)
                {
                    loader.RemoveObserver(observer);
                    waitingStack.Remove(loader);
                    LoadingQueue.Remove(loader);
                    if (loader.ObserversList.Count == 0)
                    {
                        if (loader.CurState != WebFileLoader.State.Dispose)
                        {
                            destoryQueue.Add(loader);
                            loader.SetState(WebFileLoader.State.Dispose);
                        }
                    }
                }
            }
            
            UpdateDestroy();
            destoryQueue.Clear();
            waitingStack.Clear();
            LoadingQueue.Clear();
            queue_file_list.Clear();
            finished_file_list.Clear();
            downloaded_bytes = 0;
        }

#endregion

#region Private methods

        void UpdateDestroy()
        {
            if (destoryQueue.Count > 0)
            {
                for (int i = 0; i < destoryQueue.Count; i++)
                {
                    WebFileLoader loader = destoryQueue[i];
                    loader.UpdateState();
                }
                destoryQueue.Clear();
            }
        }

        void Update()
        {
            if (starting == false)
            {
                return;
            }
            //提取到下载线程
            if (LoadingQueue.Count < maxUpdateQueue)
            {
                if (waitingStack.Count > 0)
                {
                    waitingStack[0].SetState(WebFileLoader.State.Start);
                    LoadingQueue.Add(waitingStack[0]);
                    waitingStack.RemoveAt(0);
                }
            }
            //处理数据标志了销毁的WWW线程,并且释放掉数据.
            UpdateDestroy();
            List<WebFileLoader> remove_list = new List<WebFileLoader>();
            //对每个loader进行更新状态.
            for (int i = 0; i < LoadingQueue.Count; i++)
            {
                WebFileLoader loader = LoadingQueue[i];

                loader.UpdateState();

                if (loader.CurState == WebFileLoader.State.Complate || loader.CurState == WebFileLoader.State.Error)
                {
                    remove_list.Add(loader);
                    //LoadingQueue.RemoveAt(i);
                   // i--;
                }
            }

            for (int i = 0; i < remove_list.Count; i++)
            {
                WebFileLoader loader = remove_list[i];
                LoadingQueue.Remove(loader);
            }
        }

        WebFileLoader ContainsUrl(Dictionary<int, List<WebFileLoader>> dict, string url)
        {
            foreach (var kv in dict)
            {
                foreach (WebFileLoader item in kv.Value)
                {
                    if (item.Url == url)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        int ContainsObserver(List<WebFileLoader> list, IWebFileObserver observer)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].ObserversList.IndexOf(observer) != -1)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool IsHadFileDownloading()
        {
            return (waitingStack.Count > 0 || LoadingQueue.Count > 0);
        }


        public void OnComplate(IWebFileObserver obs, byte[] datas, string url, object user_data, WebFileLoader loader)
        {
            downloaded_bytes += datas.Length;
            _owner.OnDownLoadOneSucc(url, datas, user_data, loader.DownloadQueueGroupId);
   
            //GameDebug.Log("OnComplate " + url);
            AddFinishFile(loader);

            bool check_group_finish = CheckGroupDone(loader.DownloadQueueGroupId);
            if (check_group_finish)
            {
                _owner.OnDownloadGroupSucc(user_data,loader.DownloadQueueGroupId);
            }

            if (GetFileCount(finished_file_list) == GetFileCount(queue_file_list))
            {
                downloaded_bytes = 0;
                _owner.OnDownloadAllSucc(user_data);
                StopDownload();
            }
        }


        int GetFileCount(Dictionary<int, List<WebFileLoader>>  dict)
        {
            int count = 0;
            foreach (var kv in dict)
            {
                count += kv.Value.Count;
            }
            return count;
        }

        void AddFinishFile(WebFileLoader loader)
        {
            List<WebFileLoader> list;
            if (finished_file_list.TryGetValue(loader.DownloadQueueGroupId, out list) == false)
            {
                list = new List<WebFileLoader>();
                finished_file_list.Add(loader.DownloadQueueGroupId,list);
            }
            if (list.Contains(loader) == false)
            {
                list.Add(loader);
            }
           // GameDebug.Log("finish list " + list.Count+" group "+ loader.DownloadQueueGroupId);
        }

        void AddQueueFile(WebFileLoader loader)
        {
            List<WebFileLoader> list;
            if (queue_file_list.TryGetValue(loader.DownloadQueueGroupId, out list) == false)
            {
                list = new List<WebFileLoader>();
                queue_file_list.Add(loader.DownloadQueueGroupId, list);
            }
            if (list.Contains(loader) == false)
            {
                list.Add(loader);
            }
           // GameDebug.Log("add " + loader.Url + ", count:" + list.Count);
        }

        bool CheckGroupDone(int group_id)
        {
            List<WebFileLoader> list;
            if (finished_file_list.TryGetValue(group_id, out list) )
            {
                List<WebFileLoader> list2;
                if (queue_file_list.TryGetValue(group_id, out list2))
                {
                    return list.Count == list2.Count;
                }
                return false;
            }
            return false;
        }


        public void OnError(IWebFileObserver obs, string error, object user_data)
        {
            _owner.OnError(error, user_data);
        }

        public void OnProcess(IWebFileObserver obs, float p, int downloaded_byte, object user_data)
        {
            long max_byte_len = GetAllFileLength();
            int cur_byte = this.downloaded_bytes + downloaded_byte;
            p = (float)cur_byte / (float)max_byte_len;
            _owner.OnProgress(p, cur_byte, max_byte_len, user_data);
        }


        public long GetAllFileLength()
        {
            long size = 0;
            foreach (var kv in queue_file_list)
            {
                foreach (var loader in kv.Value)
                {
                    size += loader.Length;
                }
            }
            return size;
        }

#endregion

#region constructors
        
#endregion
    }
}
#endif