using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using H3D.CResources;
public class NewBehaviourScript : MonoBehaviour {

	// Use this for initialization
	void Start () {

        //GameObject obj =  CResources.Load<GameObject>("A/Cube");

        //Instantiate(obj);
        //LogUtility.Log(obj);
        //Material mat = CResources.Load<Material>("New Material");
        //LogUtility.Log(mat);

        AudioClip clip = CResources.Load<AudioClip>("b/setting 1");
        LogUtility.Log(clip);
        AudioClip clip1 = CResources.Load<AudioClip>("b/setting 1");
        LogUtility.Log(clip1);

        CResources.UnloadUnusedAssets();
    }
	void Update () {
		
	}

    void OnGUI()
    {
        if(GUILayout.Button("vvv"))
        {
            AudioClip clip = CResources.Load<AudioClip>("b/setting 1");
            LogUtility.Log(clip);
            AudioClip clip1 = CResources.Load<AudioClip>("b/setting 1");
            LogUtility.Log(clip1);
        }
    }
}
