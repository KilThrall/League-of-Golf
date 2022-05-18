using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundForButtons : MonoBehaviour
{
    public static SoundForButtons main;

    public AudioSource source;
    public AudioClip clickSound;
    public AudioClip[] otherClips;

    private void Awake()
    {
        Button[] buttons = FindObjectsOfTypeAll(typeof(Button))as Button[];
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].onClick.AddListener(ReproduceClick);
        }
        main = this;
    }

    public void ReproduceClick()
    {
        source.PlayOneShot(clickSound,0.6f);
    }

    public void ReproduceOther(int index, float vol)
    {
        source.PlayOneShot(otherClips[index], vol);
    }
}
