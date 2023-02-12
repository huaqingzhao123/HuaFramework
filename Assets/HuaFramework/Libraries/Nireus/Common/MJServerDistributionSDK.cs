using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Nireus
{
    public class MJServerDistributionSDK : Singleton<MJServerDistributionSDK>
    {
        private string mj_sd_sdk_version = "1.1.0";
        private int mj_sd_game_id = 0;
        private int mj_sd_plat_id = 0;
        private int mj_sd_channel_type = 0;
        private bool mj_isinited = false;
        private List<string> mj_getserver_hosts = new List<string>();
        private List<string> mj_ly_getserver_hosts = new List<string>();
        private List<string> mj_monitorgetserver_hosts = new List<string>();
        private int mj_getserver_retry_limit_new = 30;
        private string cache_monitorfailreq_key = "sdk_cache_monitorfailreq";
        public Action<object> OnGetServerCallback { get; set; }
        public Action<object> OnGetServerFailCallback { get; set; }
        private delegate void CallFuncHttpGet(int status, JObject response, JObject cb_param);

        private bool isFirst = true;

        

    public bool IsByteDanceChannel(int channel)
    {
        return GameCommonGlobal.ByteDanceChannelTypeList.Contains(channel);
    }
    public bool IsByteDanceChannel()
    {
        //var channel_str = PlatformHelper.GetU8Channel();
        //if (int.TryParse(channel_str,out int channel))
        //{
        //    return GameCommonGlobal.ByteDanceChannelTypeList.Contains(channel);
        //}
        return false;
    }
    public bool Is4399Channel()
    {
        //var channel_str = PlatformHelper.GetU8Channel();
        //if (int.TryParse(channel_str,out int channel))
        //{
        //    return channel == GameCommonGlobal.CHANNEL_4399;
        //}
        return false;
    }
    public bool IsOppoChannel()
    {
        //var channel_str = PlatformHelper.GetU8Channel();
        //if (int.TryParse(channel_str,out int channel))
        //{
        //    return channel == GameCommonGlobal.CHANNEL_OPPO;
        //}
        return false;
    }
    public MJServerDistributionSDK()
    {
#if OVERSEA
#if UNITY_IOS
           mj_getserver_hosts.Add("https://centerhk.wx.m-co.cn");
           mj_monitorgetserver_hosts.Add("https://centerloghk.wx.m-co.cn");
#else
            mj_getserver_hosts.Add("https://centerhk.wxgame.youxi765.com");
        	mj_monitorgetserver_hosts.Add("https://centerloghk.wx.m-co.cn");
#endif
#else
#if UNITY_IOS
           mj_getserver_hosts.Add("https://center.wx.m-co.cn");
           mj_monitorgetserver_hosts.Add("https://centerlog.wx.m-co.cn");
#else
            mj_getserver_hosts.Add("https://center.wxgame.youxi765.com");
            mj_monitorgetserver_hosts.Add("https://centerlog.wx.m-co.cn");
#endif
#endif
        }

        /**
         * 初始化
         * @game_id 运维分配的游戏id
         * @plat_id 平台标识，1表示iOS，2表示安卓
         * @channel_type 渠道类型，0表示头条，其他表示联运渠道号，默认0
         */
        public bool Init(int game_id, int plat_id, int channel_type = 0)
        {
            if (game_id <= 0)
            {
                return false;
            }
            if (plat_id <= 0)
            {
                return false;
            }
        if (!IsByteDanceChannel(channel_type))
            {
                mj_getserver_hosts = mj_ly_getserver_hosts;
            }
            mj_isinited = true;
            mj_sd_game_id = game_id;
            mj_sd_plat_id = plat_id;
	        switch (channel_type)
	        {
	            case 0:
	            case 2:
	                mj_sd_channel_type = 2126;
	                break;

	            case 11:
	            case 12:
	                mj_sd_channel_type = 2127;
	                break;

	            default:
	                mj_sd_channel_type = channel_type;
	                break;
	        }
            ReportCacheFailLog();
            return true;
        }
        public bool GetServer(string user_id, JObject game_param)
        {
            var msg = "";
            if (string.IsNullOrWhiteSpace(user_id))
            {
                return false;
            }

            if (!mj_isinited)
            {
                msg = "no init";
                OnGetServerFailCallback?.Invoke(msg);
                return false;
            }

            var device_brand = "";
            var device_model = "";
            var device_version = "";

            var host = GetHost(0);
            var uri = "/api/game/getserver?";
            var param = "game_id=" + mj_sd_game_id + "&plat_id=" + mj_sd_plat_id + "&version=" + mj_sd_sdk_version + "&ly_platform=" + mj_sd_channel_type;
            param += "&user_id=" + user_id;
            foreach (var pair in game_param)
            {
                param += "&" + pair.Key + "=" + pair.Value.ToString();
                if (pair.Key == "device_brand")
                {
                    device_brand = pair.Value.ToString();
                }
                else if (pair.Key == "device_model")
                {
                    device_model = pair.Value.ToString();
                }
                else if (pair.Key == "device_version")
                {
                    device_version = pair.Value.ToString();
                }
                else if (pair.Key == "acc_id")
                {
                    if(!IsByteDanceChannel() && !string.IsNullOrWhiteSpace(pair.Value.ToString()))
                    {
                        uri = "/api/game/getserver_v160?";
                    }
                }
            }

            var url = host + uri + param;
            var callbackParam = new JObject();
            callbackParam["current_host_key"] = 0;
            callbackParam["current_retry_time"] = 1;
            callbackParam["current_host"] = host;
            callbackParam["req_timeout"] = 2;
            HttpGetServer(user_id, param, game_param, url, callbackParam);
            return true;
        }
        private void HttpGetServer(string user_id, string param, JObject game_param, string url, JObject cb_param)
        {
            CoroutineRunner.Instance.StartCoroutine(HttpGetServerRoutine(user_id, param, game_param, url, cb_param));
        }

        private IEnumerator HttpGetServerRoutine(string user_id, string param, JObject game_param, string url,
            JObject cb_param)
        {
            if (!isFirst)
            {
                var delay = (int)cb_param["req_timeout"];
                yield return new WaitForSeconds(delay * 0.5f);
            }

            isFirst = false;

            HttpService.Instance.get(url, resultContent =>
            {
                if (resultContent != null)
                {
                    try
                    {
                        var resultJsonObj = JsonConvert.DeserializeObject(resultContent) as JObject;
                        GetServerCallback(user_id, param, game_param, 1, resultJsonObj, cb_param);
                        return;
                    }
                    catch (Exception e)
                    {
                        GetServerCallback(user_id, param, game_param, 2, null, cb_param);
                        return;
                    }
                }
                else
                {
                    GetServerCallback(user_id, param, game_param, 3, null, cb_param);
                }
            });
        }

        private void HttpGetReport(string param, JObject data, string url, JObject cb_param)
        {
            HttpService.Instance.get(url, resultContent =>
            {
                if (resultContent != null)
                {
                    try
                    {
                        var resultJsonObj = JsonConvert.DeserializeObject(resultContent) as JObject;
                        ReportCallback(param, data, 1, resultJsonObj, cb_param);
                        return;
                    }
                    catch (Exception e)
                    {
                        ReportCallback(param, data, 2, null, cb_param);
                        return;
                    }
                }
                else
                {
                    ReportCallback(param, data, 3, null, cb_param);
                }
            }, (int)cb_param["req_timeout"]);
        }

        private void GetServerCallback(string user_id, string param, JObject game_param, int status, JObject response, JObject cb_param)
        {
            var device_brand = "";
            var device_model = "";
            var device_version = "";
            var uri = "/api/game/getserver?";
            foreach (var pair in game_param)
            {
                if (pair.Key == "device_brand")
                {
                    device_brand = pair.Value.ToString();
                }
                else if (pair.Key == "device_model")
                {
                    device_model = pair.Value.ToString();
                }
                else if (pair.Key == "device_version")
                {
                    device_version = pair.Value.ToString();
                }
            }
            //var client_time = TimeUtil.now;
            var log_data = new JObject();
            log_data["user_id"] = user_id;
            log_data["server_id"] = "";
            //log_data["client_time"] = client_time;
            log_data["is_success"] = status;
            if (cb_param.TryGetValue("current_retry_time", out var current_retry_time))
            {
                log_data["retry_times"] = (int)current_retry_time;
            }

            log_data["restart_times"] = 0;
            if (cb_param.TryGetValue("current_host", out var current_host))
            {
                log_data["current_domain"] = (string)current_host;
            }
            log_data["request_param"] = param;
            log_data["response_data"] = "";
            log_data["device_brand"] = device_brand;
            log_data["device_model"] = device_model;
            log_data["device_sys_version"] = device_version;
            if (status == 1)
            {
                if (response.TryGetValue("error", out var error) && (int)error == 1000)
                {
                    var data_obj = response["data"];
                    OnGetServerCallback?.Invoke(data_obj);
                    if ((int)current_retry_time > 1)
                    {
                        log_data["server_id"] = data_obj["server_id"];
                        log_data["is_success"] = 1;
                        log_data["response_data"] = response.ToString(Formatting.None);
                        ReportMonitorGetServer(log_data);
                    }
                }
                else
                {
                    JObject next = GetNextReqNew(cb_param);
                    if (next != null)
                    {
                        var url = next["current_host"] + uri + param;
                        HttpGetServer(user_id, param, game_param, url, next);
                    }
                    else
                    {
                        var msg = "req status error1";
                        OnGetServerFailCallback?.Invoke(msg);
                        log_data["is_success"] = -1;
                        log_data["response_data"] = response.ToString(Formatting.None);
                        ReportMonitorGetServer(log_data);
                    }

                }

            }
            else
            {
                var next = GetNextReqNew(cb_param);
                if (next != null)
                {
                    var url = next["current_host"] + uri + param;
                    HttpGetServer(user_id, param, game_param, url, next);
                }
                else
                {
                    var msg = "req status error2";
                    OnGetServerFailCallback?.Invoke(msg);
                    ReportMonitorGetServer(log_data);
                }
            }
        }
        private JObject GetNextReqCommon(List<string> hosts, JObject p, int limit_num)
        {
            var max_key = hosts.Count - 1;
            var next_key = (int)p["current_host_key"];
            if (next_key == max_key)
            {
                next_key = 0;
            }
            else
            {
                next_key = next_key++;
            }
            p["current_host_key"] = next_key;
            p["current_host"] = hosts[next_key];
            if (hosts.Count > next_key)
            {
                int current_retry_time = (int)p["current_retry_time"];
                if (limit_num > current_retry_time)
                {
                    if (current_retry_time >= 6)
                    {
                        p["req_timeout"] = 8;
                    }
                    else if (current_retry_time >= 3)
                    {
                        p["req_timeout"] = 5;
                    }
                    p["current_retry_time"] = ++current_retry_time;
                    return p;
                }
                else
                {
                    return null;
                }
            }

            return null;
        }
        private JObject GetNextReqNew(JObject p)
        {
            var max_key = mj_getserver_hosts.Count - 1;
            var next_key = (int)p["current_host_key"];
            if ((int)p["current_host_key"] == max_key)
            {
                next_key = 0;
            }
            else
            {
                next_key = (int)p["current_host_key"] + 1;
            }
            p["current_host_key"] = next_key;
            p["current_host"] = mj_getserver_hosts[next_key];
            if (mj_getserver_hosts.Count > next_key)
            {
                int current_retry_time = (int)p["current_retry_time"];
                if (mj_getserver_retry_limit_new > current_retry_time)
                {
                    if (current_retry_time >= 15)
                    {
                        p["req_timeout"] = 10;
                    }
                    else if (current_retry_time >= 6)
                    {
                        p["req_timeout"] = 6;
                    }
                    else if (current_retry_time >= 3)
                    {
                        p["req_timeout"] = 3;
                    }
                    p["current_retry_time"] = ++current_retry_time;
                    return p;
                }
                else
                {
                    return null;
                }
            }

            return null;
        }
        private string GetHost(int idx)
        {
            var url = mj_getserver_hosts[idx];
            return url;
        }
        private string GetMonitorHost(int idx)
        {
            var url = mj_monitorgetserver_hosts[idx];
            return url;
        }

        private void ReportCacheFailLog()
        {
            var str = GetMonitorFailReqStorage();
            if (!string.IsNullOrWhiteSpace(str))
            {
                var items = JsonConvert.DeserializeObject(str) as JArray;
                if (items == null) return;
                if (items.Count >= 0)
                {
                    //清空本地缓存
                    RemoveMonitorFailReqStorage();
                    //循环上报，从原数组中删除
                    foreach (var item in items)
                    {
                        JObject value = (JObject)item;
                        if (value.TryGetValue("restart_times", out var restart_times))
                        {
                            value["restart_times"] = (int)restart_times + 1;
                            ReportMonitorGetServer(value);
                            SetMonitorFailReqStorage(null);
                        }
                    }
                }
            }
        }

        private bool ReportMonitorGetServer(JObject data)
        {
            if (!data.TryGetValue("user_id", out var user_id) || string.IsNullOrWhiteSpace((string)user_id))
            {
                return false;
            }

            var host = GetMonitorHost(0);
            var uri = "/log/monitor/getserver?";
            var req_param = new JObject();
            req_param["user_device"] = user_id;
            req_param["plat_id"] = mj_sd_plat_id;
            req_param["game_id"] = mj_sd_game_id;
            req_param["channel_id"] = mj_sd_channel_type;
            req_param["server_sn"] = data["server_id"];
            req_param["sdk_version"] = mj_sd_sdk_version;
            req_param["client_time"] = data["client_time"];
            req_param["is_success"] = data["is_success"];
            req_param["retry_times"] = data["retry_times"];
            req_param["restart_times"] = data["restart_times"];
            req_param["current_domain"] = data["current_domain"];
            req_param["request_param"] = data["request_param"];
            req_param["response_data"] = data["response_data"];
            req_param["device_brand"] = data["device_brand"];
            req_param["device_model"] = data["device_model"];
            req_param["device_sys_version"] = data["device_sys_version"];
            var param = "";
            foreach (var pair in req_param)
            {
                if (param == "")
                {
                    param += pair.Key + "=" + pair.Value;
                }
                else
                {
                    param += "&" + pair.Key + "=" + pair.Value;
                }
            }
            var url = host + uri + param;
            var callbackParam = new JObject();
            callbackParam["current_host_key"] = 0;
            callbackParam["current_retry_time"] = 1;
            callbackParam["current_host"] = host;
            callbackParam["req_timeout"] = 3;
            HttpGetReport(param, data, url, callbackParam);
            return true;
        }
        private void ReportCallback(string param, JObject data, int status, JObject response, JObject cb_param)
        {
            var uri = "/log/monitor/getserver?";
            if (status == 1)
            {
                if (response.TryGetValue("error", out var error) && (int)error == 1000)
                {

                }
                else
                {
                    var next = GetNextReqCommon(mj_monitorgetserver_hosts, cb_param, 10);
                    if (next != null)
                    {
                        var url = next["current_host"] + uri + param;
                        HttpGetReport(param, data, url, next);
                    }
                    else
                    {
                        if ((int)data["restart_times"] < 5)
                        {
                            SetMonitorFailReqStorage(data);
                        }

                        //callback_fail(msg);
                    }

                }

            }
            else
            {
                var next = GetNextReqCommon(mj_monitorgetserver_hosts, cb_param, 10);
                if (next != null)
                {
                    var url = next["current_host"] + uri + param;
                    HttpGetReport(param, data, url, next);
                }
                else
                {
                    if ((int)data["restart_times"] < 5)
                    {
                        SetMonitorFailReqStorage(data);
                    }
                    //callback_fail(msg);
                }
            }
        }
        private void SetMonitorFailReqStorage(JObject data)
        {
            if (data != null)
            {
                var str = GetMonitorFailReqStorage();
                if (!string.IsNullOrWhiteSpace(str))
                {
                    var items = JsonConvert.DeserializeObject(str) as JArray;
                    if (items == null)
                    {
                        items = new JArray();
                        items.Add(data);
                    }
                    else
                    {
                        if (items.Count >= 10)
                        {
                            items.RemoveAt(0);
                        }
                        items.Add(data);
                    }
                    LocalSave.SetString(cache_monitorfailreq_key, items.ToString(Formatting.None));
                }

            }
        }
        private string GetMonitorFailReqStorage()
        {
            return LocalSave.GetString(cache_monitorfailreq_key);
        }
        private void RemoveMonitorFailReqStorage()
        {
            LocalSave.DeleteKey(cache_monitorfailreq_key);
        }
    }
}