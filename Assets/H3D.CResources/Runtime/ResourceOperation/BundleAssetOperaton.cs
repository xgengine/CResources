using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ResourceManagement
{
    public class BundleAssetOperaton<T> : IAsyncOperation
    {
        public AsyncOperationStatus Status
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsValid
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsDone
        {
            get
            {
                return false;
            }
        }

        public float PercentComplete
        {
            get
            {
                return 0;
            }
        }

        public object Context
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public Exception OperationException
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public object Result
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public object Current {
            get
            {
                throw new NotImplementedException();
            }
        }

        public event Action<IAsyncOperation> Completed;

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public void Release()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void ResetStatus()
        {
            throw new NotImplementedException();
        }

        public bool Validate()
        {
            throw new NotImplementedException();
        }
    }
}

