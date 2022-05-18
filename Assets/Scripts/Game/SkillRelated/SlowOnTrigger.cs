using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SlowOnTrigger : MonoBehaviour
{
    [Range(0.01f,10)]
    public float strength;
    public bool constantSlow;
    public Transform inmune;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.transform.parent.GetComponent<PhotonView>().IsMine)
            {
                if(other.transform.parent!=inmune)
                    other.transform.parent.GetComponent<Rigidbody>().velocity /= strength;
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (!constantSlow)
            return;
        if (other.CompareTag("Player"))
        {
            if (other.transform.parent.GetComponent<PhotonView>().IsMine)
            {
                other.transform.parent.GetComponent<Rigidbody>().velocity = Vector3.MoveTowards(other.transform.parent.GetComponent<Rigidbody>().velocity,Vector3.zero,strength * Time.deltaTime);
            }
        }
    }
}
