using Nireus;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nireus
{

    public static class GameCommonGlobal
    {

        public static int CHANNEL_OPPO = 2135;
        public static int CHANNEL_4399 = 2133;
        public static int MJ_SERVER_DISTRIBUTION_GAME_ID = 16;
        public static int DEFAULT_CHANNEL = 2;
        public static readonly List<int> ByteDanceChannelTypeList = new List<int> { 2, 11, 12};
#if UNITY_EDITOR && !OUTER_TEST || INNER_DEVELOP
        public static string requestUrl = "http://127.0.0.1/";
#elif OUTER_TEST
#if OVERSEA
		public static string requestUrl = "https://wsdxtest.wxgame.youxi765.com/";
#else
		public static string requestUrl = "https://wsdxtestcn.wxgame.youxi765.com/";
#endif
        
#else
        public static string requestUrl = "";
#endif

#if INNER_DEVELOP
        public static string checkForceUpgradeUrl { get; private set; } = "https://wsdxcftest.wxgame.youxi765.com/check_force_upgrade.php";
        public static string checkVersionUrl { get; private set; } = "https://wsdxcftest.wxgame.youxi765.com/check_version.php";
        public static string AnncInfoUrl { get; private set; } = "https://wsdxcftest.wxgame.youxi765.com/announcement/get_announcement.php";
#elif OUTER_TEST
			
#if OVERSEA
		public static string checkForceUpgradeUrl { get; private set; } = "https://wsdxcftest.wxgame.youxi765.com/check_force_upgrade.php";
		public static string checkVersionUrl { get; private set; }        = "https://wsdxcftest.wxgame.youxi765.com/check_version.php";
        public static string AnncInfoUrl { get; private set; }    =  "https://wsdxcftest.wxgame.youxi765.com/announcement/get_announcement.php";
#else
		public static string checkForceUpgradeUrl { get; private set; } = "https://wsdxcftestcn.wxgame.youxi765.com/check_force_upgrade.php";
		public static string checkVersionUrl { get; private set; }        = "https://wsdxcftestcn.wxgame.youxi765.com/check_version.php";
		public static string AnncInfoUrl { get; private set; }    =  "https://wsdxcftestcn.wxgame.youxi765.com/announcement/get_announcement.php";
#endif
#else
#if OVERSEA
#if UNITY_IOS
	        public static string checkForceUpgradeUrl { get; private set; } = "https://wsdx.wxgame.youxi765.com/port/ios/check_force_upgrade.php";
	        public static string checkVersionUrl { get; private set; }        = "https://wsdx.wxgame.youxi765.com/port/check_version.php";
	        public static string AnncInfoUrl { get; private set; }    =  "https://wsdx.wxgame.youxi765.com/port/announcement/get_announcement.php";
#else
        public static string checkForceUpgradeUrl { get; private set; } = "https://wsdx.wxgame.youxi765.com/port/check_force_upgrade.php";
	        public static string checkVersionUrl { get; private set; }        = "https://wsdx.wxgame.youxi765.com/port/check_version.php";
	        public static string AnncInfoUrl { get; private set; }    = "https://wsdx.wxgame.youxi765.com/port/announcement/get_announcement.php";
#endif
#else
#if UNITY_IOS
	        public static string checkForceUpgradeUrl { get; private set; } = "https://ws.wxgame.youxi765.com/port/ios/check_force_upgrade.php";
	        public static string checkVersionUrl { get; private set; }        = "https://ws.wxgame.youxi765.com/port/check_version.php";
	        public static string AnncInfoUrl { get; private set; }    =  "https://ws.wxgame.youxi765.com/port/announcement/get_announcement.php";
#else
	        public static string checkForceUpgradeUrl { get; private set; } = "https://ws.wxgame.youxi765.com/port/check_force_upgrade.php";
	        public static string checkVersionUrl { get; private set; }        = "https://ws.wxgame.youxi765.com/port/check_version.php";
	        public static string AnncInfoUrl { get; private set; }    =  "https://ws.wxgame.youxi765.com/port/announcement/get_announcement.php";
#endif
#endif
#endif

    }
}