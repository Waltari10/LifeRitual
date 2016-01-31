using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ElderlyGameLogic : MonoBehaviour {

    Pot pot;

    // Use this for initialization
    void Start()
    {
        pot = GameObject.Find("BonsaiPot_FTexturing").GetComponent<Pot>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Door")
        {
            if (pot.soil && pot.seed && pot.water)
            {
                SceneManager.LoadScene("EndScene");
            }
        }
    }
}
