using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class paivakirjaYoungAdult : MonoBehaviour {

    Canvas paivakirjaGO;

    // Use this for initialization
    void Start()
    {
        paivakirjaGO = GameObject.Find("paivakirja").GetComponent<Canvas>();
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            paivakirjaGO.enabled = false;
        }
    }
}
