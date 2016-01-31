using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.SceneManagement;

public class GameLogicYoungAdult : MonoBehaviour {
    
    private int timesCut = 0;
    private int timesDrunk = 0;
    bool timeUp = false;
    public GameObject gameOver;
    bool dead = false;
    float timeRemaining = 30;
    public GameObject diary;

    

    void OnGUI()
    {
        if (timeRemaining > 0)
        {
            GUI.Label(new Rect(100, 100, 200, 100), "Time Remaining : " + timeRemaining);
        } else
        {
            GUI.Label(new Rect(100, 100, 100, 100), "Time is up");
            timeUp = true;
        }

    }

    // Update is called once per frame
    void Update()
    {
        timeRemaining -=Time.deltaTime;

        if (!dead)
        {
            interact();
            checkDeath();
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

                    if (hit.collider.GetComponent<Transform>().name == "Viinapullo")
                    {
                        booze();
                    } else if (hit.collider.GetComponent<Transform>().name == "Dagger") {
                        dagger();
                    } else if (hit.collider.GetComponent<Transform>().name == "pistorasia") {
                        Death();
                    } else if (hit.collider.GetComponent<Transform>().name == "diary"){
                        Diary();
                    }
                }
            }
        }
    }

    public void Diary()
    {
        Canvas paivakirjaGO = GameObject.Find("paivakirja").GetComponent<Canvas>();
        paivakirjaGO.enabled = true;
    }


    public void checkDeath()
    {
        if (timesDrunk == 3 || timesCut == 3)
        {
            Death();
        }

    }

    public void Death()
    {
        dead = true;
        gameObject.GetComponent<FirstPersonController>().enabled = false;
        Instantiate(gameOver);
    }

    public void booze()
    {
        SoundManager.Instance.PlayDrink(transform.position);
        timesDrunk++;
        MotionBlur mBlur = Camera.main.GetComponent<MotionBlur>();
        mBlur.enabled = true;

        StartCoroutine(sober(mBlur));
    }

    IEnumerator sober(MotionBlur mBlur)
    {
        yield return new WaitForSeconds(5);
        if (!dead)
            mBlur.enabled = false;
        timesDrunk--;
    }
    

    public void dagger()
    {
        timesCut++;
    }

    void OnTriggerEnter(Collider other) {
        if (other.tag == "Door") {
            if (timeUp) {
                SceneManager.LoadScene("adult");
            }
        }
    }
}
