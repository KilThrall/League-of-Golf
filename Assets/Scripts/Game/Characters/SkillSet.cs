using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public abstract class SkillSet : MonoBehaviour
{
    public float cooldown, pasiveCooldown =-1;
    [Range(0, 1)]
    public float skillSfxVolume = 1;
    public Sprite skillSprite, characterSprite;
    [TextArea]
    public string characterName,passiveDescription, activeDescription;
    public int activeSFX=-1;

    [Header("AoE skills")]
    public GameObject skillPlaceholder;
    public float rangeLimit;
    public Vector3 rotationAxis;
    [Header("Character specific")]
    public bool lineLolIgnore;

    internal GameObject nicknameInstance;

    protected float cooldownLeft, pasiveCooldownLeft;
    protected PhotonView pView;

    [PunRPC]
    internal void SetAwake()
    {
        if (pView == null)
            pView = GetComponent<PhotonView>();
        SetPasive();
        if (!pView.IsMine)
            return;
        GameControl.main.skillImage.sprite = skillSprite;
        GameControl.main.passiveText.text = passiveDescription;
        GameControl.main.activeText.text = activeDescription;
        GameControl.main.characterNameText.text = characterName;
        if (pasiveCooldown != -1)
            GameControl.main.pasiveSkillCooldown.gameObject.SetActive(true);
    }

    private void FixedUpdate()
    {
        if (pView == null)
        {
            pView = GetComponent<PhotonView>();
        }
        if (!pView.IsMine)
            return;
        GameControl.main.skillCooldown.value = Mathf.Clamp(1-(cooldownLeft / cooldown), 0, 1);
        if (cooldownLeft > 0)
            cooldownLeft -= Time.deltaTime;
        
        GameControl.main.skillCooldown.transform.GetChild(1).GetChild(0).GetComponent<UnityEngine.UI.Image>().color=Color.Lerp(new Color(0, 0.9f, 0, 0.7f), new Color(0.9f, 0, 0, 0.7f), (cooldownLeft / cooldown));
        
        if (pasiveCooldown != -1)
        {
            if (pasiveCooldownLeft > 0)
                pasiveCooldownLeft -= Time.deltaTime;
            GameControl.main.pasiveSkillCooldown.value = Mathf.Clamp(1 - (pasiveCooldownLeft / pasiveCooldown), 0, 1);
            GameControl.main.pasiveSkillCooldown.transform.GetChild(1).GetChild(0).GetComponent<UnityEngine.UI.Image>().color = Color.Lerp(new Color(0, 0.9f, 0, 0.7f), new Color(0.9f, 0, 0, 0.7f), (pasiveCooldownLeft / pasiveCooldown));
        }
    }


    protected Transform MarkClosestToCenterBall()
    {
        Transform target = null;
        List<Transform> balls = GetAllBallsOnScreen();
        if (balls.Count > 0)
        {
            target = balls[0];
            GameControl.main.targetTransform[0].gameObject.SetActive(true);
            if (balls.Count > 1)
            {
                float distance = -1;
                for (int i = 0; i < balls.Count; i++)
                {
                    if(Vector2.Distance(Camera.main.WorldToScreenPoint(balls[i].transform.position), new Vector2(Screen.width, Screen.height) / 2)<distance||distance==-1)
                    {
                        distance = Vector2.Distance(Camera.main.WorldToScreenPoint(balls[i].transform.position), new Vector2(Screen.width, Screen.height) / 2);
                        target = balls[i];
                    }
                }
            }
            GameControl.main.targetTransform[0].position = Camera.main.WorldToScreenPoint(target.position);
            
        }
        return target;
    }

    protected List<Transform> MarkAllBalls()
    {
        List<Transform> balls = GetAllBallsOnScreen();
        for (int i = 0; i < GameControl.main.targetTransform.Length; i++)
        {
            GameControl.main.targetTransform[i].gameObject.SetActive(false);
        }
        if (balls.Count > 0)
        {
            for (int i = 0; i < balls.Count; i++)
            {
                GameControl.main.targetTransform[i].gameObject.SetActive(true);
                GameControl.main.targetTransform[i].position = Camera.main.WorldToScreenPoint(balls[i].position);
                if (balls[i].GetComponent<BallManager>().redTeam == GetComponent<BallManager>().redTeam)
                    GameControl.main.ChangeAimColor(Color.green, i);
                else
                    GameControl.main.ChangeAimColor(Color.red, i);
            }
            

        }
        return balls;
    }

    protected List<Transform> GetAllBallsOnScreen()
    {
        BallManager[] balls = FindObjectsOfType<BallManager>();
        List<Transform> result= new List<Transform>();
        for (int i = 0; i < balls.Length; i++)
        {
            if (balls[i].transform != transform)
            {
                RaycastHit ray;
                // Physics.Raycast(balls[i].transform.position, -(balls[i].transform.position - Vector3.MoveTowards(Camera.main.transform.position, balls[i].transform.position, 1)), out ray);
                Physics.Raycast(Camera.main.transform.position, -(Camera.main.transform.position-Vector3.MoveTowards(Camera.main.transform.position,balls[i].transform.position,1)), out ray);
              //  Debug.DrawRay(Camera.main.transform.position, -(Camera.main.transform.position - Vector3.MoveTowards(Camera.main.transform.position, balls[i].transform.position, 5)), Color.green);
                if (ray.transform != null)
                {
                    Vector3 viewport = Camera.main.WorldToViewportPoint(balls[i].transform.position);
                    if (ray.transform == balls[i].transform && viewport.x >= 0 && viewport.x <= 1 && viewport.y >= 0 && viewport.y <= 1 && viewport.z >= 0)
                    {
                        if(!balls[i].finished)
                            result.Add(balls[i].transform);
                    }
                    
                }
                
            }
        }
        return result;
    }

    protected abstract void SetPasive();

    internal virtual void UseActive(bool pressedLeft)
    {
        if (cooldownLeft > 0)
            return;

        if (Tutorial.main != null)
            Tutorial.main.SkillUsed(pressedLeft);

        cooldownLeft = cooldown;
        GameControl.main.AddInfoValue("skill", 1);
        if (activeSFX != -1)
            GetComponent<PhotonView>().RPC("ReproduceSound", RpcTarget.All, activeSFX,skillSfxVolume);
    }

    internal virtual void HoldActive(bool pressedLeft)
    {
        if (skillPlaceholder == null)
            return;
        if (cooldownLeft > 0)
            return;
        skillPlaceholder.SetActive(true);
        skillPlaceholder.transform.position = transform.position + Camera.main.transform.forward.normalized*rangeLimit;
        skillPlaceholder.transform.position = new Vector3(skillPlaceholder.transform.position.x, transform.position.y, skillPlaceholder.transform.position.z);
        RaycastHit ray;
        Physics.Raycast(transform.position, -(transform.position - Vector3.MoveTowards(transform.position, skillPlaceholder.transform.position, 1)), out ray, rangeLimit);
        if (ray.collider != null)
        {
            if(Vector3.Distance(transform.position,skillPlaceholder.transform.position)> Vector3.Distance(transform.position, ray.point))
                skillPlaceholder.transform.position = ray.point;
        }
        if (Vector3.Distance(rotationAxis, Vector3.zero) > 0.1f)
        {
            skillPlaceholder.transform.LookAt(transform.position, rotationAxis);
        }
    }

    internal virtual void CancelActive()
    {
        if (skillPlaceholder == null)
            return;
        skillPlaceholder.SetActive(false);
    }

    internal virtual void OnHit(Transform obj) { }
    internal virtual void OnThrow() { }
    internal virtual void OnRespawn() { }
    internal virtual void OnReset() { }
}
