/* **************************************************************
* Copyright(c) 2016 Usens Inc, All Rights Reserved.  
* Description	CN 	: 图片缓存池
* Description	EN	: 
* Author           	: qnweng
* Created          	: #CreateTime#
* Revision by 		: 
* Revision History 	: 
******************************************************************/
#if !ASSET_RESOURCES && (!UNITY_IOS || USE_SUB_PACKAGE) 
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Nireus
{
	public class WebFileCacheList
	{
#region Public variables
#endregion

#region Private variables
		Dictionary<string,RetainObject> list = new Dictionary<string, RetainObject>();
		private long usedMemory;
		private long maxMemory = 100 * (1024 * 1000);							//100M
#endregion

#region Public methods
		public void AddTextureToCache(string hashURL, Texture2D texture)
		{
			RetainObject obj = new RetainObject();
			obj.texture = texture;
			list.Add(hashURL, obj);
			usedMemory += obj.texture.width * obj.texture.height * 2;
			if (usedMemory >= maxMemory)
			{
				GCUnused();
			}
		}

		public void Remove(string url)
		{
			GC(url);
		}

		public Texture2D GetTexture(string hashUrl, bool isRetain)
		{
			RetainObject t = null;
			list.TryGetValue(hashUrl, out t);
			if (t != null)
			{
				t.retainCount++;
				return t.texture;
			}
			return null;
		}

		public void Release(string hashUrl, bool gc = false)
		{
			RetainObject t = null;
			list.TryGetValue(hashUrl, out t);
			if (t != null)
			{
				if (t.retainCount > 0)
				{
					t.retainCount--;
				}
				if (gc && t.retainCount <= 0)
				{
					GC(hashUrl);
				}
			}
		}

		//garbage collection unused resouce
		public void GCUnused()
		{
			List<KeyValuePair<string,RetainObject>> removeList = new List<KeyValuePair<string, RetainObject>>();
			foreach (var kv in list)
			{
				if (kv.Value.retainCount == 0)
				{
					removeList.Add(kv);
				}
			}
			foreach (var kv in removeList)
			{
				GC(kv.Key);
			}
		}

		public void GC(string hashUrl)
		{
			RetainObject t = null;
			list.TryGetValue(hashUrl, out t);
			if (t != null)
			{
				usedMemory -= t.texture.width * t.texture.height * 2;
				t.retainCount = 0;
				t.texture = null;
				t = null;
				GameDebug.Log("GC Finish: " + hashUrl);
				list.Remove(hashUrl);
			}
		}

		public new void ToString()//debug
		{
			foreach (var kv in list)
			{
				RetainObject t = kv.Value;
				GameDebug.Log(kv.Key + " retainCount = " + t.retainCount);
			}
		}
#endregion

#region Private methods
#endregion

#region constructors
		public WebFileCacheList()
		{
		}
#endregion
	}
}
#endif