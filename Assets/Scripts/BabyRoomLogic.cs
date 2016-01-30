using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BabyRoomLogic : MonoBehaviour {

    List<Transform> carriedObjects;
    private bool ToyBoxComplete;

	// Use this for initialization
	void Start () {
        carriedObjects = new List<Transform>();

	}
	
	// Update is called once per frame
	void Update () {
        interact();
    }

    void pickup()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int x = Screen.width / 2;
            int y = Screen.height / 2;

            Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y));

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                carriedObjects.Add(hit.collider.GetComponent<Transform>());
                Pickupable p = hit.collider.GetComponent<Pickupable>();

                if (p != null)
                    hit.collider.GetComponent<Transform>().gameObject.SetActive(false);
            }
        }

    }

    void interact()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int x = Screen.width / 2;
            int y = Screen.height / 2;

            Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y));

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                carriedObjects.Add(hit.collider.GetComponent<Transform>());

                ToyBox t = hit.collider.GetComponent<ToyBox>();

                if (t != null)
                {
                    if (t.bunny == false && listContains("BoxPuzzle_BunnyP"))
                    {
                        t.bunny = true;
                        //(0.0, 0.3, 2.8)
                        getFromList("BoxPuzzle_BunnyP").gameObject.transform.position = new Vector3(-0.016f, 0.3273f, 2.8f);

                        getFromList("BoxPuzzle_BunnyP").gameObject.SetActive(true);
                    }

                    if (t.octopus == false && listContains("BoxPuzzle_OctopusP"))
                    {
                        t.octopus = true;

                        getFromList("BoxPuzzle_OctopusP").gameObject.transform.position = new Vector3(-0.028f, 0.328f, 2.6126f);

                        getFromList("BoxPuzzle_OctopusP").gameObject.SetActive(true);
                    }

                    if (t.clown == false && listContains("BoxPuzzle_ClownP"))
                    {
                        t.clown = true;

                        getFromList("BoxPuzzle_ClownP").gameObject.transform.position = new Vector3(-0.187f, 0.3264f, 2.6399f);

                        getFromList("BoxPuzzle_ClownP").gameObject.SetActive(true);
                    }

                    if (t.complete)
                        ToyBoxComplete = true;
                }
            }
        }
    }


    bool listContains(string str)
    {
        foreach (Transform trs in carriedObjects) {
            if (trs.name == str)
            {
                return true;
            }
        }
        return false;
    }

    Transform getFromList(string str)
    {
        foreach (Transform trs in carriedObjects)
        {
            if (trs.name == str)
            {
                return trs;
            }
        }
        return null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "DoorBaby") { 
            Debug.Log("asdfasdfasdf");
            if (ToyBoxComplete)
            {
                //ChangeScene
            }
         }
        

    }

}
