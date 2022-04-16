using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.Architecture
{

    public interface ICanSendCommond :IBelongToArchitecture
    {

    }
    public static class SendCommondExtension
    {
        public static void SendCommond<T>(this ICanSendCommond self) where T : ICommond,new()
        {
            self.GetArchitecture().SendCommond<T>();
        }
        //public static void SendCommond<T>(this ICanSendCommond self, object obj) where T : ICommond, new()
        //{
        //    self.GetArchitecture().SendCommond<T>(obj);
        //}
        //public static void SendCommond<T>(this ICanSendCommond self, params object[] obj) where T : ICommond, new()
        //{
        //    self.GetArchitecture().SendCommond<T>(obj);
        //}
        public static void SendCommond<T>(this ICanSendCommond self,T commond) where T : ICommond
        {
            self.GetArchitecture().SendCommond<T>(commond);
        }
    }
}
