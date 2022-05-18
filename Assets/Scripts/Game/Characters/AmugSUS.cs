using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmugSUS : SkillSet
{
    public float passiveDuration = 1f, activeDuration = 2f;

    public int invulnerableLayer, startingLayer;
    public Material normalBallMaterial, invulnerableMaterial;

    private float passiveLeft = -1, activeLeft = -1, fogDensity = 0.018f;
    private Renderer ballRenderer;

    private void Awake()
    {
        ballRenderer = transform.GetChild(0).GetComponent<Renderer>();
        if (pView == null)
        {
            pView = GetComponent<PhotonView>();
        }
    }

    protected override void SetPasive()
    {
        pasiveCooldownLeft = pasiveCooldown;
    }

    private void Update()
    {
        if (activeLeft > 0)
        {
            activeLeft -= Time.deltaTime;
        }
        else if (activeLeft != -1)
        {
            activeLeft = -1;
            GameControl.main.directionalLight.gameObject.SetActive(true);
            RenderSettings.fogDensity = 0;
        }

        if (!pView.IsMine)
        {
            return;
        }
        if (pasiveCooldownLeft != -1)
        {
            if (pasiveCooldownLeft > 0)
            {
                pasiveCooldownLeft -= Time.deltaTime;
            }
            else
            {
                passiveLeft = passiveDuration;
                pasiveCooldownLeft = -1;
                pView.RPC("ChangeInvulnerabilityState", RpcTarget.All, true);
            }
        }
        else
        {
            if (passiveLeft > 0)
            {
                passiveLeft -= Time.deltaTime;
            }
            else if (passiveLeft != -1)
            {
                passiveLeft = -1;
                pasiveCooldownLeft = pasiveCooldown;
                pView.RPC("ChangeInvulnerabilityState", RpcTarget.All, false);
            }
        }
        
    }

    internal override void UseActive(bool pressedLeft)
    {
        if (cooldownLeft > 0)
            return;
        base.UseActive(pressedLeft);

        cooldownLeft = cooldown;

        pView.RPC("TurnOffTheLights", RpcTarget.Others);

    }

    [PunRPC]
    public void ChangeInvulnerabilityState(bool state)
    {
        gameObject.layer = state ? invulnerableLayer : startingLayer;
        ballRenderer.gameObject.layer = gameObject.layer;
        ballRenderer.material = state ? invulnerableMaterial : normalBallMaterial;
    }

    [PunRPC]
    public void TurnOffTheLights()
    {
        GameControl.main.directionalLight.gameObject.SetActive(false);
        activeLeft = activeDuration;
        RenderSettings.fogDensity = fogDensity;
    }
}
