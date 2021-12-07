using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HuaFramework.MessageCenter;
namespace HuaFramework.Unity
{
    /// <summary>
    /// 声明为抽象的目的不允许直接挂载只能通过继承使用
    /// </summary>
    public abstract partial class MonoBehaviorSimplify : MonoBehaviour
    {
        public class MessageData
        {
            private static Queue<MessageData> _messageDatasPool = new Queue<MessageData>();
            public string MessageName;
            public Action Callback;
            public static void RecyleMessageData(MessageData messageData)
            {
                _messageDatasPool.Enqueue(messageData);
            }
            public static MessageData SpawnMessageData()
            {
                return _messageDatasPool.Count > 0 ? _messageDatasPool.Dequeue() : new MessageData();
            }
        }
        private List<MessageData> _messages = new List<MessageData>();




        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="action"></param>
        public void RegisterMessage(string messageName, Action action)
        {
            var data = MessageData.SpawnMessageData();
            data.MessageName = messageName;
            data.Callback = action;
            _messages.Add(data);
            MessageCenter.MessageCenter.RegisterMessage(messageName, action);
        }

        /// <summary>
        /// 移除指定消息的指定方法
        /// </summary>
        public void UnRegisterExpactMessage(string messageName, Action action)
        {
            var select = _messages.FindAll(data => data.MessageName == messageName && data.Callback == action);
            select.ForEach(data =>
            {
                MessageData.RecyleMessageData(data);
                _messages.Remove(data);
                MessageCenter.MessageCenter.UnRegisterExpactMessage(messageName, action);
            }
                );
        }


        /// <summary>
        /// 移除指定消息的指定方法
        /// </summary>
        public void UnRegisterSelectedAllMessage(string messageName)
        {
            var select = _messages.FindAll(data => data.MessageName == messageName);
            select.ForEach(data =>
            {
                _messages.Remove(data);
                MessageCenter.MessageCenter.UnRegisterSelectedAllMessage(messageName);
            }
            );
        }



        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="messageName"></param>
        public void NewSendMessage(string messageName)
        {
            MessageCenter.MessageCenter.SendMessage(messageName);
        }


        /// <summary>
        /// 延时方法
        /// </summary>
        /// <param name="time"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        protected void Delay(float time, Action action)
        {
            StartCoroutine(DelayCoroutine(time, action));
        }
        private IEnumerator DelayCoroutine(float time, Action action)
        {
            yield return new WaitForSeconds(time);
            action?.Invoke();
        }
        private void OnDestroy()
        {
            OnBeforeDestroy();
            //清空所有消息数据
            foreach (var item in _messages)
            {
                //回收messagesData
                MessageData.RecyleMessageData(item);
                MessageCenter.MessageCenter.UnRegisterSelectedAllMessage(item.MessageName);
            }
            _messages.Clear();
        }

        /// <summary>
        /// 子类不让重写OnDestroy,而是使用此函数
        /// </summary>
        protected abstract void OnBeforeDestroy();
    }

}
