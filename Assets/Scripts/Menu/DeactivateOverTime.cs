using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateOverTime : MonoBehaviour
{
    public float timeToGo;

    private void OnEnable()
    {
        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        yield return new WaitForSeconds(timeToGo);
        gameObject.SetActive(false);
    }
}
