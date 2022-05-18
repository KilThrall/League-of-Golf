using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Mccri : SkillSet
{
    public float hitStrength, timeToCharge;
    public GameObject laserPrefab;

    private bool charging;
    private float timeChargingActive=0;
    List<Transform> targets;

    protected override void SetPasive()
    {
        
        if (pView.IsMine)
        {
            GameControl.main.controller.needsStopToMove = true;
        }
    }

    internal override void UseActive(bool pressedLeft)
    {
        if (!pView.IsMine)
            return;
        if (cooldownLeft > 0)
            return;
        base.UseActive(pressedLeft);
        charging = true;
        GameControl.main.controller.canMove = !charging;
        timeChargingActive = timeToCharge;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    private void Update()
    {
        if (charging)
        {
            targets = MarkAllBalls();

            timeChargingActive -= Time.deltaTime;
            if (timeChargingActive <= 0)
            {
                cooldownLeft = cooldown;
                charging = false;
                GameControl.main.controller.canMove = !charging;
                for (int i = 0; i < targets.Count; i++)
                {
                    Vector3 result = -(transform.position - Vector3.MoveTowards(transform.position, targets[i].position, 1)).normalized * GameControl.main.rules.maxPower * GameControl.main.controller.forceMultiplier * hitStrength;
                    targets[i].GetComponent<PhotonView>().RPC("WasHit", RpcTarget.All, result, true);

                    Vector3 laserDir = -(transform.position - targets[i].position);
                    pView.RPC("CreateLaser", RpcTarget.All, laserDir);
                }
                for (int i = 0; i < GameControl.main.targetTransform.Length; i++)
                {
                    GameControl.main.targetTransform[i].gameObject.SetActive(false);
                }
            }
        }
    }

    [PunRPC]
    public void CreateLaser(Vector3 dir)
    {
        GameObject instance = Instantiate(laserPrefab, transform.position, Quaternion.identity);
        instance.GetComponent<LineRenderer>().SetPosition(1, dir);
    }
}
