using System.Collections;
using UnityEngine;

public class PlaySoundInterval : MonoBehaviour
{
    public float minInterval = 4f;
    public float maxInterval = 12f;
    public bool playOnStart = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (playOnStart) 
        {
            GetComponent<AudioSource>().Play();
        }
        StartCoroutine(PlaySound());
    }

    IEnumerator PlaySound()
    {
       
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            GetComponent<AudioSource>().Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
