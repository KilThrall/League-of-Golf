using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TemeoShroom : MonoBehaviour
{
    public float speed, explosionForce, timeToDisappear;

    internal Vector3 midPoint, endPoint;

    private bool madeItToMid = false, disappeared=false;

    private void Update()
    {
        if (!madeItToMid)
        {
            transform.position = Vector3.MoveTowards(transform.position, midPoint, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, midPoint) < 0.3f)
                madeItToMid = true;
        }
        else if (Vector3.Distance(transform.position, endPoint) > 0.3f)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPoint, speed * Time.deltaTime);
        }
        else if(timeToDisappear>0)
        {
            timeToDisappear -= Time.deltaTime;
            
        }else if (!disappeared)
        {
            disappeared = true;
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //  Debug.Log(other.tag);
        if (other.CompareTag("Player"))
        {
            if (other.transform.parent.GetComponent<PhotonView>().IsMine)
            {
                Vector3 force= other.transform.position-transform.position;
                force.Normalize();
                force += Vector3.up;
                other.transform.parent.GetComponent<Rigidbody>().AddForce(force*explosionForce);
            }
            Destroy(gameObject);
        }
    }
}
