using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpiffingBrit : SkillSet
{
    public GameObject teaPrefab,coffeePrefab;
    public float powerDivider, powerMultiplier;

    private Transform target;
    private bool canUsePasive = true;

    private void Update()
    {
        if (pView == null)
            pView = GetComponent<PhotonView>();
        if (!pView.IsMine)
            return;
        target = MarkClosestToCenterBall();
        if (target == null)
            GameControl.main.targetTransform[0].gameObject.SetActive(false);
        else
        {
            if (target.GetComponent<BallManager>().redTeam == GetComponent<BallManager>().redTeam)
                GameControl.main.ChangeAimColor(Color.green, 0);
            else
                GameControl.main.ChangeAimColor(Color.red, 0);
        }
    }

    protected override void SetPasive()
    {
        if (pView == null)
            pView = GetComponent<PhotonView>();
        if (pView.IsMine&&canUsePasive&&PhotonNetwork.CurrentRoom.PlayerCount>1)
        {
            if (GameControl.main.isDefending)
            {
                BallManager[] balls = FindObjectsOfType<BallManager>();
                BallManager target=balls[Random.Range(0,balls.Length)];
                
                
                canUsePasive = false;
                StartCoroutine(ResetPasive(target));
               // GameControl.main.strokesText.text = initialStrokes.ToString();
            }
            
        }
    }

    private IEnumerator ResetPasive(BallManager target)
    {
        yield return new WaitForSeconds(4);
        target.GetComponent<PhotonView>().RPC("AddStroke", RpcTarget.AllViaServer);
        target.GetComponent<PhotonView>().RPC("SendGameMessage", RpcTarget.AllViaServer,"Congratulations, you wonderful person!\nYou have been randomly selected as a test subject, getting an extra stroke right from the bat.\nYours truly,\nThe Spiffing Brit");
        canUsePasive = true;
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

            float power = 1;
            if (!pressedLeft)
            {
                power = GameControl.main.rules.maxPower / powerDivider;
            }
            else
            {
               power= GameControl.main.rules.maxPower * powerMultiplier;
            }
            target.GetComponent<PhotonView>().RPC("SetMaxPower", RpcTarget.All, power); 

            Vector3 laserDir = -(transform.position - target.position);
            pView.RPC("CreateLaser", RpcTarget.All, laserDir, pressedLeft);
        }
    }

    [PunRPC]
    public void CreateLaser(Vector3 dir, bool type)
    {
        GameObject instance;
        if (type)
            instance = Instantiate(coffeePrefab, transform.position, Quaternion.identity);
        else
            instance = Instantiate(teaPrefab, transform.position, Quaternion.identity);
        instance.GetComponent<LineRenderer>().SetPosition(1, dir);
    }
}
