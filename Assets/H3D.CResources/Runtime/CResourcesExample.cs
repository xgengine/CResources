using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using H3D.CResources;
public class CResourcesExample : MonoBehaviour
{
    // Use this for initialization
    void Start()
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

        var goo1 =  CResources.CreateInstanceAsync<GameObject>("a/cube");
        var goo2 = CResources.CreateInstanceAsync<GameObject>("a/cube");
        var goo3 = CResources.CreateInstanceAsync<GameObject>("a/cube");

        //var request1  = CResources.Load<GameObject>("a/cube");
        //LogUtility.Log(request1.Content);



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
