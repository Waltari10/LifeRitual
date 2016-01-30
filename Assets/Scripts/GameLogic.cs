using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour {

    public Transform adult;
    public Transform baby;
    public Transform elder;
    private Transform current;

	// Use this for initialization
	void Start () {
        SpawnPlayer(baby);
        
	}

    IEnumerator test()
    {
        yield return new WaitForSeconds(5);

        SwapPlayer(baby);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void SpawnPlayer(Transform playerType)
    {
        Vector3 playerSpawnPosition = new Vector3(0,0,0);
        current = Instantiate(playerType, playerSpawnPosition, Quaternion.identity) as Transform;
    }

    void SwapPlayer(Transform playerType)
    {
        Destroy(current.gameObject);
 
        Vector3 playerSpawnPosition = new Vector3(0, 0, 0);
        current = Instantiate(playerType, playerSpawnPosition, Quaternion.identity) as Transform;

    }
}
