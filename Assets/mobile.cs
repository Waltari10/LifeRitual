using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class mobile : MonoBehaviour {

    Canvas phoneCanvas;

    // Use this for initialization
    void Start()
    {
        phoneCanvas = GameObject.Find("puhelin2D").GetComponent<Canvas>();

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            phoneCanvas.enabled = false;
        }
    }
}
