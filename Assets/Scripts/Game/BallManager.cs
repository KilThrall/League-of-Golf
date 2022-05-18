using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    public TrailRenderer trailRenderer;

    internal bool redTeam, isDefending, finished;
    internal int strokes, maxStrokes;
    internal float knockbackMultiplier = 1, knockbackResistance = 1;

    private PhotonView pView;
    private Rigidbody rb;

    void Awake()
    {
        pView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
       /* if (pView.IsMine)
            StartCoroutine(UpdateData());*/
    }

    private void FixedUpdate()
    {
        if (!pView.IsMine)
        {
            /*rb.position = Vector3.MoveTowards(rb.position, networkPosition, Time.fixedDeltaTime);
            rb.rotation = Quaternion.RotateTowards(rb.rotation, networkRotation, Time.fixedDeltaTime * 100.0f);
            if (Vector3.Distance(rb.velocity, networkVelocity) > 3)
            {
                networkVelocity = Vector3.MoveTowards(networkVelocity, rb.velocity, Time.fixedDeltaTime);
                rb.velocity = Vector3.MoveTowards(rb.velocity, networkVelocity, Time.fixedDeltaTime);
            }*/
        }
        else
        {
            if (finished)
            {
                rb.velocity = Vector3.zero;
                if (Input.GetKeyDown(KeyCode.Q))
                    GameControl.main.controller.ChangeSpectation(-1);
                else if(Input.GetKeyDown(KeyCode.E))
                    GameControl.main.controller.ChangeSpectation(1);
            }
               
           /* if (Vector3.Distance(rb.velocity, networkVelocity) > 3)
            {
                networkVelocity = Vector3.MoveTowards(networkVelocity, rb.velocity, Time.fixedDeltaTime);
                //rb.velocity = Vector3.MoveTowards(rb.velocity, networkVelocity, Time.fixedDeltaTime);
            }*/
        }
    }

  /*  private IEnumerator UpdateData()
    {
        yield return new WaitForSeconds(1);
        pView.RPC("ReceiveData", RpcTarget.Others, transform.position, transform.eulerAngles, rb.velocity);
        StartCoroutine(UpdateData());
    }

    [PunRPC]
    public void ReceiveData(Vector3 pos, Vector3 rot, Vector3 spe)
    {
        transform.position = pos;
        transform.eulerAngles = rot;
        rb.velocity = spe;
    }*/

    public void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.collider.tag);
         if (collision.collider.CompareTag("Player") && !pView.IsMine&&collision.collider.transform.parent.GetComponent<PhotonView>().IsMine)
         {
             pView.RPC("WasHit", pView.Owner, rb.velocity * GameControl.main.playerInstance.GetComponent<BallManager>().knockbackMultiplier, false);
             if(collision.collider.transform.parent.GetComponent<SkillSet>()!=null)
                collision.collider.transform.parent.GetComponent<SkillSet>().OnHit(transform);
        }
         else if(collision.collider.CompareTag("Player") && pView.IsMine)
         {
             if(collision.collider.transform.parent.GetComponent<BallManager>()!=null)
                 GetComponent<SkillSet>().OnHit(collision.transform);
             pView.RPC("ReproduceSound", RpcTarget.All, 1,Mathf.Clamp(Vector3.Distance(rb.velocity,Vector3.zero)/10,0,1));
         }else if (pView.IsMine)
        {
            pView.RPC("ReproduceSound", RpcTarget.All, 0, Mathf.Clamp(Vector3.Distance(rb.velocity, Vector3.zero) / 10, 0, 1));
        }
    }

    [PunRPC]
    public void SetTeam(bool isThisRed)
    {
        redTeam = isThisRed;
        if (redTeam)
        {
            trailRenderer.startColor = Color.red;
        }
        else
        {
            trailRenderer.startColor = Color.blue;
        }
    }

    [PunRPC]
    public void WasHit(Vector3 force, bool shouldAdd)
    {
        if (!pView.IsMine)
            return;
        force /= knockbackResistance;
        if (shouldAdd)
        {
            rb.AddForce(force);
        }
        else
            rb.velocity = force;
    }

    [PunRPC]
    public void AddStroke()
    {
        if (GameControl.main.rules != null)
        {
            if (maxStrokes < 1)
                maxStrokes = GameControl.main.rules.maxStrokes;
        }
        
        strokes++;
        if(pView.IsMine)
            GameControl.main.strokesText.text = strokes.ToString()+"/"+maxStrokes.ToString();
        if (strokes >= maxStrokes)
        {
            rb.velocity = Vector3.zero;
            finished = true;
            if (pView.IsMine)
                BallFinished(true);
            else
                transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    [PunRPC]
    public void ResetBall()
    {
        strokes = 0;
        finished = false;
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).GetComponent<Collider>().enabled = true;
        rb.useGravity = true;
        rb.velocity = Vector3.zero;
        if (pView.IsMine)
        {
            GameControl.main.GetComponent<SoundReproducer>().ReproduceSound(1, 1);
            GameControl.main.controller.ResetPower();
            GameControl.main.controller.canMove = true;

            GameControl.main.strokesText.text = "0";
            isDefending = GameControl.main.isDefending;
            pView.RPC("SetAwake", RpcTarget.AllViaServer);

            if (isDefending)
            {
                GameControl.main.roleAcclarationText.gameObject.SetActive(true);
                GameControl.main.roleAcclarationText.text = "You are now DEFENDING!\n<size=15>Dont let the other team get in the hole</size>";
            }
            else
            {
                GameControl.main.roleAcclarationText.gameObject.SetActive(true);
                GameControl.main.roleAcclarationText.text = "You are now Attacking!\n<size=14>Get in the hole with the least amount of strokes</size>";
            }

            pView.RPC("ChangePos", RpcTarget.Others,transform.position);
            GetComponent<SkillSet>().OnReset();
        }
        else
        {
            if (redTeam == GameControl.main.redTeam)
                isDefending = GameControl.main.isDefending;
            else
                isDefending = !GameControl.main.isDefending;
        }

    }

    [PunRPC]
    public void ChangePos(Vector3 pos)
    {
        if (!pView.IsMine)
        {
            transform.position = pos;
            rb.position = pos;
            //Debug.Log(pos);
        }
    }

    [PunRPC]
    public void ReturnToLastPos()
    {
        if (pView.IsMine)
        {
            GameControl.main.controller.ReturnToLastPos();
        }
    }

    [PunRPC]
    public void SetRespawnSpeed(float value)
    {
        if (pView.IsMine)
        {
            GameControl.main.controller.respawnSpeed = value;
        }
    }

    [PunRPC]
    public void SetMaxPower(float value)
    {
        if (pView.IsMine)
        {
            GameControl.main.controller.maxForce = value;
        }
    }

    [PunRPC]
    public void FinishStrokes()
    {
        if (isDefending || finished)
            return;
        GameControl.main.roleAcclarationText.gameObject.SetActive(true);
        GameControl.main.roleAcclarationText.text = "You ran out of strokes or time\n<size=15>Wait for the other attackers to finish</size>";
        strokes = maxStrokes;
        finished = true;
        if (pView.IsMine)
        {
            BallFinished(true);
        }
        else
            transform.GetChild(0).gameObject.SetActive(false);
    }

    [PunRPC]
    public void DeactivateBall()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }

    [PunRPC]
    public void SendGameMessage(string message)
    {
        if (pView.IsMine)
        {
            GameControl.main.gameMessage.gameObject.SetActive(true);
            GameControl.main.gameMessage.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = message;
        }
        
    }

    public void BallFinished(bool shouldNotifyMain)
    {
        rb.velocity = Vector3.zero;
        if (shouldNotifyMain)
            GameControl.main.MyBallWon(false);
        finished = true;
        transform.GetChild(0).gameObject.SetActive(false);
        rb.useGravity = false;
        GameControl.main.controller.canMove = false;
    }
}
