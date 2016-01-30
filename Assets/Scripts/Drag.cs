using UnityEngine;
using System.Collections;

public class Drag : MonoBehaviour {
    
    bool carrying;
    GameObject carriedObject;
    public float distance;
    public float smooth;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if (carrying)
        {
            carry(carriedObject);
            checkDrop();
        } else
        {
            pickup();
        }
    }

    void carry (GameObject o)
    {
        o.transform.position = Vector3.Lerp (o.transform.position, Camera.main.transform.position + Camera.main.transform.forward * distance, Time.deltaTime * smooth);
        o.transform.rotation = Quaternion.identity;
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
                Pickupable p = hit.collider.GetComponent<Pickupable>();
                if (p != null)
                {
                    carrying = true;
                    carriedObject = p.gameObject;
                    // p.GetComponent<Rigidbody>().isKinematic = true;
                    p.GetComponent<Rigidbody>().useGravity = false;
                    
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
       // carriedObject.GetComponent<Rigidbody>().isKinematic = false;
        carriedObject.GetComponent<Rigidbody>().useGravity = true;
        carriedObject = null;
    }
}
