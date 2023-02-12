/* **************************************************************
* Copyright(c) 2016 Usens Inc, All Rights Reserved.  
* Description	CN 	: 如果本地缓存存在,需要定期更新.或覆盖同名字图片应该更换图片名字.可以按照需求重构下载返回任何数据.
* Description	EN	: 
* Author           	: qnweng
* Created          	: 2016/11/23
* Revision by 		: 
* Revision History 	: 
******************************************************************/
#if !ASSET_RESOURCES && (!UNITY_IOS || USE_SUB_PACKAGE) 
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

namespace Nireus
{

	public class WebFileLoader
	{
#region Public variables

		public string 		Url		{ get { return 	url; } }
		public bool 		IsDone	{ get { return 	isDone; } }
		public State 		CurState{ get { return 	state; } }
        public object       UserData { get { return user_data; } }
        public int          DownloadQueueGroupId { get { return group_id; } }
        public long         Length { get { return file_len; } }
        public int          TimeOut { get; set; } = 30;
        public List<IWebFileObserver> 	ObserversList	{ get { return observers; } }

		public enum State{
			None,
			Start,
			Loading,
			Complate,
			Error,
			Dispose,
		}

#endregion

#region Private variables
		WWW 		www;
        object      user_data;
        string 		url;
		bool 		isDone;
        int         group_id;
        long        file_len;
        State 		state = State.None;
        float       time_out_counter;
        int         old_downloaded;
		List<IWebFileObserver> 				observers = new List<IWebFileObserver>();
		Dictionary<string,Texture2D> 	textureCacheList = new Dictionary<string, Texture2D>();
#endregion

#region Public methods


		public void SetState(State state)
		{
			this.state = state;
		}

		public void UpdateState()
		{
			switch (state)
			{
				case State.None:
					break;
				case State.Start:
					Start();
					state = State.Loading;
					break;
				case State.Loading:
					DispatchProgress(www);
                    if (www.isDone)
                    {
                        LoadDone();
                    }
                    else
                    {
                        if (old_downloaded == www.bytesDownloaded)
                        {
                            time_out_counter += Time.deltaTime;
                            if (time_out_counter > this.TimeOut)
                            {
                                time_out_counter = 0;
                                DispatchErrorEvent("progress time out" + this.url);
                                state = State.Error;
                            }
                        }
                        else
                        {
                            time_out_counter = 0;
                        }
                        old_downloaded = www.bytesDownloaded;
                    }
                    break;
				case State.Error:
					break;
				case State.Dispose:
					Dispose();
					state = State.None;
					break;
			}
		}


		public void AddObserver(IWebFileObserver ob)
		{
			if(observers.Contains(ob) == false)
				observers.Add(ob);
		}

		public void RemoveObserver(IWebFileObserver ob)
		{
			observers.Remove(ob);
		}

		public void RemoveAllObserver()
		{
			observers.Clear();
		}
#endregion

#region Private methods


		void Start()
		{
			isDone = false;
            //string cacheImagePath = cachePath + HashURL + ".png";
            //if (File.Exists(cacheImagePath))
            //{
            //	isLoadFromLocal = true;
            //	www = new WWW("file:///"+cacheImagePath);
            //}
            //else
            //{
            www = new WWW(url);
			//}
		}


		//强制关闭
		void Dispose()
		{
            if (www != null)
			{
				www.Dispose();
				www = null;
				DispatchErrorEvent("cancel");
			}
		}

		void LoadDone()
		{
			isDone = true;
			if (!string.IsNullOrEmpty(www.error))
			{
				DispatchErrorEvent(www.error+","+ this.url);
				state = State.Error;
			}
			else
			{
				DispatchComplate();
				//texture = www.texture;
				state = State.Complate;
				//SaveToLocal(www);
			}
			www = null;
            time_out_counter = 0;
            old_downloaded = 0;
        }


		void DispatchComplate()
		{
            var list = new List<IWebFileObserver>(observers);
			foreach (IWebFileObserver ob in list)
			{
				if (ob != null)
				{
					ob.OnComplate(null, www.bytes,this.url, user_data,this);
				}
			}
		}

		void DispatchErrorEvent(string error)
		{
            var list = new List<IWebFileObserver>(observers);

            foreach (IWebFileObserver ob in list)
			{
				if (ob != null)
				{
					ob.OnError(null, error, user_data);
				}
			}
		}

        //int downloaded_bytes;
        //int prev_downloaded_len;
        //int max_byte_len;


        void DispatchProgress(WWW www)
		{
            int cur_byte = www.bytesDownloaded;
            float progress = www.progress;
            //downloaded_bytes += (cur_byte - prev_downloaded_len);
            //prev_downloaded_len = cur_byte;            
            foreach (IWebFileObserver ob in observers)
			{
				if (ob != null)
				{
					ob.OnProcess(null, progress, cur_byte, user_data);
				}
			}
		}

#endregion

#region constructors

		public WebFileLoader(string url,int group_id, long fiel_len, object user_data)
		{
            this.user_data = user_data;
            this.url = url;
            this.group_id = group_id;
            this.file_len = fiel_len;
            this.SetState(State.None);
		}

#endregion
	}
}
#endif