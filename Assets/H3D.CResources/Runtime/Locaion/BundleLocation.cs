using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H3D.CResources
{
    internal class BundleLocation : IResourceLocation
    {
        string m_id;
        string m_providerId;
        List<IResourceLocation> m_dependencies;
        public string InternalId { get { return m_id; } }
        public string ProviderId { get { return m_providerId; } }
        public IList<IResourceLocation> Dependencies { get { return m_dependencies; } }
        public bool HasDependencies { get { return m_dependencies != null && m_dependencies.Count > 0; } }
        public BundleLocation(string id, string providerId, params IResourceLocation[] dependencies)
        {
            if (string.IsNullOrEmpty(id))
                throw new System.ArgumentNullException(id);
            if (string.IsNullOrEmpty(providerId))
                throw new System.ArgumentNullException(providerId);
            m_id = id;
            m_providerId = providerId;
            m_dependencies = new List<IResourceLocation>(dependencies);
        }
        public void AddDependencies(IResourceLocation[] dependencies)
        {
            m_dependencies = new List<IResourceLocation>(dependencies);
        }


    }
}
