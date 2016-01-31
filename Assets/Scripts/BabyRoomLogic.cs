using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

public class BabyRoomLogic : MonoBehaviour {

    List<Transform> carriedObjects;
    private bool ToyBoxComplete;
    public GameObject gameOver;
    private bool octopus;
    private bool clown;
    private bool bunny;
    private bool dead = false;

	// Use this for initialization
	void Start () {
        carriedObjects = new List<Transform>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!dead)
        {
            interact();
        }
    }

    public void interact()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int x = Screen.width / 2;
            int y = Screen.height / 2;

            Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y));

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.distance < 2f)
                {
                    pickup(hit);
                    toybox(hit);
                    powerOutlet(hit);
                    soittoRasia(hit);
                    tutti(hit);
                }
            }
        }
     }

    public void tutti(RaycastHit hit)
    {
        if (hit.collider.GetComponent<Transform>().name == "tutti")
        {
            SoundManager.Instance.PlayPacifierSucking(transform.position);
        }
    }

    public void soittoRasia(RaycastHit hit)
    {
        if (hit.collider.GetComponent<Transform>().name == "soittorasia")
        {
            SoundManager.Instance.PlayTuutuLaulu();
        }

    }

    public void powerOutlet(RaycastHit hit)
    {
        if (hit.collider.GetComponent<Transform>().name == "PowerOutlet")
        {
            Death();
        }

    }

    public void Death()
    {
        gameObject.GetComponent<FirstPersonController>().enabled = false;
        Instantiate(gameOver);
    }

    public void pickup(RaycastHit hit)
    {
        carriedObjects.Add(hit.collider.GetComponent<Transform>());
        Pickupable p = hit.collider.GetComponent<Pickupable>();

        if (p != null)
            hit.collider.GetComponent<Transform>().gameObject.SetActive(false);
    }

    void toybox(RaycastHit hit)
    {
        ToyBox t = hit.collider.GetComponent<ToyBox>();
        if (t != null)
        {
            if (t.bunny == false && listContains("BoxPuzzle_BunnyP"))
            {
                t.bunny = true;
                bunny = true;
                getFromList("BoxPuzzle_BunnyP").gameObject.transform.position = new Vector3(-0.016f, 0.3273f, 2.8f) * 1.5f;
                getFromList("BoxPuzzle_BunnyP").gameObject.SetActive(true);
                Debug.Log("toybox2");
            }

            if (t.octopus == false && listContains("BoxPuzzle_OctopusP"))
            {
                t.octopus = true;
                octopus = true;

                getFromList("BoxPuzzle_OctopusP").gameObject.transform.position = new Vector3(-0.028f, 0.328f, 2.6126f) * 1.5f;

                getFromList("BoxPuzzle_OctopusP").gameObject.SetActive(true);
            }

            if (t.clown == false && listContains("BoxPuzzle_ClownP"))
            {
                t.clown = true;
                clown = true;

                getFromList("BoxPuzzle_ClownP").gameObject.transform.position = new Vector3(-0.187f, 0.3264f, 2.6399f) * 1.5f;

                getFromList("BoxPuzzle_ClownP").gameObject.SetActive(true);
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
        if (other.tag == "Door") { 
            if (octopus && bunny && clown)
            {
                SceneManager.LoadScene("YoungAdult");
            }
         }
    }
}
