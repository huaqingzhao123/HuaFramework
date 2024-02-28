using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text;
using System;

namespace Nireus
{
	public class HttpService : SingletonBehaviour<HttpService>
	{
	    public const int TIME_OUT = 5;

        public System.Action on_start_rpc_callback;
        public System.Action on_end_rpc_callback;
        public delegate void HttpStringCallback(string result);
        public delegate void HttpBytesCallback(byte[] result);
        public delegate void DownloadImageCallback(Texture2D texture);

		public void get(string url, HttpStringCallback http_callback,int time_out = TIME_OUT)
		{
            if (on_start_rpc_callback != null)
                on_start_rpc_callback();
            
            GameDebug.Log("HttpService get url = " + url +",callback =" + http_callback.ToString());
            HttpStringCallback callback = (result) =>
            {
                http_callback(result);
                if (on_end_rpc_callback != null)
                    on_end_rpc_callback();
            };
			StartCoroutine(getImpl(url, callback, time_out));
		}

        public void post(string url, Dictionary<string, object> post_data)
        {
            if (on_start_rpc_callback != null)
                on_start_rpc_callback();

            HttpStringCallback callback = (result) =>
            {
                if (on_end_rpc_callback != null)
                    on_end_rpc_callback();
            };
            StartCoroutine(postImpl(url, post_data, callback));
        }

        public void post(string url, Dictionary<string, object> post_data, HttpStringCallback http_callback)
        {
            if (on_start_rpc_callback != null)
                on_start_rpc_callback();

            HttpStringCallback callback = (result) =>
            {
                http_callback(result);
                if (on_end_rpc_callback != null)
                    on_end_rpc_callback();
            };
            StartCoroutine(postImpl(url, post_data, callback));
		}

		public void uploadImage(string url, Texture2D image, string file_name, string file_type, HttpStringCallback http_callback)
		{
			StartCoroutine(uploadImageImpl(url, image, file_name, file_type, http_callback));
		}

		public void downloadImage(string url, DownloadImageCallback download_image_callback)
		{
			StartCoroutine(url, download_image_callback);
		}

		private IEnumerator getImpl(string url, HttpStringCallback http_callback,int time_out)
		{
			var www = UnityWebRequest.Get(url);
            www.timeout = time_out;
            yield return www.SendWebRequest();
            //LoadingBar.getInstance().realHide();
			if (www.isHttpError || www.isNetworkError)
			{
				GameDebug.Log("get url: " + url + " error: " + www.error);
				http_callback(null);
				yield return null;
			}
			else
			{
				//GameDebug.Log("HttpService getImpl text = " + www.downloadHandler.text);
				http_callback(www.downloadHandler.text);
			}
		}

		private IEnumerator postImpl(string url, Dictionary<string, object> post_data, HttpStringCallback http_callback)
		{
			WWWForm form = new WWWForm();
			foreach (KeyValuePair<string, object> post_arg in post_data)
			{
				form.AddField(post_arg.Key, post_arg.Value.ToString());
			}

            var www = UnityWebRequest.Post(url, form);
		    www.timeout = TIME_OUT;
            yield return www.SendWebRequest();

			if (www.isHttpError || www.isNetworkError)
			{
				GameDebug.Log("post url: " + url + " error: " + www.error);
				http_callback(null);
				yield return null;
			}
			else
			{
			    var resultText = Encoding.UTF8.GetString(www.downloadHandler.data);
			    http_callback?.Invoke(resultText);
			}
		}


        public void post(string url, Dictionary<string, object> post_data, HttpBytesCallback http_callback)
        {
            if (on_start_rpc_callback != null)
                on_start_rpc_callback();

            HttpBytesCallback callback = (result) =>
            {
                http_callback(result);
                if (on_end_rpc_callback != null)
                    on_end_rpc_callback();
            };
            StartCoroutine(postImpl_bytes(url, post_data, callback));
        }

        private IEnumerator postImpl_bytes(string url, Dictionary<string, object> post_data, HttpBytesCallback http_callback)
        {
            WWWForm form = new WWWForm();
            foreach (KeyValuePair<string, object> post_arg in post_data)
            {
                form.AddField(post_arg.Key, post_arg.Value.ToString());
            }

            var www = UnityWebRequest.Post(url, form);
            www.timeout = TIME_OUT;
            yield return www.SendWebRequest();

            if (www.isHttpError || www.isNetworkError)
            {
                GameDebug.Log("post url: " + url + " error: " + www.error);
                http_callback(null);
                yield return null;
            }
            else
            {
                http_callback(www.downloadHandler.data);
            }
        }

        public void Post(string url, Dictionary<string, object> post_data, Action<byte[]> success_callback, Action<string> fail_callback)
        {
            on_start_rpc_callback?.Invoke();
            StartCoroutine(postImpl(url, post_data, success_callback, fail_callback));
        }

        private IEnumerator postImpl(string url, Dictionary<string, object> post_data, Action<byte[]> success_callback, Action<string> fail_callback)
        {
            WWWForm form = new WWWForm();
            foreach (KeyValuePair<string, object> post_arg in post_data)
            {
                form.AddField(post_arg.Key, post_arg.Value.ToString());
            }

            var www = UnityWebRequest.Post(url, form);
            www.timeout = TIME_OUT;
            yield return www.SendWebRequest();

            if (www.isHttpError || www.isNetworkError)
            {
                GameDebug.Log("post url: " + url + " error: " + www.error);
                fail_callback?.Invoke(www.error);
                yield return null;
            }
            else
            {
                success_callback?.Invoke(www.downloadHandler.data);
            }
        }

        public void Post(string url, WWWForm wwwForm, Action<string> success_callback, Action<string> fail_callback)
        {
            on_start_rpc_callback?.Invoke();
            StartCoroutine(postImpl(url, wwwForm, success_callback, fail_callback));
        }

        private IEnumerator postImpl(string url, WWWForm wwwForm, Action<string> success_callback, Action<string> fail_callback)
        {
            var www = UnityWebRequest.Post(url, wwwForm);
            www.timeout = TIME_OUT;
            yield return www.SendWebRequest();

            if (www.isHttpError || www.isNetworkError)
            {
                GameDebug.Log("post url: " + url + " error: " + www.error);
                fail_callback?.Invoke(www.error);
                yield return null;
            }
            else
            {
                var resultText = Encoding.UTF8.GetString(www.downloadHandler.data);
                success_callback?.Invoke(resultText);
            }
        }

        private IEnumerator uploadImageImpl(string url, Texture2D image, string file_name, string file_type, HttpStringCallback http_callback)
		{
			WWWForm form = new WWWForm();
			form.AddBinaryData("picture", image.EncodeToPNG(), file_name, file_type);

            var www = UnityWebRequest.Post(url, form);
            yield return www.SendWebRequest();

            if (www.isHttpError || www.isNetworkError)
			{
				GameDebug.Log("upload image url: " + url + " error: " + www.error);
				http_callback(null);
				yield return null;
			}
			else
			{
				http_callback("");
			}
		}

		private IEnumerator downloadImageImpl(string url, DownloadImageCallback download_image_callback)
		{
			var www = UnityWebRequestTexture.GetTexture(url);
			yield return www.SendWebRequest();

			if (www.isHttpError || www.isNetworkError)
			{
				GameDebug.Log("download image url: " + url + " error: " + www.error);
				download_image_callback(null);
				yield return null;
			}
			else
			{
				download_image_callback(DownloadHandlerTexture.GetContent(www));
			}
		}

    }
}