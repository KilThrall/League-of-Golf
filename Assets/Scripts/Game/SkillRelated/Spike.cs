using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Spike : MonoBehaviour
{
    [Range(0.01f,10)]
    public float strength;
    public Transform inmune;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.transform.parent.GetComponent<PhotonView>().IsMine)
            {
                if(other.transform.parent!=inmune)
                    other.transform.parent.GetComponent<Rigidbody>().AddForce((Vector3.up+Vector3.forward*Random.Range(-1f,1f)+ Vector3.right * Random.Range(-1f, 1f)).normalized *strength*GameControl.main.controller.maxForce*GameControl.main.controller.forceMultiplier);
            }
        }
    }
}
