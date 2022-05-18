using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WinZone : MonoBehaviour
{
    public int holeValue;

    private void OnTriggerEnter(Collider other)
    {
      //  Debug.Log(other.tag);
        if (other.CompareTag("Player"))
        {
            if (!GameControl.main.isDefending&&GameControl.main.playerInstance==other.transform.parent.gameObject)
            {
                if (!GameControl.main.rules.anyHoleWorks)
                {
                    if(holeValue==GameControl.main.GetHole())
                        GameControl.main.MyBallWon(true);
                    else
                        GameControl.main.ResetPosition(other.transform.parent);
                }
                else
                {
                    GameControl.main.MyBallWon(true);
                }
            }
            else if(other.transform.parent.GetComponent<PhotonView>().IsMine)
            {
                GameControl.main.ResetPosition(other.transform.parent);
            }
        }
    }
}
