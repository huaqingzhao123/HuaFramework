using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.MessageCenter
{
    public class MessageCenter
    {

        private static Dictionary<string, Action> _messagesData = new Dictionary<string, Action>();

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="action"></param>
        public static void RegisterMessage(string messageName, Action action)
        {
            if (!_messagesData.ContainsKey(messageName))
                _messagesData.Add(messageName, delegate { });
            _messagesData[messageName] += action;
        }

        /// <summary>
        /// 移除指定消息的指定方法
        /// </summary>
        public static void UnRegisterExpactMessage(string messageName, Action action)
        {
            if (!_messagesData.ContainsKey(messageName)) return;
            _messagesData[messageName] -= action;
        }


        /// <summary>
        /// 移除指定消息的指定方法
        /// </summary>
        public static void UnRegisterSelectedAllMessage(string messageName)
        {
            if (!_messagesData.ContainsKey(messageName)) return;
            _messagesData.Remove(messageName);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="messageName"></param>
        public static void SendMessage(string messageName)
        {
            if (!_messagesData.ContainsKey(messageName)) return;
            if (_messagesData[messageName] != null)
                _messagesData[messageName].Invoke();
        }

    }



}
