using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Driftking : SkillSet
{
    public float driftPotency, friction;

    private Vector3 dashDir;
    private Rigidbody rb;

    private void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    protected override void SetPasive()
    {
        if(pView.IsMine)
            GameControl.main.controller.friction = GameControl.main.rules.friction*friction;
    }

    internal override void UseActive(bool pressedLeft)
    {
        if (cooldownLeft > 0)
            return;
        base.UseActive(pressedLeft);

        cooldownLeft = cooldown;
        dashDir = new Vector3(GameControl.main.controller.transform.forward.x, 0, GameControl.main.controller.transform.forward.z).normalized;
        Vector3 rotated;
        if (pressedLeft)
            rotated = Quaternion.AngleAxis(90, Vector3.up) * dashDir;
        else
            rotated = Quaternion.AngleAxis(-90, Vector3.up) * dashDir;

        rb.AddForce(rotated * GameControl.main.controller.forceMultiplier * driftPotency);
    }
}
