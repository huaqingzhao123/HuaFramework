using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nireus
{
    //定时器回调函数
    public delegate void SchedulerCallback();

    public class SchedulerData
    {
        private float _startDelay;        //多少秒后启动
        private int _runCount;               //运行次数
        private float _timeInterval;         //每两次运行的时间间隔
        private Action _action; //回调

        //private bool isPause = false;
        public IEnumerator coroutine;

        public SchedulerData(float startDelay = 0, int runCount = 0, float timeInterval = 0f, Action action = null)
        {
            _startDelay = startDelay;
            _runCount = runCount;
            _timeInterval = timeInterval;
            _action = action;
        }

        public IEnumerator RunFunction()
        {
            yield return new WaitForSeconds(_startDelay);
            int count = 0;
            while (true)
            {
                if (_runCount > 0 && count >= _runCount)
                {
                    Scheduler.Instance.Remove(this);
                    break;
                }

                //if (isPause) continue;

                try
                {
                    _action?.Invoke();
                }
                catch (Exception e)
                {
                    GameDebug.LogError(e);
                }

                count++;

                if (_timeInterval <= 0f)
                {
                    yield return null; //下一帧继续
                }
                else
                {
                    yield return new WaitForSeconds(_timeInterval); //_timeInterval秒后继续
                }
            }
        }
    }

    public class Scheduler : SingletonBehaviour<Scheduler>
    {
        private Dictionary<string, SchedulerData> schedulers = new Dictionary<string, SchedulerData>();

        /// <summary>
        /// Stop and remove a scheduler.
        /// </summary>
        /// <param name="name">scheduler key name</param>
        public void Remove(string name)
        {
            SchedulerData scheduler;
            if (schedulers.TryGetValue(name, out scheduler))
            {
                if (null != scheduler.coroutine)
                {
                    StopCoroutine(scheduler.coroutine);
                }
                //移除
                schedulers.Remove(name);
            }
        }

        public void Remove(SchedulerData schedulerData)
        {
            foreach (var pair in schedulers)
            {
                if (pair.Value == schedulerData)
                {
                    Remove(pair.Key);
                    break;
                }
            }
        }

        public void RemoveAll()
        {
            foreach(var kv in schedulers)
            {
                var scheduler = kv.Value;
                if (null != scheduler.coroutine)
                {
                    StopCoroutine(scheduler.coroutine);
                }
            }

            schedulers.Clear();
        }

        /// <summary>
        /// Stop and remove a scheduler.
        /// </summary>
        /// <param name="name">scheduler key name</param>
        public void Unregister(string name)
        {
            Remove(name);
        }

        /// <summary>
        /// Create a scheduler
        /// </summary>
        /// <param name="name">scheduler key name</param>
        /// <param name="action">the action</param>
        /// <param name="startDelay">delay time</param>
        /// <param name="runCount">the times that the action will run. (0 means infinity)</param>
        /// <param name="timeInterval">The time interval of action running. (0 means that action run once every frame)</param>
        /// <returns></returns>
        public SchedulerData CreateScheduler(string name, Action action, float startDelay = 0, int runCount = 0, float timeInterval = 0f)
        {
            if (string.IsNullOrEmpty(name))
            {
                GameDebug.LogError("[Scheduler.CreateScheduler] The name is illegal.");
                return null;
            }

            SchedulerData scheduler = new SchedulerData(startDelay, runCount, timeInterval, action);
            scheduler.coroutine = scheduler.RunFunction();
            StartCoroutine(scheduler.coroutine);
            if (schedulers.ContainsKey(name))
            {
                //关闭同名定时器
                Remove(name);
                GameDebug.LogError("Create Same name Scheduler!!!");
            }
            //保存
            schedulers.Add(name, scheduler);
            return scheduler;
        }

        public void RegisterSecond(string name, Action action)
        {
            CreateScheduler(name, action, 0f, 0, 1f);
        }

        public void RegisterUpdate(string name, Action action)
        {
            CreateScheduler(name, action);
        }
    }
}
