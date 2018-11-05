using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using H3D.CResources;
public class CResourcesExample : MonoBehaviour
{

    // Use this for initialization
    IEnumerator Start()
    {
        GameObject obj2 = CResources.Load<GameObject>("A/Cube");

        GameObject obj = CResources.Instantiate(obj2);


        GameObject request = CResources.CreateInstance<GameObject>("a/cube");

        yield return request;
        LogUtility.Log(request);
        CResources.Destroy(obj2);
        CResources.Destroy(obj);
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
                GameObject obj2 = CResources.Load<GameObject>("A/Cube");

                LogUtility.Log(obj2);
            };
         
        }


        if (GUILayout.Button("Load"))
        {
           
        }

       
    }
}
