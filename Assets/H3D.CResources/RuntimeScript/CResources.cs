using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class CResources
{
    static List<ICResourcesLocator> m_ResourceLocator;
    static CResources()
    {

    }

   

    public static T Load<T>(string requestID) where T: Object
    {
        
        return null;
    }
}
public interface ICResourcesLocator
{
    ICResourcesLocation GetLocation(string requestID);
}

public class ICResourcesLocation
{

}

public class ICResourceProvider
{

}