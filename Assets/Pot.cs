using UnityEngine;
using System.Collections;

public class Pot : MonoBehaviour {

    public GameObject soilGO;
    public GameObject hoseGO;
    public GameObject seedHoseGO;
   public bool soil = false;
   public bool water = false;
   public bool seed = false;

	// Use this for initialization
	void Start () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Seed" && soil && water)
        {
            seedHoseGO.SetActive(true);
            seed = true;
            StartCoroutine(disableSeed());
        } else if (other.tag == "Soil") {
            soilGO.GetComponent<MeshRenderer>().enabled = true;
            soil = true;
        } else if (other.tag == "Flask" && soil) {
            hoseGO.SetActive(true);
            water = true;
            StartCoroutine(disableWater());

        }
    }

    IEnumerator disableSeed()
    {
        yield return new WaitForSeconds(2);
        seedHoseGO.SetActive(false);
    }

    IEnumerator disableWater()
    {
        yield return new WaitForSeconds(2);
        hoseGO.SetActive(false);
    }
}
