using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PhantomBall : MonoBehaviour
{
    public float lifetime=3;

    private void Start()
    {
        if (GetComponent<PhotonView>() == null)
        {
            StartCoroutine(TimeDeath());
            return;
        }
        if(GetComponent<PhotonView>().IsMine)
            StartCoroutine(TimeDeath());
    }

    private IEnumerator TimeDeath()
    {
        yield return new WaitForSeconds(lifetime);
        if (GetComponent<PhotonView>() != null)
            PhotonNetwork.Destroy(gameObject);
        else
            Destroy(gameObject);
    }
   /* public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
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
    }*/

}
