using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H3D.CResources
{
    internal class LegacyResourcesLocation : IResourceLocation
    {
        string m_InternalId;
        string m_ProviderId;
        public string InternalId { get { return m_InternalId; } }
        public string ProviderId { get { return m_ProviderId; } }
        public IList<IResourceLocation> Dependencies { get { return null; } }
        public bool HasDependencies { get { return false; } }

        public LegacyResourcesLocation(string internalId, string providerId)
        {
            if (string.IsNullOrEmpty(internalId))
                throw new System.ArgumentNullException(internalId);
            if (string.IsNullOrEmpty(providerId))
                throw new System.ArgumentNullException(providerId);
            m_InternalId = internalId;
            m_ProviderId = providerId;
        }

    }

}
