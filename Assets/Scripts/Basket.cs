using UnityEngine;
using System.Collections;

public class Basket : MonoBehaviour {

    public int itemCount = 0;
    public bool levelComplete = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("trigger");
        itemCount++;

        if (itemCount > 3)
        {
            levelComplete = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        itemCount--;
    }
}
