using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Jet : SkillSet
{
    public float mass, dashSpeed, dashDuration, postDashSpeedReduction;

    private Vector3 dashDir;
    private bool dashing = false;
    private float dashLeft = 0;
    private Rigidbody rb;

    protected override void SetPasive()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
        rb.mass = mass;
    }

    internal override void UseActive(bool pressedLeft)
    {
        if (cooldownLeft > 0)
            return;
        base.UseActive(pressedLeft);

        cooldownLeft = cooldown;
        dashDir = new Vector3(GameControl.main.controller.transform.forward.x, 0, GameControl.main.controller.transform.forward.z).normalized;
        dashing = true;
        dashLeft = dashDuration;
        pView.RPC("StartDash", RpcTarget.Others,dashDir);
    }

    [PunRPC]
    public void StartDash(Vector3 dir)
    {
        dashDir = dir;
        dashing = true;
        dashLeft = dashDuration;
    }

    private void Update()
    {
        if (dashing)
        {
            dashLeft -= Time.fixedDeltaTime;
            rb.velocity = dashDir * dashSpeed;
            if (dashLeft <= 0)
            {
                dashing = false;
                rb.velocity = rb.velocity / postDashSpeedReduction;
            }
                
        }
    }
}
