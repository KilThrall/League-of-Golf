using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Audio;

public class GameControl : MonoBehaviourPunCallbacks
{
    public static GameControl main;
    public AudioMixer audioMixer;
    public Material personalBallShader;

    [Header("Game references")]
    public Slider powerBar;
    public Hole[] holes;
    public PlayerController controller;
    public GameObject scoreMenu, skillsMenu, gameMessage;
    public Text strokesText, timeLeftText, passiveText, activeText, characterNameText, roleAcclarationText, blueTotalPoints, redTotalPoints, errorText;
    public Slider skillCooldown, resetBar, volume, sfxVol, musicVol, ambientVol, pasiveSkillCooldown;
    public Image skillImage;
    public RectTransform[] targetTransform;
    public Transform scoresList;
    public Toggle cameraAvoidsToggle;
    public GameObject dirArrowPrefab;
    public GameObject directionalLight;

    [Header("Prefab references")]
    [Space(20)]
    public GameObject nicknamePrefab;
    public GameObject[] characterPrefabs;

    internal Rules rules;
    internal bool isDefending = true, redTeam=true;
    internal GameObject playerInstance, nicknameInstance;

    private int characterSelected=0, currentHole=0, currentSpawner=0, attackingPlayers=0, finishedPlayers=0, holesUsed=3;
    private bool firstRound=true;
    private float timeLeft, originalTimeLimit;
    private Stats gameInfo = new Stats();

    private GameObject dirArrowInstance;

    private void Awake()
    {
        if (main == null)
            main = this;
        else
            Destroy(this);

        StartCoroutine(TrueAwake());
    }

