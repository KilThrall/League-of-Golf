using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Fernando : SkillSet
{
    public GameObject projectilePrefab;
    public float angleDif, distanceFromBallAtSpawn, strength;
    public int initialStrokes;


    protected override void SetPasive()
    {
        if (pView == null)
            pView = GetComponent<PhotonView>();
        if (pView.IsMine)
        {
            GetComponent<BallManager>().strokes = initialStrokes;
            GameControl.main.strokesText.text = initialStrokes.ToString();
        }
    }

    internal override void UseActive(bool pressedLeft)
    {
        if (cooldownLeft > 0)
            return;
        base.UseActive(pressedLeft);
        Vector3 forward = new Vector3(GameControl.main.controller.transform.forward.x, 0, GameControl.main.controller.transform.forward.z).normalized;
        
        float angle = Mathf.Asin(forward.x / forward.magnitude);
        float angle1 = angle + angleDif;
        float angle2 = angle - angleDif;
        Vector3 right = new Vector3(Mathf.Sin(angle1), 0, Mathf.Cos(angle1));
        Vector3 left = new Vector3(Mathf.Sin(angle2), 0, Mathf.Cos(angle2));

        if (forward.z < 0)
        {
            left = new Vector3(left.x, 0, left.z * -1);
            right = new Vector3(right.x, 0, right.z * -1);
        }


        GameObject instance = PhotonNetwork.Instantiate(projectilePrefab.name, transform.position + forward * distanceFromBallAtSpawn, Quaternion.identity);
        instance.GetComponent<Rigidbody>().AddForce(forward * GameControl.main.rules.maxPower * GameControl.main.controller.forceMultiplier*strength);

        instance = PhotonNetwork.Instantiate(projectilePrefab.name, transform.position + right * distanceFromBallAtSpawn, Quaternion.identity);
        instance.GetComponent<Rigidbody>().AddForce(right * GameControl.main.rules.maxPower * GameControl.main.controller.forceMultiplier * strength);

        instance = PhotonNetwork.Instantiate(projectilePrefab.name, transform.position + left * distanceFromBallAtSpawn, Quaternion.identity);
        instance.GetComponent<Rigidbody>().AddForce(left * GameControl.main.rules.maxPower*GameControl.main.controller.forceMultiplier * strength);
        //ballRb.AddForce((new Vector3(transform.forward.x,0,transform.forward.z).normalized+Vector3.up*0.05f) * currentForce * forceMultiplier);
        cooldownLeft = cooldown;
    }
}
