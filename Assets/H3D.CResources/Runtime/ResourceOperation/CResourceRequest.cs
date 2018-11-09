﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ResourceManagement
{
    //public class CResourceRequest<TObject> : IAsyncOperation<TObject>
    //{
    //    protected TObject m_result;
    //    protected AsyncOperationStatus m_status;
    //    protected Exception m_error;
    //    protected object m_context;
    //    protected bool m_releaseToCacheOnCompletion = false;
    //    Action<IAsyncOperation> m_completedAction;

    //    List<Action<IAsyncOperation<TObject>>> m_completedActionT;

    //    protected CResourceRequest()
    //    {
    //        IsValid = true;
    //    }

    //    public bool IsValid { get; set; }

    //    public override string ToString()
    //    {
    //        var instId = "";
    //        var or = m_result as Object;
    //        if (or != null)
    //            instId = "(" + or.GetInstanceID().ToString() + ")";
    //        return base.ToString() + " result = " + m_result + instId + ", status = " + m_status + ", Valid = " + IsValid + ", canRelease = " + m_releaseToCacheOnCompletion;
    //    }

    //    public virtual void Release()
    //    {
    //        Validate();
    //        m_releaseToCacheOnCompletion = true;
    //        if (!m_insideCompletionEvent && IsDone)
    //            AsyncOperationCache.Instance.Release(this);
    //    }

    //    public IAsyncOperation<TObject> Retain()
    //    {
    //        Validate();
    //        m_releaseToCacheOnCompletion = false;
    //        return this;
    //    }

    //    public virtual void ResetStatus()
    //    {
    //        m_releaseToCacheOnCompletion = true;
    //        m_status = AsyncOperationStatus.None;
    //        m_error = null;
    //        m_result = default(TObject);
    //        m_context = null;
    //    }

    //    public bool Validate()
    //    {
    //        if (!IsValid)
    //        {
    //            Debug.LogError("INVALID OPERATION STATE: " + this);
    //            return false;
    //        }
    //        return true;
    //    }

    //    public event Action<IAsyncOperation<TObject>> Completed
    //    {
    //        add
    //        {
    //            Validate();
    //            if (IsDone)
    //            {
    //                DelayedActionManager.AddAction(value, 0, this);
    //            }
    //            else
    //            {
    //                if (m_completedActionT == null)
    //                    m_completedActionT = new List<Action<IAsyncOperation<TObject>>>(2);
    //                m_completedActionT.Add(value);
    //            }
    //        }

    //        remove
    //        {
    //            m_completedActionT.Remove(value);
    //        }
    //    }

    //    event Action<IAsyncOperation> IAsyncOperation.Completed
    //    {
    //        add
    //        {
    //            Validate();
    //            if (IsDone)
    //                DelayedActionManager.AddAction(value, 0, this);
    //            else
    //                m_completedAction += value;
    //        }

    //        remove
    //        {
    //            m_completedAction -= value;
    //        }
    //    }

    //    object IAsyncOperation.Result
    //    {
    //        get
    //        {
    //            Validate();
    //            return m_result;
    //        }
    //    }

    //    public AsyncOperationStatus Status
    //    {
    //        get
    //        {
    //            Validate();
    //            return m_status;
    //        }
    //        protected set
    //        {
    //            Validate();
    //            m_status = value;
    //        }
    //    }

    //    public Exception OperationException
    //    {
    //        get
    //        {
    //            Validate();
    //            return m_error;
    //        }
    //    }

    //    public bool MoveNext()
    //    {
    //        Validate();
    //        return !IsDone;
    //    }

    //    public void Reset()
    //    {
    //    }

    //    public object Current
    //    {
    //        get
    //        {
    //            Validate();
    //            return Result;
    //        }
    //    }
    //    public TObject Result
    //    {
    //        get
    //        {
    //            Validate();
    //            return m_result;
    //        }
    //        set
    //        {
    //            Validate();
    //            m_result = value;
    //        }
    //    }
    //    public virtual bool IsDone
    //    {
    //        get
    //        {
    //            Validate();
    //            return !(EqualityComparer<TObject>.Default.Equals(Result, default(TObject)));
    //        }
    //    }
    //    public virtual float PercentComplete
    //    {
    //        get
    //        {
    //            Validate();
    //            return IsDone ? 1f : 0f;
    //        }
    //    }
    //    public object Context
    //    {
    //        get
    //        {
    //            Validate();
    //            return m_context;
    //        }
    //        protected set
    //        {
    //            Validate();
    //            m_context = value;
    //        }
    //    }

    //    bool m_insideCompletionEvent = false;
    //    public void InvokeCompletionEvent()
    //    {
    //        Validate();
    //        m_insideCompletionEvent = true;
    //        if (m_completedActionT != null)
    //        {
    //            for (int i = 0; i < m_completedActionT.Count; i++)
    //            {
    //                try
    //                {
    //                    m_completedActionT[i](this);
    //                }
    //                catch (Exception e)
    //                {
    //                    Debug.LogException(e);
    //                    m_error = e;
    //                    m_status = AsyncOperationStatus.Failed;
    //                }
    //            }
    //            m_completedActionT.Clear();
    //        }

    //        if (m_completedAction != null)
    //        {
    //            var tmpEvent = m_completedAction;
    //            m_completedAction = null;
    //            try
    //            {
    //                tmpEvent(this);
    //            }
    //            catch (Exception e)
    //            {
    //                Debug.LogException(e);
    //                m_error = e;
    //                m_status = AsyncOperationStatus.Failed;
    //            }
    //        }
    //        m_insideCompletionEvent = false;
    //        if (m_releaseToCacheOnCompletion)
    //            AsyncOperationCache.Instance.Release(this);
    //    }

    //    public virtual void SetResult(TObject result)
    //    {
    //        Validate();
    //        m_result = result;
    //        m_status = (m_result == null) ? AsyncOperationStatus.Failed : AsyncOperationStatus.Succeeded;
    //    }

    //}

    //public class EmptyOperation<TObject> : AsyncOperationBase<TObject>
    //{
    //    public virtual IAsyncOperation<TObject> Start(IResourceLocation loc, TObject val)
    //    {
    //        m_context = loc;
    //        SetResult(val);
    //        DelayedActionManager.AddAction((Action)InvokeCompletionEvent, 0);
    //        return this;
    //    }
    //}

    //public class ChainOperation<TObject, TObjectDependency> : AsyncOperationBase<TObject>
    //{
    //    Func<TObjectDependency, IAsyncOperation<TObject>> m_func;
    //    public virtual IAsyncOperation<TObject> Start(IAsyncOperation<TObjectDependency> dependency, Func<TObjectDependency, IAsyncOperation<TObject>> func)
    //    {
    //        m_func = func;
    //        dependency.Completed += OnDependencyCompleted;
    //        return this;
    //    }

    //    private void OnDependencyCompleted(IAsyncOperation<TObjectDependency> op)
    //    {
    //        var funcOp = m_func(op.Result);
    //        m_context = funcOp.Context;
    //        op.Release();
    //        funcOp.Completed += OnFuncCompleted;
    //    }

    //    private void OnFuncCompleted(IAsyncOperation<TObject> op)
    //    {
    //        SetResult(op.Result);
    //        InvokeCompletionEvent();
    //    }
    //}
}

