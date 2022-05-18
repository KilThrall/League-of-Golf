using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SoundReproducer : MonoBehaviour
{
    public AudioClip[] audioClips;

    private AudioSource audioSource;

    [PunRPC]
    public void ReproduceSound(int value, float volume)
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(audioClips[value], volume);
    }
}
