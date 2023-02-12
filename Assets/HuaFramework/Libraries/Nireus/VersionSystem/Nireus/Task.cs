using System;
using System.Collections;
using System.Collections.Generic;
namespace Nireus

{
    public class Task
    {
        private string m_TaskName;
        public string TaskName
        {
            set
            {
                m_TaskName = value;
            }
            get
            {
                return m_TaskName;
            }
        }

        public Action Action;

        public Task(Action work, string taskName = "defaultTaskName")
        {
            this.Action = work;
            this.m_TaskName = taskName;
        }
    }

    public class TaskQueue
    {

        public float TaskProcess
        {
            get
            {
                return 1 - m_TaskQueue.Count * 1.0f / m_TasksNum;
            }
        }

        private int m_TasksNum = 0;

        private LinkedList<Task> m_TaskQueue;

        public TaskQueue()
        {
            m_TaskQueue = new LinkedList<Task>();
            m_TasksNum = 0;
        }

        public void AddTask(Task task)
        {
            m_TaskQueue.AddLast(task);
        }

        public void AddTask(Action action)
        {
            Task task = new Task(action);
            m_TaskQueue.AddLast(task);
        }

        public void RemoveTask(Task task)
        {
            m_TaskQueue.Remove(task);
        }

        public void RemoveTask(string name)
        {
            foreach (var item in m_TaskQueue)
            {
                if (item.TaskName == name)
                {
                    m_TaskQueue.Remove(item);
                    return;
                }
            }
        }


        public void Start()
        {
            m_TasksNum = m_TaskQueue.Count;

            if (OnStart != null)
            {
                OnStart();
            }
            NextTask();
        }

        public void Clear()
        {
            m_TaskQueue.Clear();
            m_TasksNum = 0;
        }

        public Action OnStart = null;

        public Action OnFinish = null;

        private void NextTask()
        {
            if (m_TaskQueue.Count > 0)
            {
                Task task = m_TaskQueue.First.Value;
                m_TaskQueue.RemoveFirst();
                task.Action();
                NextTask();
            }
            else
            {
                if (OnFinish != null)
                {
                    OnFinish();
                }
            }
        }

        public void UpdateNextTask()
        {
            if (m_TaskQueue.Count > 0)
            {
                Task task = m_TaskQueue.First.Value;
                m_TaskQueue.RemoveFirst();
                task.Action();
            }
            else
            {
                if (OnFinish != null)
                {
                    OnFinish();
                }
            }
        }

    }

}