using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Larry : SkillSet
{
    public float multiplier=1.5f, hitStrength;
    public GameObject laserPrefab;

    private Transform target;

    private void Start()
    {
        pView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (!pView.IsMine)
            return;
        target = MarkClosestToCenterBall();
        if (target==null)
            GameControl.main.targetTransform[0].gameObject.SetActive(false);
        else
        {
            if (target.GetComponent<BallManager>().redTeam == GetComponent<BallManager>().redTeam)
                GameControl.main.ChangeAimColor(Color.green, 0);
            else
                GameControl.main.ChangeAimColor(Color.red,0);
        }
    }

    protected override void SetPasive()
    {
        if(pView==null)
            pView = GetComponent<PhotonView>();

        GetComponent<BallManager>().knockbackMultiplier = multiplier;
    }

    internal override void UseActive(bool pressedLeft)
    {
        if (cooldownLeft > 0)
            return;
        base.UseActive(pressedLeft);


        if (target == null)
            return;
        else
        {
            cooldownLeft = cooldown;

            Vector3 result = -(transform.position - Vector3.MoveTowards(transform.position, target.position, 1)).normalized * GameControl.main.rules.maxPower * GameControl.main.controller.forceMultiplier* hitStrength;
            target.GetComponent<PhotonView>().RPC("WasHit",RpcTarget.All,result,true);

            Vector3 laserDir = -(transform.position - target.position);
            pView.RPC("CreateLaser", RpcTarget.All, laserDir);
        }
    }

    [PunRPC]
    public void CreateLaser(Vector3 dir)
    {
        GameObject instance = Instantiate(laserPrefab, transform.position, Quaternion.identity);
        instance.GetComponent<LineRenderer>().SetPosition(1, dir);
    }
}
