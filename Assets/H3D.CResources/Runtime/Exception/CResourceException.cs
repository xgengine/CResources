using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H3D.CResources
{
    public class CResourcesException : System.Exception
    {
        public CResourcesException(string message) : base(message)
        {
        }
        public override string Message
        {
            get
            {
                return "[ CResourcesException ] " + base.Message;
            }
        }
    }

    public class CanNotLocateExcption : CResourcesException
    {
        public CanNotLocateExcption(string message) : base(message)
        {
        }
        public override string Message
        {
            get
            {
                return "[CanNotLocateExcption] Load Path="+base.Message +", Can not Loacte";
            }
        }
    }

    public class UnknownResourceProviderException : CResourcesException
    {

        public IResourceLocation Location { get; private set; }
        public UnknownResourceProviderException(IResourceLocation location,string message =""):base(message)
        {
            Location = location;
        }
        public override string Message
        {
            get
            {
                return "[UnknownResourceProviderException]" +base.Message + ", Location=" + Location;
            }
        }
    }

    public class ResourceProviderFailedException : CResourcesException
    {
        public IResourceLocation Location { get; private set; }
        public IResourceProvider Provider { get; private set; }
        public ResourceProviderFailedException(IResourceProvider provider, IResourceLocation location ,string message = "") : base(message)
        {
            Provider = provider;
            Location = location;
        }
        public override string Message
        {
            get
            {
                return "[ResourceProviderFailedException]"+base.Message + ", Provider=" + Provider + ", Location=" + Location;
            }
        }
    }
}
