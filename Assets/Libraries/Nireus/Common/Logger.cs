using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nireus
{

    public class Logger
    {
        private static string log_path;
        private static string BR_STRING = "\r\n";
        public static void Init()
        {
            log_path = Application.persistentDataPath + "/outlog.txt";
            Application.logMessageReceived += LogHandler;
        }

        static void LogHandler(string logString, string stackTrace, LogType logType)
        {
            Log(logString);
            Log(stackTrace);
        }

        public static void Log(string log_str)
        {
            StreamWriter sw = new StreamWriter(log_path, true);
            sw.Write(log_str);
            sw.Write(BR_STRING);
            sw.Close();
        }
        public static void LogFormat(string name, string log_str)
        {
            /*StreamWriter sw = new StreamWriter("C:/qjcj_log/" + name + ".log", true);
            sw.Write(log_str);
            sw.Write(BR_STRING);
            sw.Close();*/
        }
        public static void LogFormat(string name, string log_str, params object[] args)
        {
            /*StreamWriter sw = new StreamWriter("C:/qjcj_log/" + name + ".log", true);
            sw.Write(string.Format(log_str, args));
            sw.Write(BR_STRING);
            sw.Close();*/
        }
    }
}
