using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using H3D.CResources;
public class CResourcesExample : MonoBehaviour
{
    // Use this for initialization
    IEnumerator Start()
    {
       // CResourceRequest<GameObject> request = CResources.Load<GameObject>("a/cube");
        //request.Completed += (op) =>
        //{
        //    LogUtility.Log("MOJ " + op.Content.GetInstanceID());
        //    CResources.Destroy(op.Content);
        //};
      
        //var request2 = CResources.Load<GameObject>("a/cube");
        CResourceRequest<GameObject> request3 = CResources.Load<GameObject>("a/cube");
        //request3.Completed += (op) =>
        //{
        //    LogUtility.Log("MOJ " + op.Content.GetInstanceID());
        //    CResources.Destroy(op.Content);
        //};


        //LogUtility.Log("MOJ " + request2.Content.GetInstanceID());

        float t = Time.realtimeSinceStartup;
        var goo1 =  CResources.LoadAsync<GameObject>("a/cube");
        yield return goo1;
        Debug.Log(Time.realtimeSinceStartup - t);
        t = Time.realtimeSinceStartup;
        Instantiate(goo1.Content);


        Debug.Log(Time.realtimeSinceStartup - t);
        t = Time.realtimeSinceStartup;
        var goo2 = CResources.LoadAsync< GameObject>("a/cube");
        yield return goo2;
        Debug.Log(Time.realtimeSinceStartup - t);
        t = Time.realtimeSinceStartup;
        Instantiate(goo2.Content);
        Debug.Log(Time.realtimeSinceStartup - t);
        t = Time.realtimeSinceStartup;
        var goo3 = CResources.LoadAsync<GameObject>("a/cube");
        yield return goo3;
              Debug.Log(Time.realtimeSinceStartup - t);
        //var request1  = CResources.Load<GameObject>("a/cube");
        //LogUtility.Log(request1.Content);
        t = Time.realtimeSinceStartup;
        var goo4 = Resources.LoadAsync<GameObject>("cube");
        Debug.Log(Time.realtimeSinceStartup - t);
        t = Time.realtimeSinceStartup;
        Instantiate(goo1.Content);


        Debug.Log(Time.realtimeSinceStartup - t);
    }
    void Update()
    {
    }

    void OnGUI()
    {
        if(GUILayout.Button("Unload"))
        {

            UnityEngine.SceneManagement.SceneManager.LoadScene("good");
        }


        if (GUILayout.Button("UnloadUnusedAssets"))
        {

            CResources.UnloadUnusedAssets();

         
        }

        if (GUILayout.Button("Load"))
        {
           
        }

       
    }
}
