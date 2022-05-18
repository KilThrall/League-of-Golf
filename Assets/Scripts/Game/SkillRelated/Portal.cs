using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Portal : MonoBehaviour
{
    public Transform exit;

    private void OnTriggerEnter(Collider other)
    {
        //  Debug.Log(other.tag);
        if (other.CompareTag("Player"))
        {
            if (other.transform.parent.GetComponent<PhotonView>().IsMine)
            {
                Vector3 dif = other.transform.parent.GetComponent<Rigidbody>().velocity.normalized;
                other.transform.parent.position = exit.position + dif * 1.5f;
            }
        }
    }
}
