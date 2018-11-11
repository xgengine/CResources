using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace H3D.CResources
{
    public class TaskResult
    {
        public object result;
    }

    public class TaskConst
    {
        public const int INVAILD_TASK_ID = -1;
    }

#if !CONSOLE_CLIENT

    public class Task
    {
        public enum TaskState
        {
            Running,
            Suspend,
            Stop,
        }

        Stack<IEnumerator> m_stack = new Stack<IEnumerator>();
        List<int> m_join_task_list = new List<int>();

        int m_id;
        string m_start_coroutine = "";
        public int Id
        {
            get { return m_id; }
        }

        TaskState m_state = TaskState.Running;
        public Task.TaskState State
        {
            get { return m_state; }
        }

        object m_data;
        public object Data
        {
            get { return m_data; }
            set { m_data = value; }
        }

        public Task(int id, IEnumerator coroutine)
        {
            m_id = id;
            m_stack.Push(coroutine);
            m_start_coroutine = coroutine.ToString();
            {
#if USE_LOGWRAPPER
            LogWrapper.LogDebug("[TaskManager]new Task add to stack:" + id + ",corutine:" + m_start_coroutine);  
#endif
            }
            //      GameHelper.StartCoroutine(Update());

            TaskManager.Instance.StartCoroutine(Update());
        }

        public IEnumerator Suspend()
        {
            if (m_state == TaskState.Running)
            {
                m_state = TaskState.Suspend;
#if USE_LOGWRAPPER
                string log = "[TaskManager][Suspend]:" + LogString();
                LogWrapper.LogDebug(log);  
#endif
                yield return null;
            }
            yield break;
        }

        public void Resume()
        {
            if (m_state == TaskState.Suspend)
            {
                m_state = TaskState.Running;

#if USE_LOGWRAPPER
            string log = "[TaskManager][Resume]:" + LogString();
            LogWrapper.LogDebug(log);
#endif
            }
        }

        public void Stop()
        {
            m_state = TaskState.Stop;

#if USE_LOGWRAPPER
        string log = "[TaskManager][Stop]:" + LogString();
        LogWrapper.LogDebug(log);
#endif
        }

        public IEnumerator Join(int task_id)
        {
            Task task = TaskManager.Instance.FindTask(task_id);
            if (task != null)
            {
                if (task.m_join_task_list == null)
                {
                    task.m_join_task_list = new List<int>();
                }
                task.m_join_task_list.Add(m_id);
                yield return Suspend();
            }
            yield break;
        }

        IEnumerator Update()
        {
            while (State != TaskState.Stop)
            {
                if (State == TaskState.Suspend)
                {
                    yield return null;
                }
                else
                {
                    IEnumerator e = m_stack.Peek();
                    Task last_task = TaskManager.Instance.CurrTask;
                    TaskManager.Instance.CurrTask = this;
                    bool move_next = e.MoveNext();

                    TaskManager.Instance.CurrTask = last_task;
                    if (move_next)
                    {
                        if (e.Current is IEnumerator)
                        {
                            m_stack.Push(e.Current as IEnumerator);

                            continue;
                        }
                        yield return e.Current;
                    }
                    else
                    {
                        //IEnumerator ie = m_stack.Pop();
                        if (m_stack.Count == 0)
                        {
                            Stop();
                        }
                    }
                }
            }
            TaskManager.Instance.RemoveTask(m_id);
            if (m_join_task_list != null)
            {
                for (int i = 0; i < m_join_task_list.Count; i++)
                {
                    int task_id = m_join_task_list[i];
                    TaskManager.Instance.ResumeTask(task_id);
                }
            }
        }

        public string LogString()
        {
            return "task_id:" + m_id + ",status:" + m_state + ",stack_count:" + m_stack.Count + ",join_tasks:" + m_join_task_list.Count + " " + m_start_coroutine + "\n";
        }

    }

    public class TaskManager
#if !CONSOLE_CLIENT
    : MonoBehaviour

#endif
    {
        static TaskManager s_instance = null;

        public static TaskManager Instance
        {
            //   get { return GameImmortalMng.TaskManager; }
            get
            {
                if (s_instance == null)
                {
                    GameObject rMg = GameObject.Find("[TaskManager]");
                    if (rMg != null)
                    {
                        s_instance = rMg.GetComponent<TaskManager>();
                    }
                    else
                    {
                        rMg = new GameObject("[TaskManager]");
                        s_instance = rMg.AddComponent<TaskManager>();
                    }
                }//end by xg
                return s_instance;
            }
        }

        void Awake()
        {
            s_instance = this;
            ///	Debug.Log ("awake");
            Init();
        }


        Dictionary<int, Task> m_tasks = new Dictionary<int, Task>();
        int m_seq = 0;
        Task m_curr_task;
        public Task CurrTask
        {
            get { return m_curr_task; }
            set { m_curr_task = value; }
        }

        public int TaskCount
        {
            get { return m_tasks.Count; }
        }

        public void Init()
        {
            s_instance = this;
        }

        public Task FindTask(int task_id)
        {
            Task task;
            m_tasks.TryGetValue(task_id, out task);
            return task;
        }

        public int StartTask(IEnumerator coroutine)
        {
            if (m_seq < 0)
            {
                m_seq = 0;
            }
            int id = m_seq++;

#if USE_LOGWRAPPER
        if (id % 3  == 0)
        {
            string log = "[TaskManager][StartTask]CurTaskCount:" + m_tasks.Count;
            var node = m_tasks.Begin();
            for (; node != null; node = node.Next)
            {
                log += node.Value.value.LogString();
            }
            LogWrapper.LogDebug(log);
        }
            
#endif

            Task task = new Task(id, coroutine);
            if (task.State != Task.TaskState.Stop)
            {
                m_tasks[id] = task;
                return id;
            }
            return -1;
        }

        public void RemoveTask(int task_id)
        {
            m_tasks.Remove(task_id);

#if USE_LOGWRAPPER
        string log = "[TaskManager][RemoveTask]:" + task_id;
        LogWrapper.LogDebug(log);
#endif
        }

        public void StopTask(int task_id)
        {
            Task task = FindTask(task_id);
            if (task != null)
            {
                task.Stop();
            }
        }

        public void SuspendTask(int task_id)
        {
            Task task = FindTask(task_id);
            if (task != null)
            {
                task.Suspend();
            }
        }

        public void ResumeTask(int task_id)
        {
            Task task = FindTask(task_id);
            if (task != null)
            {
                task.Resume();
            }
        }

    }

#else



public class Task
{
    public enum TaskState
    {
        Running,
        Suspend,
        Stop,
    }

    //IEnumerator m_coroutine;
    Stack<IEnumerator> m_stack = new Stack<IEnumerator>();
    List<int> m_join_task_list;

    int m_id;
    public int Id
    {
        get { return m_id; }
    }

    TaskState m_state;
    public Task.TaskState State
    {
        get { return m_state; }
    }

    object m_data;
    public object Data
    {
        get { return m_data; }
        set { m_data = value; }
    }

    TaskManager m_taskMngr = null;
    public Task(int id, IEnumerator coroutine, TaskManager mngr)
    {
        m_taskMngr = mngr;
        m_id = id;
        m_stack.Push(coroutine);
    }

    public int Suspend()
    {
        if (m_state == TaskState.Running)
        {
            m_state = TaskState.Suspend;
        }
        return 0;
    }

    public void Resume()
    {
        if (m_state == TaskState.Suspend)
        {
            m_state = TaskState.Running;
        }
    }

    public void Stop()
    {
        m_state = TaskState.Stop;
    }

    public IEnumerator Join(int task_id)
    {
        Task task = m_taskMngr.FindTask(task_id);
        if (task != null)
        {
            if (task.m_join_task_list == null)
            {
                task.m_join_task_list = new List<int>();
            }
            task.m_join_task_list.Add(m_id);
            yield return Suspend();
        }
        yield break;
    }

    public void Update()
    {
        while (State != TaskState.Stop)
        {
            if (State == TaskState.Suspend)
            {
                return;
            }
            else
            {
                IEnumerator e = m_stack.Peek();
                Task last_task = m_taskMngr.SetCurrentTask(this);
                bool move_next = e.MoveNext();
                m_taskMngr.SetCurrentTask(last_task);
                if (move_next)
                {
                    if (e.Current is IEnumerator)
                    {
                        m_stack.Push(e.Current as IEnumerator);
                        continue;
                    }
                    return;
                }
                else
                {
                    if (m_stack.Count != 0) 
                    {
                        m_stack.Pop();
                    }
                    
                    if (m_stack.Count == 0)
                    {
                        Stop();
                    }
                }
            }
        }
        m_taskMngr.RemoveTask(m_id);
        if (m_join_task_list != null)
        {
            for (int i = 0; i < m_join_task_list.Count;i++ )
            {
                int task_id = m_join_task_list[i];
                m_taskMngr.ResumeTask(task_id);
            }
        }
    }
}

public class TaskManager
{
    public delegate void StartTaskCallback(int ntaskid);
    public delegate void RemoveStartCallback(int ntaskid);
    public event StartTaskCallback OnStartTask = null;
    public event RemoveStartCallback OnRemoveTask = null;
    static TaskManager Instance
    {
        get { return GameGlobal.TaskManager; }
    }

    public void Init()
    {
    }

    public void Update()
    {
        int i = m_tasks.Count - 1;
        if (i >= 0)
        {
            m_tasks[i].Value.Update();
        }
    }

    List<KeyValuePair<int, Task>> m_tasks = new List<KeyValuePair<int, Task>>();
    int m_seq = 0;
    Task m_curr_task;
    public Task CurrTask
    {
        get { return m_curr_task; }
        set { m_curr_task = value; }
    }

    public Task SetCurrentTask(Task newTask)
    {
        Task oldtask = m_curr_task;
        m_curr_task = newTask;
        return oldtask;
    }
    public int TaskCount
    {
        get { return m_tasks.Count; }
    }

    public Task FindTask(int task_id)
    {
        for (int i = 0; i < m_tasks.Count; ++i)
        {
            if (m_tasks[i].Key == task_id)
            {
                return m_tasks[i].Value;
            }
        }
        return null;
    }

    public int StartTask(IEnumerator coroutine)
    {
        int id = m_seq++;
        var pair = new KeyValuePair<int, Task>(id, new Task(id, coroutine, this));
        m_tasks.Add(pair);
        if (null != OnStartTask) 
        {
            OnStartTask(id);
        }
        pair.Value.Update();
        return id;
    }

    public void RemoveTask(int task_id)
    {
        for (int i = 0; i < m_tasks.Count; ++i)
        {
            if (m_tasks[i].Key == task_id)
            {               
                m_tasks.RemoveAt(i);
                if (null != OnRemoveTask)
                {
                    OnRemoveTask(task_id);
                }
                break;
            }
        }
    }

    public void StopTask(int task_id)
    {
        Task task = FindTask(task_id);
        if (task != null)
        {
            task.Stop();
        }
    }

    public void SuspendTask(int task_id)
    {
        Task task = FindTask(task_id);
        if (task != null)
        {
            task.Suspend();
        }
    }

    public void ResumeTask(int task_id)
    {
        Task task = FindTask(task_id);
        if (task != null)
        {
            task.Resume();
        }
    }
}

#endif

}
