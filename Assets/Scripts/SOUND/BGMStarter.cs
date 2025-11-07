using UnityEngine;
using System.Collections;

public class BGMStarter : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(PlayWithDelay());
    }

    IEnumerator PlayWithDelay()
    {
        yield return new WaitForSeconds(0.3f);
        audioSource.Play();
    }
}