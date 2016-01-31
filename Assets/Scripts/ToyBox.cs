using UnityEngine;
using System.Collections;

public class ToyBox : MonoBehaviour {

    public bool clown = false;
    public bool octopus = false;
    public bool bunny = false;
    public bool complete = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        checkIfBoxComplete();
	}

    void checkIfBoxComplete()
    {
        if (clown && octopus && bunny)
        {
            complete = true;
        }

    }
}
