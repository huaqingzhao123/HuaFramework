using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Net;
using UnityEngine.Networking;

#if !ASSET_RESOURCES
public class FileDownloader
{
    public string Text { get; private set; } = "";
    public byte[] FileData { get { return _file_data; } }
    public string error_code { get { return _error_code;} }
    public Action<float,int,int> OnProgress { get;set;}
    public Action OnSucc;
    public Action<string> OnError;
    private byte[] _file_data;
    private string _error_code = "";
    private bool _is_stop;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="get_len"></param>
    /// <param name="get_txt">如果只要字节这里应该位false，不然会把字节转txt会卡住</param>
    /// <returns></returns>
    public IEnumerator DownloadByUnity(string url,bool get_len,bool get_txt)
    {
        bool succ = false;
        _error_code = "";
        Text = "";
        byte[] file_data = null;
        _file_data = null;
        int cnt = 0;
        _is_stop = false;
        int len = 1;
        int cur_length = 0;
        if (get_len)
        {
            len = (int)GetLength(url);
        }

        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            uwr.SendWebRequest();//开始请求
            while (!uwr.isDone)
            {
                if (_is_stop)
                {
                    _error_code = "break";
                    break;
                }
                //GameDebug.LogError(www.downloadProgress);
                cur_length = (int)uwr.downloadedBytes;
                OnProgress?.Invoke(uwr.downloadProgress, cur_length, len);
                yield return new WaitForSeconds(0.1f);
            }
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                _error_code = ("file data error"+ uwr.error);
                OnError?.Invoke(_error_code);
            }
            else
            {
                file_data = uwr.downloadHandler.data;
                if (get_txt)
                {
                    Text = uwr.downloadHandler.text;
                }
                OnSucc?.Invoke();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="get_len"></param>
    /// <param name="get_text">如果只要字节这里应该位false，不然会把字节转txt会卡住</param>
    /// <returns></returns>
    public IEnumerator DownloadByWWW(string url,bool get_len,bool get_text)
    {
        
        bool succ = false;
        _error_code = "";
        Text = "";
        byte[] file_data = null;
        _file_data = null;
        int cnt = 0;
        _is_stop = false;
        int len = 1;
        int cur_length = 0;
        if (get_len)
        {
            len = (int)GetLength(url);
        }

        while (true)
        {
            if (++cnt == 3)
            {
                _error_code = $"not find url :{url}";
                //OnError?.Invoke($"not find url :{patch_file_url}");
                succ = false;
                break;
            }

            WWW www = new WWW(url);
            while (!www.isDone)
            {
                if (_is_stop)
                {
                    _error_code = "break";
                    break;
                }
                cur_length = www.bytesDownloaded;
                OnProgress?.Invoke(www.progress, cur_length,len);
                yield return new WaitForSeconds(0.1f);
            }

            if (string.IsNullOrEmpty(www.error))
            {
                file_data = www.bytes;
                if (get_text)
                {
                    Text = www.text;
                }
                www.Dispose();
                www = null;
                succ = true;
                break;
            }
            else
            {
                _error_code = www.error;
                www.Dispose();
                www = null;
            }
        }

        if (file_data == null || file_data.Length <= 0 || succ == false)
        {
            _error_code = ("file data error");
            OnError?.Invoke(_error_code);
            yield break;
        }
        _file_data = file_data;
        OnSucc?.Invoke();
    }


    public long GetLength(string url)
	{	
		HttpWebRequest requet = HttpWebRequest.Create(url)as HttpWebRequest;
		requet.Method ="HEAD";
		HttpWebResponse response = requet.GetResponse()as HttpWebResponse;
		return response.ContentLength;
	}


public IEnumerator DownloadByRequest(string url)
    {
        long len = GetLength(url);


        HttpWebRequest request = (HttpWebRequest)(HttpWebRequest.Create(url));
        request.Method = "GET";
        HttpWebResponse hw = (HttpWebResponse)request.GetResponse();
        Stream stream = hw.GetResponseStream();
        var file_stream = new MemoryStream();
        int file_length = (int)hw.ContentLength;
        int cur_length = 0;
        float progress = 0 ;
        _file_data = null;
        _error_code = "";
        _is_stop = false;

        while (cur_length < file_length)
        {
            if (_is_stop)
            {
                _error_code = "break";
                break;
            }
            byte[] buffer = new byte[1024];
            cur_length += stream.Read(buffer,0,buffer.Length);
            file_stream.Write(buffer,0,buffer.Length);
            progress = (float)cur_length / (float)file_length;
            OnProgress?.Invoke(progress,cur_length,file_length);
            if (cur_length % 102400 == 0)
            {
                yield return null;
            }
        }

        hw.Close();
        stream.Close();
        file_stream.Close();
        if (cur_length != file_length)
        {
            _error_code = ("file data error");
            OnError?.Invoke(_error_code);
        }
        else
        {
            _file_data = file_stream.ToArray();
            OnSucc?.Invoke();
        }
    }

    public void StopDownload()
    {
        _is_stop = true;
    }

}
#endif