using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using H3D.CResources;
public class CResourcesExample : MonoBehaviour
{

    // Use this for initialization
    IEnumerator Start()
    {

        float t = Time.realtimeSinceStartup;
        GameObject obj2 = CResources.Load<GameObject>("A/Cube");

        GameObject obj = CResources.Instantiate(obj2);

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

            Resources.UnloadUnusedAssets();
        }


        if (GUILayout.Button("UnloadUnusedAssets"))
        {

            CResources.UnloadUnusedAssets();
        }


        if (GUILayout.Button("Load"))
        {
            GameObject obj2 = CResources.Load<GameObject>("A/Cube");
        }

       
    }
}
