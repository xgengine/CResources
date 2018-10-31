using UnityEngine;
using System.Collections;
using System;
namespace H3D.EditorCResources
{
    public class CResourcesException:Exception
    {
        public CResourcesException(string message) : base(message)
        {
        }
    }

    public class NoSupportException : CResourcesException
    {
        public NoSupportException(string message) : base(message)
        {
        }
    }

    public class NullOperationParam : CResourcesException
    {
        public NullOperationParam(string message) : base(message)
        {
        }
    }

}

