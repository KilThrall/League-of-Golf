using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Sanic : SkillSet
{
    public float hitStrength, respawnSpeedMultiplier, respawnSpeedDebuff;
    public GameObject aura;

    private bool activeInUse;

    protected override void SetPasive()
    {
        if (!GetComponent<PhotonView>().IsMine)
            return;
        GameControl.main.controller.respawnSpeed = respawnSpeedMultiplier;
    }

    internal override void UseActive(bool pressedLeft)
    {
        if (cooldownLeft > 0)
        {
            activeInUse = false;
            return;
        }
            
        activeInUse = !activeInUse;
        pView.RPC("SwitchAuraState", RpcTarget.All, activeInUse);
        /*if (activeInUse)
            GetComponent<BallManager>().knockbackMultiplier = hitStrength;
        else
            GetComponent<BallManager>().knockbackMultiplier = 1;*/
    }

    internal override void OnHit(Transform collision)
    {
        if (activeInUse)
        {
            Debug.Log(collision.name);
            cooldownLeft = cooldown;
            collision.GetComponent<PhotonView>().RPC("SetRespawnSpeed", RpcTarget.All, respawnSpeedDebuff);
            Vector3 result = -(transform.position - Vector3.MoveTowards(transform.position, collision.position, 1)).normalized * GameControl.main.rules.maxPower * GameControl.main.controller.forceMultiplier*hitStrength;
            collision.GetComponent<PhotonView>().RPC("WasHit", RpcTarget.All, result, true);

            //GetComponent<BallManager>().knockbackMultiplier = 1;
            activeInUse = false;
            pView.RPC("SwitchAuraState", RpcTarget.All, activeInUse);
            GameControl.main.AddInfoValue("skill", 1);
        }
           
    }

    [PunRPC]
    public void SwitchAuraState(bool state)
    {
        aura.SetActive(state);
        /*if(state)
            GetComponent<BallManager>().knockbackMultiplier = hitStrength;
        else
            GetComponent<BallManager>().knockbackMultiplier = 1;*/
    }

    internal override void OnReset()
    {
        if (activeInUse)
        {
            activeInUse = !activeInUse;
            pView.RPC("SwitchAuraState", RpcTarget.All, activeInUse);
        }
    }
}
