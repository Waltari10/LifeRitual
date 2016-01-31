using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AdultGameLogic : MonoBehaviour {

    Basket basket;

	// Use this for initialization
	void Start () {
        basket = GameObject.Find("basketCollider").GetComponent<Basket>();
	
	}

    void OnTriggerEnter(Collider other)
    {
        if (basket.levelComplete)
        {
            SceneManager.LoadScene("elderly");
        }
    }
}
