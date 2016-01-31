using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour {

    Button playButton;

	// Use this for initialization
	void Start () {
        playButton = GameObject.Find("Button (1)").GetComponent<Button>();

        playButton.onClick.AddListener(() => Play());
    }

    void Play()
    {
        SceneManager.LoadScene("Child_Room");

    }
}
