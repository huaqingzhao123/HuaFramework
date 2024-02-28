using System;
using UnityEngine;

namespace Nireus
{
    class Platform
    {
        public static string getIP()
        {
            var ip_entry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            var addr = ip_entry.AddressList;
            if (addr.Length >= 2)
            {
                return addr[1].ToString();
            }

            return "";
        }

//        public static string getClientVersion()
//        {
//            return Setting.getInstance().baseVersion;
//        }


//        public static string getChannelId()
//        {
//#if U8SDK
//            return U8SDKInterface.Instance.GetCurrChannel() + "_" + U8SDKInterface.Instance.GetUCode();
//#else
//            return Setting.getInstance().channelType + "_0";
//#endif
//        }

        public static string getDeviceType()
        {
#if UNITY_ANDROID
            return "android";
#elif UNITY_IPHONE
			return "ios";
#else
            return "windows";
#endif
        }


        public static string getPackageName() {
#if UNITY_5
            return Application.bundleIdentifier;
#else
            return Application.identifier;
#endif

        }

        // 资源在手机存储卡上的路径，优先会找这个路径，没有将尝试存储到手机自带存储中;
        public static string getResFolder0()
        {
            return "/sdcard/" + getPackageName() + "/res";
        }

        // 资源在手机自带存储中的存储路径，中间的路径要和包名一致;
        public static string getResFolder1()
        {
            return "/data/data/" + getPackageName() + "/res";
        }

        //public static string getDynamicDllFolder()
        //{
        //    return "/data/data/" + getPackageName() + "/dynamic_dll";
        //}
    }
}
