using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H3D.CResources
{
    internal class DefulatLocator : CResourceLocator
    {
        public override IResourceLocation Locate<T>(object requestID)
        {
            return new LegacyResourcesLocation(requestID as string, typeof(LegacyResourcesProvider).FullName);
        }
    }
}
