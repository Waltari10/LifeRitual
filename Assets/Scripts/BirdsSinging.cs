using UnityEngine;
using System.Collections;

public class BirdsSinging : MonoBehaviour {

    public AudioSource Source;
    public AudioClip BirdSing;
    public AudioClip BirdSing2;
    public AudioClip BirdSing3;

    public float singingVolume = 0.5F;

    public float MinRandomNumber = 1;
    public float MaxRandomNumber = 4;

    void Start () {

        StartCoroutine(SingingCoroutine());
  
	}

    IEnumerator SingingCoroutine() {
        PlayRandomAudio(BirdSing, BirdSing2, BirdSing2, singingVolume, transform.position);

        yield return new WaitForSeconds(Random.Range(MinRandomNumber, MaxRandomNumber));

        StartCoroutine(SingingCoroutine());
    }

    void PlayRandomAudio(AudioClip clip, AudioClip clip2, AudioClip clip3, float volume, Vector3 location)
    {
        transform.position = location;
        int x = Random.Range(1, 9);
        if (x == 1) Source.PlayOneShot(clip, volume);
        else if (x == 2) Source.PlayOneShot(clip2, volume);
        else Source.PlayOneShot(clip3, volume);
    }
}
