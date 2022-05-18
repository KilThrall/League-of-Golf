using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Thortilla : SkillSet
{
    public float knockbackRes = 1.5f, speedReduction, bounceRange;
    public GameObject lightningPrefab;
    public int bounces;
    public LayerMask whatIsPlayer;

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

        GetComponent<BallManager>().knockbackResistance = knockbackRes;
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

            List<Vector3> positions = new List<Vector3>();
            positions.Add(target.position + Vector3.up * 15);
            positions.Add(target.position);
            List<Transform> targets = new List<Transform>();
            targets.Add(target);

            for (int i = 0; i < bounces; i++)
            {
                Collider[] colliders = Physics.OverlapSphere(positions[positions.Count - 1], bounceRange,whatIsPlayer);
                if (colliders != null)
                {
                    Collider closestCollider = null;
                    float distance = -1;
                    foreach(Collider a in colliders)
                    {
                        if((Vector3.Distance(positions[positions.Count - 1], a.transform.position) < distance || distance == -1) && !IsTransformInList(a.transform, targets))
                        {
                            distance = Vector3.Distance(positions[positions.Count - 1], a.transform.position);
                            closestCollider = a;
                        }
                    }
                    if (closestCollider != null)
                    {
                        targets.Add(closestCollider.transform);
                        positions.Add(closestCollider.transform.position);
                    }
                }
            }
            Vector3[] positionsA = new Vector3[positions.Count];
            for (int i = 0; i < positions.Count; i++)
            {
                positionsA[i] = positions[i];
            }
            pView.RPC("SetLightnings", RpcTarget.All, positionsA);
            foreach(Transform a in targets)
            {
                if(a.transform.GetComponent<BallManager>() != null)
                {
                    a.transform.GetComponent<PhotonView>().RPC("WasHit", RpcTarget.All, a.transform.GetComponent<Rigidbody>().velocity / speedReduction, false);
                }
                else if(a.transform.parent.GetComponent<BallManager>()!=null)
                    a.transform.parent.GetComponent<PhotonView>().RPC("WasHit", RpcTarget.All, a.transform.parent.GetComponent<Rigidbody>().velocity / speedReduction, false);
            }
        }
    }

    [PunRPC]
    public void SetLightnings(Vector3[] pos)
    {
        for (int i = 1; i < pos.Length; i++)
        {
            Transform instance=Instantiate(lightningPrefab, pos[i], Quaternion.identity).transform;
            instance.GetChild(0).position = pos[i - 1];
            instance.GetChild(1).position = pos[i];
        }
    }

    private bool IsTransformInList(Transform transform, List<Transform> list)
    {
        bool result = false;
        for (int i = 0; i < list.Count; i++)
        {
            if (transform == list[i])
                result = true;
        }
        return result;
    }
}
