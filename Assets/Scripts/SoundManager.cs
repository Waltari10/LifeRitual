using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {

	public AudioSource Source;
	public AudioClip BabyCry;
	public AudioClip BabyCry2;
	public AudioClip BabyCry3;
    public AudioClip PaciferSucking;
    public AudioClip Drink1;
    public AudioClip Drink2;
    public AudioClip Drink3;
    public AudioClip TuutuLaulu;
    public AudioClip Box;
    public AudioClip Box2;
    public AudioClip Box3;
    public AudioClip Drawer;
    public AudioClip Drawer2;
    public AudioClip Drawer3;

	float babyCryVolume = 0.5f;
	float paciferSuckingVolume = 0.5f;
    float drinkVolume = 0.5f;
    float TuutuLauluVolume = 0.5f;
    float boxVolume = 0.5f;
    float windVolume = 0.5f;
    float drawerVolume = 0.5f;

    private static SoundManager instance;
	
	public static SoundManager Instance {
		get {
			if (instance == null) {
				instance = GameObject.FindObjectOfType<SoundManager>();
			}
			
			return instance;
		}
	}
	
	void Awake() {
		if(instance == null) {
			instance = this;
			DontDestroyOnLoad(this);
		}
		else {
			if (this != instance) Destroy(this.gameObject);
		}
	}
	
	void PlayRandomAudio(AudioClip clip, AudioClip clip2, AudioClip clip3, float volume, Vector3 location) {
		transform.position = location;
		int x = Random.Range(1,4);
		if (x == 1) Source.PlayOneShot(clip, volume);
		else if (x == 2) Source.PlayOneShot(clip2, volume);
		else Source.PlayOneShot(clip3, volume);
	}
	
	public void PlayBabyCry(Vector3 position) {
		PlayRandomAudio(BabyCry, BabyCry2, BabyCry3, babyCryVolume, position);
	}

    public void PlayDrink(Vector3 position)
    {
        PlayRandomAudio(Drink1, Drink2, Drink3, drinkVolume, position);
    }

    public void PlayTuutuLaulu()
    {
        Source.PlayOneShot(TuutuLaulu, TuutuLauluVolume);
    }

    public void PlayPacifierSucking(Vector3 position)
    {
        Source.PlayOneShot(PaciferSucking, paciferSuckingVolume);
    }

    public void PlayBox(Vector3 position)
    {
        PlayRandomAudio(Box, Box2, Box3, boxVolume, position);
    }

    public void PlayDrawer(Vector3 position)
    {
        PlayRandomAudio(Drawer, Drawer2, Drawer3, drawerVolume, position);
    }
}
