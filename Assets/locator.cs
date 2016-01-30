using UnityEngine;
using System.Collections;

public class locator : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log(gameObject.transform.position);
        Debug.Log(gameObject.transform.localPosition);
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
