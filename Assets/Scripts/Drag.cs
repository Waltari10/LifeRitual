using UnityEngine;
using System.Collections;

public class Drag : MonoBehaviour {
    
    GameObject[] carriedObjects;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        pickup();
    }

    void pickup()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int x = Screen.width / 2;
            int y = Screen.width / 2;

            Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y));

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                hit.collider.GetComponent<GameObject>();

                ToyBox toyBox = hit.collider.GetComponent<ToyBox>();
                if (toyBox != null)
                {
                    Destroy
                    
                }
            }
        }

    }

    void checkDrop() 
    {
        if (Input.GetMouseButtonDown(0))
            dropObject();
    }

    void dropObject()
    {
        carrying = false;
        carriedObject = null;
    }
}