    private IEnumerator TrueAwake()
    {
        yield return new WaitForSeconds(.2f);

        {
            int aux = PlayerPrefs.GetInt("cameraAvoids", 0);
            bool result = aux == 1;
            cameraAvoidsToggle.isOn = result;
            controller.SwitchCameraAvoids(result);
        }
        

        if (PlayerPrefs.HasKey("characterSelected"))
        {
            characterSelected = PlayerPrefs.GetInt("characterSelected");
        }

        if (PlayerPrefs.HasKey("redTeam"))
        {
            if (PlayerPrefs.GetInt("redTeam") == 0)
                redTeam = false;
        }

        if (PlayerPrefs.HasKey("stats"))
        {
            string result = PlayerPrefs.GetString("stats");
            gameInfo = JsonUtility.FromJson<Stats>(result);
        }
        
        if (gameInfo.timesCharacterWasUsed.Length < characterPrefabs.Length)
        {
            int[] aux = new int[characterPrefabs.Length];
            for (int i = 0; i < gameInfo.timesCharacterWasUsed.Length; i++)
            {
                aux[i] = gameInfo.timesCharacterWasUsed[i];
            }
            gameInfo.timesCharacterWasUsed = aux;
        }
        

        isDefending = redTeam;

        if (PhotonNetwork.CurrentRoom.PlayerCount <= 1)
        {
            firstRound = true;
            isDefending = false;
            redTeam = false;
        }

        Vector3 spawnPos;
        if (isDefending)
        {
            currentSpawner = Random.Range(0, holes[currentHole].defenderSpawns.Length);
            spawnPos = holes[currentHole].defenderSpawns[currentSpawner].position;
        }
        else
        {
            currentSpawner = Random.Range(0, holes[currentHole].attackerSpawns.Length);
            spawnPos = holes[currentHole].attackerSpawns[currentSpawner].position;
        }
        spawnPos += new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));

        playerInstance = PhotonNetwork.Instantiate(characterPrefabs[characterSelected].name, spawnPos, Quaternion.identity);
       
        //TODO: Switch to support skin
        // playerInstance.transform.GetChild(0).GetComponent<MeshRenderer>().material = personalBallShader;
        controller.instanceToFollow = playerInstance.transform;

        CopyRotation arrow = Instantiate(dirArrowPrefab).GetComponent<CopyRotation>();
        dirArrowInstance = arrow.gameObject;
        arrow.posTarget = playerInstance.transform;
        arrow.rotTarget = controller.transform;

        if (PhotonNetwork.IsMasterClient)
        {
            string newRules = PlayerPrefs.GetString("rules");
            yield return new WaitForSeconds(0.6f);
            GetComponent<PhotonView>().RPC("SetRules", RpcTarget.AllBuffered, newRules);
            
        }

        playerInstance.GetComponent<PhotonView>().RPC("SetTeam",RpcTarget.AllBuffered, redTeam);
        playerInstance.GetComponent<BallManager>().isDefending = isDefending;
        playerInstance.GetComponent<PhotonView>().RPC("ResetBall", RpcTarget.AllBufferedViaServer);

        nicknameInstance = PhotonNetwork.Instantiate(nicknamePrefab.name, spawnPos, Quaternion.identity);
        nicknameInstance.GetComponent<BallNickname>().ballToFollow = playerInstance.transform;
        playerInstance.GetComponent<SkillSet>().nicknameInstance = nicknameInstance;
        
        SetVolumes();
    }

    private void FixedUpdate()
    {
        if (rules == null)
            return;
        if (rules.timeInSeconds <= 1)
            return;
        if (timeLeft <= rules.timeInSeconds && timeLeft != -3)
        {
            timeLeft += Time.deltaTime;
            timeLeftText.text = (rules.timeInSeconds-timeLeft).ToString("F1");
        }
        else if (timeLeft!=-3)
        {
            
            timeLeft = -3;
            if (PhotonNetwork.IsMasterClient)
            {
                BallManager[] pList = FindObjectsOfType<BallManager>();
                for (int i = 0; i < pList.Length; i++)
                {
                    pList[i].GetComponent<PhotonView>().RPC("FinishStrokes", RpcTarget.All);
                }
            }
        }

        if (PhotonNetwork.IsMasterClient)
            CheckDesynchronization();
    }

    private void Update()
    {
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                currentHole++;
                GetComponent<PhotonView>().RPC("APlayerWon", RpcTarget.AllViaServer, 0);
            }
        }
    }

    public void ResetPosition()
    {
        Vector3 spawnPos;
        if(isDefending)
            spawnPos= holes[currentHole].defenderSpawns[currentSpawner].position;
        else
            spawnPos = holes[currentHole].attackerSpawns[currentSpawner].position;
        spawnPos += new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));

        playerInstance.GetComponent<Rigidbody>().position = spawnPos;
        playerInstance.transform.position = spawnPos;
        playerInstance.GetComponent<PhotonView>().RPC("ChangePos", RpcTarget.Others, playerInstance.transform.position);
    }

    public void ResetPosition(Transform target)
    {
        Vector3 spawnPos;
        if (isDefending)
            spawnPos = holes[currentHole].defenderSpawns[currentSpawner].position;
        else
            spawnPos = holes[currentHole].attackerSpawns[currentSpawner].position;
        spawnPos += new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));

        target.GetComponent<Rigidbody>().position = spawnPos;
        target.position = spawnPos;
    }

    public void MyBallWon(bool shouldNotifyBall)
    {
        dirArrowInstance.SetActive(false);
        roleAcclarationText.gameObject.SetActive(true);
        roleAcclarationText.text = "You are inside!\n<size=15>Wait for the other attackers to finish. Use <color=#FFBD00>Q</color> or <color=#FFBD00>E</color> to spectate</size>";
        
        if (shouldNotifyBall)
            playerInstance.GetComponent<BallManager>().BallFinished(false);
        
        GetComponent<PhotonView>().RPC("APlayerWon", RpcTarget.AllViaServer, playerInstance.GetComponent<BallManager>().strokes);
        GetComponent<PhotonView>().RPC("ReproduceSound", RpcTarget.All,0,1f);
        playerInstance.GetComponent<PhotonView>().RPC("DeactivateBall", RpcTarget.All);
    }

    public IEnumerator MovePlayerToNextHole()
    {
        
        scoreMenu.SetActive(true);
        
        rules.timeInSeconds = originalTimeLimit;
        if (currentHole >= holesUsed)
        {
            SaveInfo();

            Debug.Log("GameFinished");
            if (PhotonNetwork.IsMasterClient)
                GetComponent<PhotonView>().RPC("GetFinalScores", RpcTarget.MasterClient, GetPoints(true), GetPoints(false));
            yield return new WaitForSeconds(10);
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel(0);
            }
        }
        else
        {
            scoreMenu.transform.GetChild(0).GetComponent<ScrollRect>().content.localPosition = GetPositionOfObjectInScore(currentHole);
            finishedPlayers = 0;

            
           // UpdatePassives();

            yield return new WaitForSeconds(3);

            dirArrowInstance.SetActive(true);

            SetRoleByTeam();

            Vector3 spawnPos;
            if (isDefending)
            {
                currentSpawner = Random.Range(0, holes[currentHole].defenderSpawns.Length);
                spawnPos = holes[currentHole].defenderSpawns[currentSpawner].position;
            }
            else
            {
                currentSpawner = Random.Range(0, holes[currentHole].attackerSpawns.Length);
                spawnPos = holes[currentHole].attackerSpawns[currentSpawner].position;
            }
            spawnPos += new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));

            playerInstance.GetComponent<BallManager>().isDefending = isDefending;

            timeLeft = 0;

            scoreMenu.SetActive(false);

            playerInstance.transform.position = spawnPos;
            playerInstance.GetComponent<Rigidbody>().position = spawnPos;
            controller.SetLastPos(spawnPos);
            playerInstance.GetComponent<PhotonView>().RPC("ResetBall", RpcTarget.All);
            controller.instanceToFollow = playerInstance.transform;
        }
    }



    /*    public void GetFinalScores(int[] redPoints, int[] bluePoints)
        {
            for (int i = 0; i < holesUsed; i++)
            {
                if (holes[i].blueTeamStrokes < bluePoints[i])
                {
                    holes[i].blueTeamStrokes = bluePoints[i];
                }
                if (holes[i].redTeamStrokes < redPoints[i])
                {
                    holes[i].redTeamStrokes = redPoints[i];
                }

                holes[i].redTeamStrokeTexts.text = holes[i].redTeamStrokes.ToString();
                holes[i].blueTeamStrokeTexts.text = holes[i].blueTeamStrokes.ToString();
            }

            if (PhotonNetwork.IsMasterClient)
            {
                GetComponent<PhotonView>().RPC("GetFinalScores",RpcTarget.Others, GetPoints(true), GetPoints(false));
            }
        }*/
    [PunRPC]
    public void GetFinalScores(int[] redPoints, int[] bluePoints)
    {
        for (int i = 0; i < holesUsed; i++)
        {

            holes[i].blueTeamStrokes = bluePoints[i];
            holes[i].redTeamStrokes = redPoints[i];

            if(holes[i].redTeamStrokes>-6)
                holes[i].redTeamStrokeTexts.text = holes[i].redTeamStrokes.ToString();
            if (holes[i].blueTeamStrokes > -6)
                holes[i].blueTeamStrokeTexts.text = holes[i].blueTeamStrokes.ToString();
        }

        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("GetFinalScores", RpcTarget.Others, GetPoints(true), GetPoints(false));
        }
    }

    private int[] GetPoints(bool isRedTeam)
    {
        int[] result = new int[holesUsed];
        if (isRedTeam)
        {
            for (int i = 0; i < holesUsed; i++)
            {
                result[i] = holes[i].redTeamStrokes;
            }
        }else
        {
            for (int i = 0; i < holesUsed; i++)
            {
                result[i] = holes[i].blueTeamStrokes;
            }
        }

        return result;
    }

    [PunRPC]
    public void APlayerWon(int strokes)
    {
        if (Tutorial.main != null)
            Tutorial.main.HoleCompleted();
        BallManager[] pList = FindObjectsOfType<BallManager>();
        attackingPlayers = 0;
        for (int i = 0; i < pList.Length; i++)
        {
            if (!pList[i].isDefending)
            {
                attackingPlayers++;
            }
        }

        if (firstRound)
        {
            
            if (strokes != 0)
            {
                if (holes[currentHole].blueTeamStrokes == -69)
                    holes[currentHole].blueTeamStrokes = 0;
                holes[currentHole].blueTeamStrokes += strokes;
                holes[currentHole].blueTeamStrokeTexts.text = holes[currentHole].blueTeamStrokes.ToString();
                UpdateTotalPoints(false);
            }
        }
        else
        {
            
            if (strokes != 0)
            {
                if (holes[currentHole].redTeamStrokes == -69)
                    holes[currentHole].redTeamStrokes = 0;
                holes[currentHole].redTeamStrokes += strokes;
                holes[currentHole].redTeamStrokeTexts.text = holes[currentHole].redTeamStrokes.ToString();
                UpdateTotalPoints(true);
            }
        }


        finishedPlayers++;
        if (finishedPlayers >= attackingPlayers)
        {
            if (firstRound)
            {
                firstRound = !firstRound;
            }
            else
            {
                currentHole++;
                firstRound = true;
            }

            if (pList.Length <= 1)
            {
                firstRound = true;
                isDefending = false;
                redTeam = false;
                currentHole++;
            }

            if (PhotonNetwork.IsMasterClient&&currentHole< holesUsed)
                GetComponent<PhotonView>().RPC("SetGameState", RpcTarget.AllViaServer, currentHole, holes[currentHole].blueTeamStrokes, holes[currentHole].redTeamStrokes, firstRound);
            else if(PhotonNetwork.IsMasterClient)
                GetComponent<PhotonView>().RPC("SetGameState", RpcTarget.AllViaServer, currentHole, holes[currentHole-1].blueTeamStrokes, holes[currentHole-1].redTeamStrokes, firstRound);

            for (int i = 0; i < pList.Length; i++)
            {
                pList[i].strokes = 0;
            }
        }
        
        
    }

    [PunRPC]
    public void SetRules(string sRules)
    {
        rules = JsonUtility.FromJson<Rules>(sRules);
        if (rules == null)
        {
            Debug.Log(sRules);
            return;
        }
            
        timeLeft = 0;
        originalTimeLimit = rules.timeInSeconds;
        //playerInstance.GetComponent<PhotonView>().RPC("SetAwake", RpcTarget.AllBuffered);
        controller.SetAwake();
        SetInitialScoreboard();
        StartMovement();
    }

    [PunRPC]
    public void SetLimitTime(float time)
    {
        rules.timeInSeconds = time;
    }

    [PunRPC]
    public void SetGameState(int newHole, int blueStrokes, int redStrokes, bool hostFirstRound)
    {
        currentHole = newHole;
        if (currentHole >= holesUsed)
        {
            StartCoroutine(MovePlayerToNextHole());
            return;
        }

        holes[currentHole].blueTeamStrokes = blueStrokes;
    //    holes[currentHole].blueTeamStrokeTexts.text = holes[currentHole].blueTeamStrokes.ToString();
        UpdateTotalPoints(false);

        holes[currentHole].redTeamStrokes = redStrokes;
    //    holes[currentHole].redTeamStrokeTexts.text = holes[currentHole].redTeamStrokes.ToString();
        UpdateTotalPoints(true);
        firstRound = hostFirstRound;

        
        StartCoroutine(MovePlayerToNextHole());
    }

    public int GetHole()
    {
        return currentHole;
    }

    public void ChangeAimColor(Color color, int index)
    {
        targetTransform[index].GetComponent<Image>().color = color;
    }

    internal void AddInfoValue(string info, float value)
    {
        switch (info)
        {
            case "shot":
                gameInfo.shotsMade += (int)value;
                break;
            case "skill":
                gameInfo.skillsUsed += (int)value;
                break;
        }
    }

    private void UpdateTotalPoints(bool newRedTeam)
    {
        int total = 0;
        for (int i = 0; i < holesUsed; i++)
        {
            if (newRedTeam)
            {
                if (holes[i].redTeamStrokes != -69)
                    total += holes[i].redTeamStrokes;
            }
                
            else
            {
                if(holes[i].blueTeamStrokes!=-69)
                    total += holes[i].blueTeamStrokes;
            }
                
        }
        if (newRedTeam)
            redTotalPoints.text = total.ToString();
        else
            blueTotalPoints.text = total.ToString();
    }

    private void SetInitialScoreboard()
    {
        GameObject holePrefab = scoresList.GetChild(0).gameObject;
        holesUsed = holes.Length;
        if (holesUsed > rules.totalHoles)
            holesUsed = rules.totalHoles;
        for (int i = 0; i < holesUsed; i++)
        {
            GameObject instance = Instantiate(holePrefab, scoresList);
            instance.transform.GetChild(3).GetComponent<Text>().text = (i + 1).ToString();
            holes[i].blueTeamStrokeTexts = instance.transform.GetChild(4).GetComponent<Text>();
            holes[i].redTeamStrokeTexts = instance.transform.GetChild(5).GetComponent<Text>();
        }
        holePrefab.SetActive(false);
    }

    private void SaveInfo()
    {
        if (Application.isEditor)
        {
            PlayerPrefs.SetString("lastDayPlayed", Newtonsoft.Json.JsonConvert.SerializeObject(System.DateTime.Today));
            PlayerPrefs.SetInt("gamePlayed", 1);
        }
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2||holesUsed<8)
            return;
        gameInfo.gamesPlayed++;
        
        int redPoints = int.Parse(redTotalPoints.text), bluePoints = int.Parse(blueTotalPoints.text);
        int ballsEarned = 20+10* holesUsed;
        if (redTeam)
        {
            if (redPoints < bluePoints)
            {
                gameInfo.gamesWon++;
                ballsEarned = 40 + 20 * holesUsed;
            }
        }
        else
        {
            if (redPoints > bluePoints)
            {
                gameInfo.gamesWon++;
                ballsEarned = 40 + 20 * holesUsed;
            }   
        }
        PlayerPrefs.SetInt("gamePlayed", 1);
        PlayerPrefs.SetInt("currentBalls", PlayerPrefs.GetInt("currentBalls") + ballsEarned);
        gameInfo.timesCharacterWasUsed[characterSelected]++;
        PlayerPrefs.SetString("stats", JsonUtility.ToJson(gameInfo));
    }

    private void CheckDesynchronization()
    {
        for (int i = 0; i < currentHole; i++)
        {
            if (holes[i].blueTeamStrokes == -69) 
            {
                currentHole = i;
                firstRound = true;
                GetComponent<PhotonView>().RPC("SetGameState", RpcTarget.AllViaServer, currentHole, holes[currentHole].blueTeamStrokes, holes[currentHole].redTeamStrokes, firstRound);
                SendError("There was a networking error, going back to a previous level. If you think this is a bug let us know.");
                return;
            }
            if(holes[i].redTeamStrokes == -69&&PhotonNetwork.CurrentRoom.PlayerCount>1)
            {
                currentHole = i;
                firstRound = false;
                GetComponent<PhotonView>().RPC("SetGameState", RpcTarget.AllViaServer, currentHole, holes[currentHole].blueTeamStrokes, holes[currentHole].redTeamStrokes, firstRound);
                SendError("There was a networking error, going back to a previous level. If you think this is a bug let us know.");
                return;
            }
        }

        if (!firstRound)
        {
            if (holes[currentHole].blueTeamStrokes == -69)
            {
                firstRound = true;
                GetComponent<PhotonView>().RPC("SetGameState", RpcTarget.AllViaServer, currentHole, holes[currentHole].blueTeamStrokes, holes[currentHole].redTeamStrokes, firstRound);
                SendError("There was a networking error, going back to a previous level. If you think this is a bug let us know.");
                return;
            }
        }
    }

    private void SendError(string message)
    {
        errorText.gameObject.SetActive(true);
        errorText.text = message;
    }



    public void LeaveGame()
    {
        PhotonNetwork.LeaveRoom();
        
    }

    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void ForfeitHole()
    {
        playerInstance.GetComponent<PhotonView>().RPC("FinishStrokes", RpcTarget.All);
    }

    public void SetVolume(float value)
    {
        //   QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetFloat("volume", value);
        audioMixer.SetFloat("masterVol", Mathf.Log10(value) * 20);
    }
    public void SetVolumeSFX(float value)
    {
        //   QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetFloat("sfxVol", value);
        audioMixer.SetFloat("sfxVol", Mathf.Log10(value) * 20);
    }
    public void SetMusicVolume(float value)
    {
        //   QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetFloat("musicVol", value);
        audioMixer.SetFloat("musicVol", Mathf.Log10(value) * 20);
    }
    public void SetVolumeAmbient(float value)
    {
        //   QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetFloat("ambientVol", value);
        audioMixer.SetFloat("ambientVol", Mathf.Log10(value) * 20);
    }

    private void SetVolumes()
    {
        if (PlayerPrefs.HasKey("volume"))
        {
            volume.value = PlayerPrefs.GetFloat("volume");
            audioMixer.SetFloat("masterVol", Mathf.Log10(PlayerPrefs.GetFloat("volume")) * 20);
        }
        if (PlayerPrefs.HasKey("musicVol"))
        {
            musicVol.value = PlayerPrefs.GetFloat("musicVol");
            audioMixer.SetFloat("musicVol", Mathf.Log10(PlayerPrefs.GetFloat("musicVol")) * 20);
        }
        if (PlayerPrefs.HasKey("sfxVol"))
        {
            sfxVol.value = PlayerPrefs.GetFloat("sfxVol");
            audioMixer.SetFloat("sfxVol", Mathf.Log10(PlayerPrefs.GetFloat("sfxVol")) * 20);
        }
        if (PlayerPrefs.HasKey("ambientVol"))
        {
            ambientVol.value = PlayerPrefs.GetFloat("ambientVol");
            audioMixer.SetFloat("ambientVol", Mathf.Log10(ambientVol.value) * 20);
        }
    }

    private void SetRoleByTeam()
    {
        if (redTeam)
        {
            if (firstRound)
                isDefending = true;
            else
                isDefending = false;
        }
        else
        {
            if (firstRound)
                isDefending = false;
            else
                isDefending = true;
        }

    }

    private void StartMovement()
    {
        RotatingObject[] rotatingObjects = FindObjectsOfType<RotatingObject>();
        if (rotatingObjects != null)
        {
            for (int i = 0; i < rotatingObjects.Length; i++)
            {
                rotatingObjects[i].StartMovement();
            }
        }
    }

    private Vector2 GetPositionOfObjectInScore(int pos)
    {
        if (pos > holes.Length)
            return Vector2.zero;
        Canvas.ForceUpdateCanvases();
        ScrollRect scrollRect = scoreMenu.transform.GetChild(0).GetComponent<ScrollRect>();
        Vector2 viewportLocalPosition = scrollRect.viewport.localPosition;
        Vector2 childLocalPosition = holes[pos].blueTeamStrokeTexts.transform.parent.localPosition;
        Vector2 result = new Vector2(
            0 - (viewportLocalPosition.x + childLocalPosition.x),
            scrollRect.content.localPosition.y/*0 - (viewportLocalPosition.y + childLocalPosition.y)*/
        );
        return result;
    }
}

[System.Serializable]
public class Hole
{
    public int blueTeamStrokes=-69, redTeamStrokes=-69;
    public Transform[] defenderSpawns, attackerSpawns;
    public Text blueTeamStrokeTexts,redTeamStrokeTexts;
}

[System.Serializable]
public class Stats
{
    public int gamesPlayed, gamesWon, shotsMade, skillsUsed;
    public int[] timesCharacterWasUsed= new int[1];
}