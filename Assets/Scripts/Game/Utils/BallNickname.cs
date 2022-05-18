using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BallNickname : MonoBehaviour
{
    public Transform ballToFollow;
    public Vector3 offset, upVector;

    private PhotonView pView;

    private void Awake()
    {
        pView = GetComponent<PhotonView>();
        if (pView.IsMine)
            transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        else
            transform.GetChild(0).GetComponent<TextMesh>().text = pView.Owner.NickName;
    }

    private void FixedUpdate()
    {
        if (pView.IsMine && ballToFollow != null)
            transform.position = ballToFollow.position + offset;
        else
            transform.LookAt(Camera.main.transform,upVector);
    }

    [PunRPC]
    public void ChangeActive(bool state)
    {
        transform.GetChild(0).GetComponent<MeshRenderer>().enabled = state;
    }
}
