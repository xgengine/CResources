using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using H3D.CResources;
using UnityEngine.Profiling;
public class CResourcesExample : MonoBehaviour
{
    // Use this for initialization
    IEnumerator Start()
    {
        yield return 1;

        //float t = Time.realtimeSinceStartup;
        //var goo1 =  CResources.LoadAsync<GameObject>("a/cube");
        //yield return goo1;
        //Debug.Log(Time.realtimeSinceStartup - t);


        //t = Time.realtimeSinceStartup;
        //GameObject instance1 = Instantiate(goo1.Content);
        //Debug.Log(Time.realtimeSinceStartup - t);

        //t = Time.realtimeSinceStartup;
        //var goo2 = CResources.LoadAsync< GameObject>("a/cube");
        //yield return goo2;
        //Debug.Log(Time.realtimeSinceStartup - t);

        //t = Time.realtimeSinceStartup;
        //Instantiate(goo2.Content);
        //Debug.Log(Time.realtimeSinceStartup - t);

        //t = Time.realtimeSinceStartup;
        //var goo3 = CResources.LoadAsync<GameObject>("a/cube");
        //yield return goo3;
        //Debug.Log(Time.realtimeSinceStartup - t);

        //t = Time.realtimeSinceStartup;
        //var goo4 = Resources.LoadAsync<GameObject>("cube");
        //yield return goo4;
        //Debug.Log(Time.realtimeSinceStartup - t);
        //t = Time.realtimeSinceStartup;
        //Instantiate(goo1.Content);

        //Debug.Log(Time.realtimeSinceStartup - t);


        //Material mat2 =CResources.Load<Material>("qq1").Content;
        //Material mat = CResources.Load<Material>("New Material").Content;
        //Debug.LogError(mat2.mainTexture.GetInstanceID());
        /*
        Texture t = mat.mainTexture;
       
        Resources.UnloadAsset(mat);
        Resources.UnloadAsset(t);
        */
        //Resources.UnloadAsset(mat);


        // var request = Resources.LoadAsync<GameObject>("qq");
        // request.completed += (op) =>
        // {
        //      Debug.LogError(op.isDone + " " + op.priority);
        //      Debug.LogError((op as ResourceRequest).asset);
        // };

        //while(!request.isDone)
        //{
        //    Debug.LogError(request.progress);
        //    yield return null;
        //}

        //yield return new WaitForSeconds(2);

        //yield return request;

        //Debug.LogError(request.asset);


        GameObject asset;
        BeginSample();
        asset = CResources.Load<GameObject>("qq");
        EndSample("Load Resource");

        //BeginSample();
        //GameObject objs1 = Instantiate(asset);
        //EndSample("Instantiate Resource");

        CResources.Destroy(asset);

        BeginSample();
        asset = CResources.Load<GameObject>("qqbund");
        EndSample("Load Bundle");
   


        GameObject ins1 = CResources.CreateInstance<GameObject>("qqbund");
        CResources.Destroy(ins1);
        //Debug.LogError("vvv");
        //CResources.Destroy(ins1);

        //BeginSample();
        //GameObject objs2 = Instantiate(asset);
        //objs2.transform.Translate(Vector3.one);
        //EndSample("Instantiate Bundle");


        //Material mats = objs.GetComponent<MeshRenderer>().sharedMaterial;
        //Resources.UnloadAsset(mats);
        //Destroy(objs);
        //var objs1 =Instantiate(asset);
        //objs1.name = "vvvv";

    }
    float t = 0;
    void BeginSample()
    {
        t = Time.realtimeSinceStartup;
    }
    void EndSample(string info="")
    {
        LogUtility.Log("{0} {1}", info, Time.realtimeSinceStartup - t);
    }
    GameObject asset;
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
