using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AdultGameLogic : MonoBehaviour {

    Basket basket;
    public GameObject puhelinCanvas;
    bool phoneActive = false;

	// Use this for initialization
	void Start () {
        basket = GameObject.Find("basketCollider").GetComponent<Basket>();
	
	}

    void Update()
    {
        interact();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            phoneActive = false;
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
                    Debug.Log(hit.collider.GetComponent<Transform>().name);

                    if (hit.collider.GetComponent<Transform>().name == "puhelin" && !phoneActive)
                    {
                        phoneActive = true;

                        Canvas phoneCanvas = GameObject.Find("puhelin2D").GetComponent<Canvas>();
                        phoneCanvas.enabled = true;
                        //Instantiate(puhelinCanvas);
                    }
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("asdfasdf");
        if (basket.levelComplete)
        {
            SceneManager.LoadScene("elderly");
        }
    }
}
