using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using H3D.CResources;
public class NewBehaviourScript : MonoBehaviour {

	// Use this for initialization
	void Start () {

        CResources.Load<GameObject>("A/Cube");
      
	}
	void Update () {
		
	}
}
