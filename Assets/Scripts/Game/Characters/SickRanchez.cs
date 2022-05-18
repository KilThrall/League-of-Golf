using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Photon.Pun;

public class SickRanchez : SkillSet
{
    public float shakeIntensity, shakeGain, shakeTime, returningSpeed;
    public NoiseSettings noiseProfile;
    public GameObject portalPrefab, laserPrefab;

    private CinemachineVirtualCamera cinemachineVirtualCamera;
    private float shakeLeft = 0, startingYPos=-1;
    private Transform target;
    private Rigidbody rb;
    //  private Vector3 startingPos, startingRot;

    protected override void SetPasive()
    {
           if (startingYPos == -1)
           {
                startingYPos = Camera.main.transform.localPosition.y;
               //startingRot = Camera.main.transform.localRotation.eulerAngles;
           }
        if (!pView.IsMine)
            return;
        if (cinemachineVirtualCamera == null)
            cinemachineVirtualCamera = Camera.main.GetComponent<CinemachineVirtualCamera>();
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        cinemachineBasicMultiChannelPerlin.m_NoiseProfile = noiseProfile;
        cinemachineBasicMultiChannelPerlin.m_FrequencyGain = shakeGain;
        pasiveCooldownLeft = pasiveCooldown/2;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (pView == null)
            return;
        if (!pView.IsMine)
            return;

        target = MarkClosestToCenterBall();
        if (target == null)
            GameControl.main.targetTransform[0].gameObject.SetActive(false);
        else
        {
            if (Vector3.Distance(transform.position, target.position) > rangeLimit)
            {
                GameControl.main.targetTransform[0].gameObject.SetActive(false);
            }
            else
            {
                if (target.GetComponent<BallManager>().redTeam == GetComponent<BallManager>().redTeam)
                    GameControl.main.ChangeAimColor(Color.green, 0);
                else
                    GameControl.main.ChangeAimColor(Color.red, 0);
            }
           
        }

        if (pasiveCooldownLeft <= 0)
        {
            
            if (cinemachineVirtualCamera == null)
                cinemachineVirtualCamera = Camera.main.GetComponent<CinemachineVirtualCamera>();
            cinemachineVirtualCamera.enabled = true;

            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = shakeIntensity;
            shakeLeft = shakeTime;
            pasiveCooldownLeft = pasiveCooldown;
        }
        SetScreenShake();
    }

    private void SetScreenShake()
    {
        if (shakeLeft > 0)
        {
            shakeLeft -= Time.deltaTime;
            if (shakeLeft <= 0)
            {
                CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

                cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0;
                cinemachineVirtualCamera.enabled = false;
            }
        }
        if (shakeLeft <= 0 && Vector3.Distance(new Vector3(0,startingYPos,-GameControl.main.controller.zoom), Camera.main.transform.localPosition) > 0.1f)
        {
            Camera.main.transform.localPosition = Vector3.MoveTowards(Camera.main.transform.localPosition, new Vector3(0, startingYPos, -GameControl.main.controller.zoom), returningSpeed * Time.deltaTime);
        }
        if (shakeLeft <= 0 && Vector3.Distance(Vector3.zero, Camera.main.transform.localRotation.eulerAngles) > 0.1f)
        {
            //   Camera.main.transform.localEulerAngles=(Vector3.MoveTowards(Camera.main.transform.localEulerAngles, Vector3.zero, returningSpeed));
            Camera.main.transform.localEulerAngles = Vector3.zero;

          /*  float difx = Camera.main.transform.localEulerAngles.x / returningSpeed;
            if (Mathf.Abs(difx) < 3)
            {
                difx = Camera.main.transform.localEulerAngles.x;
            }
            float dify = Camera.main.transform.localEulerAngles.y / returningSpeed;
            if (Mathf.Abs(dify) < 3)
            {
                dify = Camera.main.transform.localEulerAngles.y;
            }
            float difz = Camera.main.transform.localEulerAngles.z / returningSpeed;
            if (Mathf.Abs(difz) < 3)
            {
                difz = Camera.main.transform.localEulerAngles.z;
            }
            Camera.main.transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x - difx, Camera.main.transform.localEulerAngles.y - dify, Camera.main.transform.localEulerAngles.z - difz);*/
        }
    }

