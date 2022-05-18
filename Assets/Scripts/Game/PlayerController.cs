using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    public Transform instanceToFollow;
    public float mouseSensitivity = 1, objectiveDistance;
    public LayerMask whatIsFloor;
    public bool manualFriction;

    [Header("UI")]
    public GameObject pauseMenu;
    public Slider sensSlider;
    public InputField sensText;

    internal PhotonView pView;
    internal bool canMove, needsStopToMove;
    internal float forceMultiplier = 90000, respawnSpeed=1, friction = 1, maxForce = 1, zoom = 10;

    private Vector3 lastPosition;
    private BallManager[] pList;
    private Rigidbody ballRb;
    private bool charging,onFloor, paused, insideTheMap, awakened=false, cameraAvoids=false;
    private float currentForce=0, jumpForce=24000, jumpMultiplier=1, timeHoldingRestart=0;
    private Image sliderImage;
    private int following = 0;

    // Update is called once per frame
    void Update()
    {
        if (GameControl.main.rules == null)
            return;
        if ((instanceToFollow == null || ballRb == null) && GameControl.main != null)
        {
            SetReferences();
            return;
        }

        zoom -= Input.mouseScrollDelta.y;
        zoom = Mathf.Clamp(zoom,1, 25);

        RaycastHit ray;
        Physics.Raycast(instanceToFollow.position, Vector3.down, out ray,.9f, whatIsFloor);
        if (ray.transform != null)
        {
            onFloor = true;
            if (ray.collider.CompareTag("OutOfBounds"))
                insideTheMap = false;
            else
            {
                insideTheMap = true;
            }
                
        }
        else
        {
            insideTheMap = false;
            onFloor = false;
        }
        

        if (!pView.IsMine)
            return;


        if (pList == null)
            pList = FindObjectsOfType<BallManager>();
        if(pList.Length<PhotonNetwork.CurrentRoom.PlayerCount)
            pList = FindObjectsOfType<BallManager>();

        if (!canMove)
        {
            currentForce = 0;
            GameControl.main.powerBar.value = currentForce;
            charging = false;
        }
            

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameControl.main.scoreMenu.SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            GameControl.main.scoreMenu.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            GameControl.main.skillsMenu.SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.K))
        {
            GameControl.main.skillsMenu.SetActive(false);
        }

        if (onFloor&&needsStopToMove&& Vector3.Distance(ballRb.velocity, Vector3.zero) < 0.1f)
        {
            if (GameControl.main.rules.shootOutside)
                sliderImage.color = Color.green;
            else if (insideTheMap)
                sliderImage.color = Color.green;
            else
                sliderImage.color = Color.red;
        }else if (onFloor && Vector3.Distance(ballRb.velocity, Vector3.zero) < 1.5f&&!needsStopToMove)
        {
            if (GameControl.main.rules.shootOutside)
                sliderImage.color = Color.green;
            else if (insideTheMap)
                sliderImage.color = Color.green;
            else
                sliderImage.color = Color.red;
        }
        else
        {
            sliderImage.color = Color.red;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;
            Cursor.visible = paused;
            pauseMenu.SetActive(paused);

            if (paused)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        if (paused)
            return;

        Vector3 mouseDir = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));

        if (Input.GetKeyDown(KeyCode.Mouse1)&&canMove)
        {
            charging = false;
            currentForce = 0;
            GameControl.main.powerBar.value = currentForce;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && canMove)
        {
            charging = true;
        }
        else if (!Input.GetKey(KeyCode.Mouse0)&&onFloor&&charging && canMove)
        {
            if((Vector3.Distance(ballRb.velocity, Vector3.zero) < 1.5f&&!needsStopToMove)|| (Vector3.Distance(ballRb.velocity, Vector3.zero) < 0.1f && needsStopToMove))
            {
                if ((!GameControl.main.rules.shootOutside && insideTheMap) || GameControl.main.rules.shootOutside)
                {
                    charging = false;

                    if (currentForce > 0.05f)
                    {
                        if (!GameControl.main.isDefending)
                            ballRb.GetComponent<PhotonView>().RPC("AddStroke", RpcTarget.All);
                        ballRb.AddForce((new Vector3(transform.forward.x, 0, transform.forward.z).normalized + Vector3.up * 0.05f) * currentForce * forceMultiplier);
                        GameControl.main.AddInfoValue("shot", 1);
                        maxForce = GameControl.main.rules.maxPower;
                        ballRb.GetComponent<SoundReproducer>().ReproduceSound(2,1);
                        if (Tutorial.main != null)
                            Tutorial.main.PlayerShot();
                        ballRb.GetComponent<SkillSet>().OnThrow();
                    }

                    currentForce = 0;
                    GameControl.main.powerBar.value = currentForce / maxForce;
                }
            }
        }

        if (Input.GetKey(KeyCode.R) && canMove)
        {
            Respawning(false);
           
        }else if (Input.GetKey(KeyCode.F) && canMove)
        {
            Respawning(true);
        }
        if (Input.GetKeyUp(KeyCode.R) || Input.GetKeyUp(KeyCode.F))
        {
            timeHoldingRestart = 0;
            GameControl.main.resetBar.value = 0;
            GameControl.main.resetBar.gameObject.SetActive(false);
        }
           

        if (Input.GetKeyDown(KeyCode.Space)&&onFloor && canMove)
            ballRb.AddForce(Vector3.up * jumpForce*jumpMultiplier);

        if (!charging|| !Input.GetKey(KeyCode.Mouse0))
        {
            transform.eulerAngles += mouseDir * mouseSensitivity;
            //     Debug.Log(transform.eulerAngles.x);

            float angleX = Mathf.Clamp(transform.eulerAngles.x, 0, 80);
            if(transform.eulerAngles.x>180)
                angleX = Mathf.Clamp(transform.eulerAngles.x, 280, 360);
            transform.eulerAngles = new Vector3(angleX, transform.eulerAngles.y, transform.eulerAngles.z);
        }
        else
        {
            transform.eulerAngles += new Vector3(0, mouseDir.y, 0) * mouseSensitivity;
            if (Input.GetKey(KeyCode.Mouse0))
            {
                currentForce += -mouseDir.x * Time.deltaTime * mouseSensitivity * 2* maxForce;
                currentForce = Mathf.Clamp(currentForce, 0, maxForce);
                GameControl.main.powerBar.value = currentForce/maxForce;
            }
        }

        if (canMove)
        {
            if (Input.GetKey(KeyCode.E))
            {
                ballRb.GetComponent<SkillSet>().HoldActive(true);
            }

            else if (Input.GetKey(KeyCode.Q))
            {
                ballRb.GetComponent<SkillSet>().HoldActive(false);
            }

            if (Input.GetKeyUp(KeyCode.E))
            {
                ballRb.GetComponent<SkillSet>().CancelActive();
                ballRb.GetComponent<SkillSet>().UseActive(true);
            }
               
            else if (Input.GetKeyUp(KeyCode.Q))
            {
                ballRb.GetComponent<SkillSet>().CancelActive();
                ballRb.GetComponent<SkillSet>().UseActive(false);
            }
                
        }
    }

    private void FixedUpdate()
    {
        if ((instanceToFollow == null||ballRb==null)&&GameControl.main!=null)
        {
            if (GameControl.main.rules == null)
                return;
            SetReferences();
            return;
        }
            
        if (!pView.IsMine)
            return;

        if (ballRb == null)
            ballRb = GameControl.main.playerInstance.GetComponent<Rigidbody>();

        transform.position = instanceToFollow.position;
        if (ballRb.velocity != Vector3.zero&&ballRb.velocity!=null && onFloor&&manualFriction)
            ballRb.velocity = Vector3.MoveTowards(ballRb.velocity, Vector3.zero, friction * Time.deltaTime);

        if (Vector3.Distance(ballRb.velocity, Vector3.zero) < 1&&insideTheMap)
            lastPosition = transform.position;

        if (paused)
            return;

        RaycastHit ray;
        Physics.Raycast(transform.position, -(transform.position - Vector3.MoveTowards(transform.position, Camera.main.transform.position, 1)), out ray,10);
        if (ray.collider == null||!cameraAvoids)
            transform.GetChild(0).localPosition = Vector3.back * zoom;
        else
        {
            float dist = Mathf.Clamp(zoom, 1, Vector3.Distance(transform.position, ray.point));
            transform.GetChild(0).localPosition = Vector3.back * dist;
        }
            

    }


    internal void SetAwake()
    {
        if (ballRb == null)
        {
            SetReferences();
            return;
        }

        if (GameControl.main.rules == null)
            return;

        canMove = true;
        awakened = true;

        sliderImage = GameControl.main.powerBar.transform.GetChild(1).GetChild(0).GetComponent<Image>();

        
        maxForce = GameControl.main.rules.maxPower;
        friction = GameControl.main.rules.friction;
        ballRb.transform.GetChild(0).GetComponent<Collider>().material.bounciness = GameControl.main.rules.bounciness;
        jumpMultiplier = GameControl.main.rules.jumpPower;


        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameControl.main.powerBar.maxValue = 1;
        

        float value = 1;
        if (PlayerPrefs.HasKey("sensitivity"))
        {
            value = PlayerPrefs.GetFloat("sensitivity");
        }
        mouseSensitivity = value;
        sensSlider.value = value;
        sensText.text = value.ToString();
    }

    internal void ResetPower()
    {
        currentForce = 0;
        charging = false;
    }


    public void SendRules(Rules data)
    {
        maxForce = data.maxPower;
        friction = data.friction;
        jumpForce = 200 * data.jumpPower;
    }

    public void ChangeSensitivity(string sValue)
    {
        float value = float.Parse(sValue);
        if (value < 0.05f)
            value = 0.05f;
        mouseSensitivity = value;
        sensSlider.value = value;
        PlayerPrefs.SetFloat("sensitivity", value);
        sensText.text = value.ToString();
    }

    public void ChangeSensitivity(float value)
    {
        mouseSensitivity = value;
        sensSlider.value = value;
        PlayerPrefs.SetFloat("sensitivity", value);
        sensText.text = value.ToString("F2");
    }

    public void SwitchCameraAvoids(bool isOn)
    {
        cameraAvoids = isOn;
        if(isOn)
            PlayerPrefs.SetInt("cameraAvoids", 1);
        else
            PlayerPrefs.SetInt("cameraAvoids", 0);
    }

    internal void ReturnToLastPos()
    {
        ballRb.velocity = Vector3.zero;
        ballRb.position = lastPosition;
        transform.position = lastPosition;
        ballRb.GetComponent<PhotonView>().RPC("ChangePos", RpcTarget.Others, ballRb.transform.position);
    }

    internal void SetLastPos(Vector3 pos)
    {
        lastPosition = pos;
    }

    internal void ChangeSpectation(int dif)
    {
        following += dif;
        if (following >= pList.Length)
            following = 0;
        if (following < 0)
            following = pList.Length - 1;
        instanceToFollow = pList[following].transform;
    }

    private bool SetReferences()
    {
        if (GameControl.main.playerInstance == null)
            return false;
        if (instanceToFollow == null)
            instanceToFollow = GameControl.main.playerInstance.transform;
        ballRb = instanceToFollow.GetComponent<Rigidbody>();
        pView = ballRb.GetComponent<PhotonView>();
        if(!awakened)
            SetAwake();
        return true;
    }

    

    private void Respawning(bool goingToBase)
    {
        GameControl.main.resetBar.gameObject.SetActive(true);
        timeHoldingRestart += Time.deltaTime * respawnSpeed;
        if (!goingToBase)
        {
            GameControl.main.resetBar.value = timeHoldingRestart / GameControl.main.rules.respawnTime;
            if (timeHoldingRestart >= GameControl.main.rules.respawnTime)
            {
                ReturnToLastPos();
                FinishRestart();
            }
        }
        else
        {
            GameControl.main.resetBar.value = timeHoldingRestart / GameControl.main.rules.resetTime;
            if (timeHoldingRestart >= GameControl.main.rules.resetTime)
            {
                
                GameControl.main.ResetPosition();
                FinishRestart();
            }
        }
    }

    private void FinishRestart()
    {
        ballRb.velocity = Vector3.zero;
        timeHoldingRestart = 0;
        if (respawnSpeed < 1)
            respawnSpeed = 1;
        ballRb.GetComponent<SkillSet>().OnRespawn();
        if (GameControl.main.rules.resetPrice > 0)
        {
            for (int i = 0; i < GameControl.main.rules.resetPrice; i++)
            {
                ballRb.GetComponent<PhotonView>().RPC("AddStroke", RpcTarget.All);
            }
        }
        
    }
   
}
