using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Enivia : SkillSet
{
    public GameObject AoEPrefab, placeholderPrefab;
    public float timeForPassive, respawnSpeedWithPassive=10, wallSpeed=0.7f;
    public Vector3 initialWallDif;

    private Transform wallCreated;
    private Vector3 targetPos;

    protected override void SetPasive()
    {
        if (skillPlaceholder == null)
            skillPlaceholder = Instantiate(placeholderPrefab);
        skillPlaceholder.SetActive(false);
    }

    internal override void UseActive(bool pressedLeft)
    {
        if (cooldownLeft > 0)
            return;
        base.UseActive(pressedLeft);
        Vector3 initialPos = skillPlaceholder.transform.position;

        Quaternion initialRot = skillPlaceholder.transform.rotation;
        pView.RPC("SpawnWall", RpcTarget.AllViaServer, initialPos, initialRot);
    }

    internal override void OnRespawn()
    {
        if (GameControl.main.controller.respawnSpeed==respawnSpeedWithPassive)
        {
            GameControl.main.controller.respawnSpeed = 1;
            pasiveCooldownLeft = pasiveCooldown;
        }       
    }

    private void Update()
    {
        if (wallCreated != null)
        {
            wallCreated.position = Vector3.MoveTowards(wallCreated.position, targetPos, wallSpeed * Time.deltaTime);
        }
        if (pView == null)
            pView = GetComponent<PhotonView>();
        if (pView.IsMine)
        {
            if (pasiveCooldownLeft <= 0)
            {
                GameControl.main.controller.respawnSpeed = respawnSpeedWithPassive;
            }
        }
    }

    [PunRPC]
    public void SpawnWall(Vector3 pos, Quaternion rotation)
    {
        targetPos = pos;
        wallCreated = Instantiate(AoEPrefab, pos+initialWallDif, rotation).transform;

    }
}