    internal override void UseActive(bool pressedLeft)
    {
        if (cooldownLeft > 0)
            return;
        base.UseActive(pressedLeft);


        if (target == null)
        {
         /*   Vector3 portal1Pos = transform.position;
            Vector3 dif = Vector3.forward;
            if (Vector3.Distance(rb.velocity, Vector3.zero) > 1 && Mathf.Abs(rb.velocity.y) < Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.z))
            {
                dif = rb.velocity.normalized;
                dif = new Vector3(dif.x, 0, dif.z).normalized;
            }
            else if (Vector3.Distance(rb.velocity, Vector3.zero) > 1)
            {
                dif = Vector3.up * rb.velocity.y / Mathf.Abs(rb.velocity.y);
            }
            portal1Pos += dif;

            Vector3 portal2Pos = transform.position;
            dif = Vector3.forward*14;

            portal2Pos += dif;


            Transform instance = Instantiate(portalPrefab, transform.position, Quaternion.identity).transform;
            instance.GetChild(0).position = portal1Pos;
            instance.GetChild(1).position = portal2Pos;
            instance.GetChild(0).LookAt(transform.position);
            instance.GetChild(1).LookAt(transform.position);
            pView.RPC("SpawnPortals", RpcTarget.Others, instance.position, instance.GetChild(0).position, instance.GetChild(1).position, instance.GetChild(0).rotation, instance.GetChild(1).rotation);*/
            return;
        }
        else
        {
            Vector3 portal1Pos = transform.position;
            Vector3 dif = Vector3.forward;
            if (Vector3.Distance(rb.velocity, Vector3.zero) > 1&&Mathf.Abs(rb.velocity.y)<Mathf.Abs(rb.velocity.x)+ Mathf.Abs(rb.velocity.z))
            {
                if (Vector3.Distance(rb.velocity, Vector3.zero) > 4)
                    dif = rb.velocity / 4;
                else
                    dif = rb.velocity.normalized;
            }else if(Vector3.Distance(rb.velocity, Vector3.zero) > 1)
            {
                dif = Vector3.up * rb.velocity.y / Mathf.Abs(rb.velocity.y);
            }
            portal1Pos += dif;

            Vector3 portal2Pos = target.position;
            dif = Vector3.forward;
            if (Vector3.Distance(target.GetComponent<Rigidbody>().velocity, Vector3.zero) > 1 && Mathf.Abs(target.GetComponent<Rigidbody>().velocity.y) < Mathf.Abs(target.GetComponent<Rigidbody>().velocity.x) + Mathf.Abs(target.GetComponent<Rigidbody>().velocity.z))
            {
                if (Vector3.Distance(target.GetComponent<Rigidbody>().velocity, Vector3.zero) > 4)
                    dif = target.GetComponent<Rigidbody>().velocity / 4;
                else
                    dif = target.GetComponent<Rigidbody>().velocity.normalized;
                // dif = new Vector3(dif.x, 0, dif.z).normalized;
            }
            else if (Vector3.Distance(target.GetComponent<Rigidbody>().velocity, Vector3.zero) > 1)
            {
                dif = Vector3.up * target.GetComponent<Rigidbody>().velocity.y / Mathf.Abs(target.GetComponent<Rigidbody>().velocity.y);
            }
            portal2Pos += dif;


            Transform instance=Instantiate(portalPrefab, transform.position, Quaternion.identity).transform;
            instance.GetChild(0).position = portal1Pos;
            instance.GetChild(1).position = portal2Pos;
            instance.GetChild(0).LookAt(transform.position);
            instance.GetChild(1).LookAt(target.position);
            pView.RPC("SpawnPortals", RpcTarget.Others, instance.position, instance.GetChild(0).position, instance.GetChild(1).position, instance.GetChild(0).rotation, instance.GetChild(1).rotation);
        }
    }


    [PunRPC]
    public void SpawnPortals(Vector3 initialPos, Vector3 portal1, Vector3 portal2, Quaternion portal1Rot, Quaternion portal2Rot)
    {
        Transform instance = Instantiate(portalPrefab, initialPos, Quaternion.identity).transform;
        instance.GetChild(0).position = portal1;
        instance.GetChild(1).position = portal2;
        instance.GetChild(0).rotation = portal1Rot;
        instance.GetChild(1).rotation = portal2Rot;
    }

    [PunRPC]
    public void CreateLaser(Vector3 dir)
    {
        GameObject instance = Instantiate(laserPrefab, transform.position, Quaternion.identity);
        instance.GetComponent<LineRenderer>().SetPosition(1, dir);
    }
}
