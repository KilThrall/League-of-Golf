using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Kenny : SkillSet
{
    public float mass;
    public GameObject aura;

    private bool activeInUse;

    protected override void SetPasive()
    {
        GetComponent<Rigidbody>().mass = mass;
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

    }

    internal override void OnHit(Transform collision)
    {
        if (!pView.IsMine)
            return;
        if (activeInUse)
        {
            cooldownLeft = cooldown;
            collision.GetComponent<PhotonView>().RPC("ReturnToLastPos", RpcTarget.All);
            GameControl.main.controller.ReturnToLastPos();
            
            activeInUse = false;
            pView.RPC("SwitchAuraState", RpcTarget.All, activeInUse);
            GameControl.main.AddInfoValue("skill", 1);
        }
    }

    internal override void OnReset()
    {
        if (activeInUse)
        {
            activeInUse = !activeInUse;
            pView.RPC("SwitchAuraState", RpcTarget.All, activeInUse);
        }
    }

    [PunRPC]
    public void SwitchAuraState(bool state)
    {
        aura.SetActive(state);
    }
}
