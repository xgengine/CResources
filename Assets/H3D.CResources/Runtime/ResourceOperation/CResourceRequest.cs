using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace H3D.CResources
{
    public class CResourceRequest :IAsyncOperation
    {
        protected object m_result;
        protected AsyncOperationStatus m_status;
        protected Exception m_error;
        protected object m_context;
        protected int m_refcount;
        protected bool m_isDone;

        Action<IAsyncOperation> m_completedAction;

        protected CResourceRequest()
        {
        }

        public bool Release()
        {
            m_refcount--;
            return m_refcount == 0;
        }

        public CResourceRequest Retain()
        {
            m_refcount++;
            return this;
        }

        public virtual void ResetStatus()
        {
            m_isDone = false;
            m_status = AsyncOperationStatus.None;
            m_error = null;
            m_context = null;
        }

        event Action<IAsyncOperation> IAsyncOperation.Completed
        {
            add
            {
                if (IsDone)
                    value(this);
                else
                    m_completedAction += value;
            }

            remove
            {
                m_completedAction -= value;
            }
        }

        public object Result
        {
            get
            {
                return m_result;
            }
        }

        public AsyncOperationStatus Status
        {
            get
            {
                return m_status;
            }
            protected set
            {
                m_status = value;
            }
        }

        public Exception OperationException
        {
            get
            {
                return m_error;
            }
        }

        public bool MoveNext()
        {
            return !IsDone;
        }

        public void Reset()
        {
        }

        public object Current
        {
            get
            {
                return Result;
            }
        }

        public virtual bool IsDone
        {
            get
            {
                return m_isDone;
            }
        }

        public virtual float PercentComplete
        {
            get
            {
                return IsDone ? 1f : 0f;
            }
        }

        public object Context
        {
            get
            {
                return m_context;
            }
            protected set
            {
                m_context = value;
            }
        }

        public virtual void InvokeCompletionEvent()
        {
            if (m_completedAction != null)
            {
                var tmpEvent = m_completedAction;
                m_completedAction = null;
                try
                {
                    tmpEvent(this);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    m_error = e;
                    m_status = AsyncOperationStatus.Failed;
                }
            }
        }

        public virtual void SetResult(object result)
        {
            m_result = result;
            m_status = (m_result == null) ? AsyncOperationStatus.Failed : AsyncOperationStatus.Succeeded;
        }
    }

    public class CResourceRequest<T> : CResourceRequest where T : class
    {
        List<Action<CResourceRequest<T>>> m_completedActionT;

        public CResourceRequest()
        {
        }

        public new CResourceRequest<T> Retain()
        {
            m_refcount++;
            return this;
        }

        public event Action<CResourceRequest<T>> Completed
        {
            add
            {
                if (IsDone)
                {
                    value(this);
                }
                else
                {
                    if (m_completedActionT == null)
                        m_completedActionT = new List<Action<CResourceRequest<T>>>(2);
                    m_completedActionT.Add(value);
                }
            }

            remove
            {
                m_completedActionT.Remove(value);
            }
        }

        public new T Result
        {
            get
            {
                return m_result as T;
            }
            set
            {
                m_result = value;
            }
        }

        public override void InvokeCompletionEvent()
        {
            if (m_completedActionT != null)
            {
                for (int i = 0; i < m_completedActionT.Count; i++)
                {
                    try
                    {
                        m_completedActionT[i](this);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        m_error = e;
                        m_status = AsyncOperationStatus.Failed;
                    }
                }
                m_completedActionT.Clear();
            }
            base.InvokeCompletionEvent();
        }

        protected IResourceLocation m_location;

        protected List<CResourceRequest<object>> m_dependencyOperations;

        public CResourceRequest<T> Send(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation, bool isAsync)
        {
            m_location = location;
            m_dependencyOperations = loadDependencyOperation;
            if (isAsync)
            {
                TaskManager.Instance.StartTask(LoadAsyncInternal());
            }
            else
            {
                LoadInternal();
            }
            return this;
        }

        protected IEnumerator LoadAsyncInternal()
        {
            yield return LoadAsync();
            m_isDone = true;
            InvokeCompletionEvent();

        }

        protected void LoadInternal()
        {
            Load();
            m_isDone = true;
        }

        protected virtual IEnumerator LoadAsync()
        {
            yield break;

        }

        protected virtual void Load()
        {

        }

        public void LoadImmediate()
        {
            LoadInternal();
        }
    }

}

