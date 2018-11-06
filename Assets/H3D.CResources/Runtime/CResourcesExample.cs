using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using H3D.CResources;
public class CResourcesExample : MonoBehaviour
{

    // Use this for initialization
    IEnumerator Start()
    {


        CResourceRequest<GameObject> request = CResources.LoadAsync<GameObject>("a/cube");
        request.Completed += (op) =>
        {
            LogUtility.Log("MOJ " + request.Content);

            CResources.Destroy(request.Content);
        };
        //var request2 = CResources.LoadAsync<GameObject>("a/cube");
        //request2.Completed += (p) =>
        //{
        //    LogUtility.Log("MOJ " + request2.Content);

        //    CResources.Destroy(request2.Content);
        //};

        var request1  = CResources.Load<GameObject>("a/cube");
        LogUtility.Log(request1.Content);
        while(!request1.IsDone)
        {
            yield return null;
        }
        LogUtility.Log(request1.Content);
  
        yield break;
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

            CResources.UnloadUnusedAssets().completed+=(p)=>
            {
                

            };
         
        }


        if (GUILayout.Button("Load"))
        {
           
        }

       
    }
}
