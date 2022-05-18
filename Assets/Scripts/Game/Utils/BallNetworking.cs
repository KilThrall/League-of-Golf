using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BallNetworking : MonoBehaviour,IPunObservable
{
    private Rigidbody rb;
    private PhotonView pView;

    private Vector3 networkPosition, networkVelocity;
    private Quaternion networkRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pView = GetComponent<PhotonView>();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rb.position);
            stream.SendNext(rb.rotation);
            stream.SendNext(rb.velocity);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            networkVelocity = (Vector3)stream.ReceiveNext();


            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            networkPosition += (rb.velocity * lag);
        }
    }

    private void FixedUpdate()
    {
        if (!pView.IsMine)
        {
            if (networkVelocity == null)
            {
                networkVelocity=Vector3.zero;
                networkPosition = transform.position;
                networkRotation = transform.rotation;
            }
            if (float.IsNaN(networkPosition.x) || float.IsNaN(networkRotation.y) || float.IsNaN(networkVelocity.z))
            {
                return;
            }
            if (Vector3.Distance(rb.velocity, networkVelocity) > 0.05f)
            {
                rb.velocity = networkVelocity;
            }
            if (Vector3.Distance(rb.velocity, Vector3.zero) < 4f)
            {
                if(Vector3.Distance(rb.position, networkPosition) > 1&& Vector3.Distance(rb.position, networkPosition) < 99999)
                {
                    rb.position = networkPosition;
                }
                else if(Vector3.Distance(rb.position, networkPosition) > 0.05f)
                {
                   // rb.position = Vector3.MoveTowards(rb.velocity,networkPosition,Time.fixedDeltaTime);
                }
                
                rb.rotation = networkRotation;
                //  transform.position = networkPosition;
                //  transform.rotation = networkRotation;
            }
        }
    }
}
