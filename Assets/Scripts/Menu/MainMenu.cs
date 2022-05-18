using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Audio;

public class MainMenu : MonoBehaviourPunCallbacks
{
    public static MainMenu main;

    public string gameVersion;
    public int tutorialLevel;
    public AudioMixer audioMixer;

    [Header("UI")]
    public Animator mainAnim;
    public Text statusMessage;
    public InputField playerName, firstTimePlayerName, lobbyName;
    public GameObject retryButton, lobbyMenu, playerPrefab, mainMenu, tutorialWindow, tutorialButton, firstTimeNameSelector;
    public Transform blueTeamList, redTeamList, settingsList;
    public Image lastCharSelectedImage;
    public Text lastCharSelectedText, selectedRegionText, roomNameText,playerAmountText, ballsAmountText, quartersAmountText, versionText;
    public Character[] characters;
    public Animator patchnotsAnim;

    [Header("UI Settings")]
    public Toggle fullscreen;
    public Dropdown quality, resolution, regionDropdown;
    public Slider volume, sfxVol, ambientVol, musicVol;

    internal PlayerDetails playerInstance;

    private bool onLobby, gameStarted = false, interactable = false, volumesSet = false, regionChanged = false, changeVisible = false;
    private Resolution[] resolutions;
    private int currentBalls, currentQuarters;

    private void Awake()
    {
        if (main == null)
            main = this;
        else
            Destroy(this);

        

        PhotonNetwork.AutomaticallySyncScene = true;

        
        PhotonNetwork.ConnectUsingSettings();
        

        statusMessage.text = "Connecting... ";

        PhotonNetwork.GameVersion = gameVersion;
        Application.targetFrameRate = 60;
        versionText.text = "v"+gameVersion;


        if (!PlayerPrefs.HasKey("characterBought0"))
        {
            for (int i = 0; i < characters.Length; i++)
            {
                PlayerPrefs.SetInt("characterBought" + i.ToString(), characters[i].bought);
            }
        }

        if (!PlayerPrefs.HasKey("currentBalls"))
        {
            Stats stats=null;
            if (PlayerPrefs.HasKey("stats"))
                stats = JsonUtility.FromJson<Stats>(PlayerPrefs.GetString("stats"));
            currentBalls = 2000;
            if (stats != null)
                currentBalls += stats.gamesPlayed * 150+stats.gamesWon*150;
            PlayerPrefs.SetInt("currentBalls", currentBalls);
        }
        else
        {
            currentBalls = PlayerPrefs.GetInt("currentBalls");
            /*if (Application.isEditor)
            {
                GiveBalls(2000);
            }*/
        }
        ballsAmountText.text = currentBalls.ToString();

        if (!PlayerPrefs.HasKey("lastPatchnotes"))
        {
            PlayerPrefs.SetString("lastPatchnotes", gameVersion);
            patchnotsAnim.SetTrigger("change");
        }
        else
        {
            if (PlayerPrefs.GetString("lastPatchnotes")!=gameVersion)
            {
                PlayerPrefs.SetString("lastPatchnotes", gameVersion);
                patchnotsAnim.SetTrigger("change");
            }
        }

        

        if (PhotonNetwork.InLobby||PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (PlayerPrefs.HasKey("rules"))
        {
            PlayerPrefs.DeleteKey("rules");
        }

        //ShowLastCharacter();

        SetSettings();
        
    }

    private void Update()
    {
        if (!volumesSet)
            SetVolumes();
        if (!onLobby)
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
                JoinLobby();
        }

        if (!onLobby||playerInstance==null)
            return;
        if (redTeamList.childCount > 5||playerInstance.redTeam)
            redTeamList.GetChild(0).gameObject.SetActive(false);
        else
            redTeamList.GetChild(0).gameObject.SetActive(true);

        if (blueTeamList.childCount > 5 || !playerInstance.redTeam)
            blueTeamList.GetChild(0).gameObject.SetActive(false);
        else
            blueTeamList.GetChild(0).gameObject.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
        {
            PlayerDetails[] pList = FindObjectsOfType<PlayerDetails>();
            bool allReady = true;
            for (int i = 0; i < pList.Length; i++)
            {
                if (!pList[i].ready)
                    allReady = false;
            }
            if (allReady&&pList.Length>0&&!gameStarted)
            {
                StartGame();
            }

            if (!interactable)
            {
                interactable = true;
                for (int i = 0; i < settingsList.childCount; i++)
                {
                    settingsList.GetChild(i).GetComponent<SettingsObject>().SetInteractable(true);
                }
            }
        }
    }

    private void ShowLastCharacter()
    {
        if (PlayerPrefs.HasKey("characterSelected"))
        {
            int value = PlayerPrefs.GetInt("characterSelected");
            if (value < characters.Length)
            {
                Character chara = characters[value];
                lastCharSelectedImage.sprite = chara.image;
                string slogan = chara.slogans[Random.Range(0, chara.slogans.Length)];
                lastCharSelectedText.text = "Your last character selected was " + chara.name + '\n' + '\n' + slogan;
            }

        }
        else
        {
            lastCharSelectedImage.gameObject.SetActive(false);
            lastCharSelectedText.text = "You have never played a game before. Go play a game. Trust me. It will be fun";
        }
    }

    private void SetSettings()
    {
        resolutions = Screen.resolutions;
        resolution.ClearOptions();

        List<string> options = new List<string>();

        int myRes = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width.ToString() + "x" + resolutions[i].height.ToString();
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                myRes = i;
            }
        }

        resolution.AddOptions(options);
        resolution.value = myRes;
        resolution.RefreshShownValue();

        if (PlayerPrefs.HasKey("quality"))
        {
            quality.value = PlayerPrefs.GetInt("quality");
            quality.RefreshShownValue();
        }
        if (PlayerPrefs.HasKey("resolution"))
        {
            resolution.value = PlayerPrefs.GetInt("resolution");
            resolution.RefreshShownValue();
        }
        if (PlayerPrefs.HasKey("fullscreen"))
        {
            int a = PlayerPrefs.GetInt("fullscreen");
            bool result = false;
            if (a == 1)
                result = true;
            fullscreen.isOn = result;
        }
    }

    internal void GetBalls(int amount)
    {
        currentBalls += amount;
        PlayerPrefs.SetInt("currentBalls", currentBalls);
        ballsAmountText.text = currentBalls.ToString();
    }

    public override void OnConnectedToMaster()
    {
        selectedRegionText.text = "Current region: "+PhotonNetwork.CloudRegion;
        tutorialButton.SetActive(true);
        statusMessage.text = "Connected succesfully to server";
        retryButton.SetActive(false);

        SetRegionDropdown();

        if (PlayerPrefs.HasKey("nickname"))
        {
            playerName.text = PlayerPrefs.GetString("nickname");
        }
        else
        {
            firstTimeNameSelector.SetActive(true);
            var tempName = "";// "Player" + Random.Range(0, 1000).ToString("0000"); //TODO: arreglar esto
            firstTimePlayerName.text = tempName;
            NickChanged(tempName);
            PhotonNetwork.NickName = tempName;
        }

        if (!PlayerPrefs.HasKey("tutorial"))
        {
            PlayerPrefs.SetInt("tutorial", 0);
            tutorialWindow.SetActive(true);
        }
        StartCoroutine(UpdatePlayerCount());
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        tutorialButton.SetActive(false);
        statusMessage.text = "Disconnected from main server. Please retry";
        retryButton.SetActive(true);
        if (regionChanged)
        {
            PhotonNetwork.ConnectToRegion(PlayerPrefs.GetString("PreferredRegion"));
            regionChanged = false;
        }
           
    }

    public override void OnJoinedRoom()
    {
        if (PlayerPrefs.GetInt("tutorial") == 1)
        {
            PlayerPrefs.SetInt("tutorial", 0);
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel(tutorialLevel);
        }

        base.OnJoinedRoom();
        onLobby = true;

        statusMessage.text = "Joined a room";

        mainAnim.SetTrigger("Lobby");
        
        playerName.gameObject.SetActive(!onLobby);
        if (mainMenu != null)
        {
            lobbyMenu.SetActive(onLobby);
            mainMenu.SetActive(!onLobby);
        }
        
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        

        StartCoroutine(ConnectPlayer());
    }

    private IEnumerator ConnectPlayer()
    {
        yield return new WaitForSeconds(.2f);
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 10)
        {
            statusMessage.text = "Can't join that lobby. It's full";
            PhotonNetwork.LeaveRoom();
        }

        playerInstance = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity).GetComponent<PlayerDetails>();
        if (blueTeamList.childCount > redTeamList.childCount)
        {
            playerInstance.GetComponent<PhotonView>().RPC("SwitchTeams", RpcTarget.AllBuffered);
        }
        else
        {
            playerInstance.GetComponent<PhotonView>().RPC("SwitchTeams", RpcTarget.AllBuffered);
            playerInstance.GetComponent<PhotonView>().RPC("SwitchTeams", RpcTarget.AllBuffered);
        }
        playerInstance.transform.localScale = Vector3.one;
        if (PhotonNetwork.IsMasterClient)
        {
            playerInstance.GetComponent<PhotonView>().RPC("RulesChanged", RpcTarget.AllBuffered, JsonUtility.ToJson(new Rules()));
            if (changeVisible)
            {
                changeVisible = false;
                settingsList.GetChild(0).GetComponent<SettingsObject>().SetValues(false, false);
            }
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        onLobby = false;
        playerName.gameObject.SetActive(!onLobby);
        if (mainMenu != null)
        {
            lobbyMenu.SetActive(onLobby);
            
            mainMenu.SetActive(!onLobby);
        }

        statusMessage.text = "Left a room";

        mainAnim.SetTrigger("Main");

        if (interactable)
        {
            interactable = false;
            for (int i = 0; i < settingsList.childCount; i++)
            {
                settingsList.GetChild(i).GetComponent<SettingsObject>().SetInteractable(false);
            }
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRandomRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        SoundForButtons.main.ReproduceOther(0, 0.8f);
    }

    public void LeaveLobby()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void RetryConnection()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        statusMessage.text = "Connecting... ";
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    private void CreateRandomRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 10;
        roomOptions.IsVisible = true;
        PhotonNetwork.CreateRoom("Room" + Random.Range(0, 50000).ToString(), roomOptions, null);
    }

    public void QuickMatch()
    {
        PhotonNetwork.NickName = playerName.text;

        PhotonNetwork.JoinRandomRoom();
    }

    public void CreateLobby()
    {
        PhotonNetwork.NickName = playerName.text;
        if (lobbyName.text.Length < 2)
        {
            statusMessage.text = "Can't use that name. It's too short";
            return;
        }
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 10;
        roomOptions.IsVisible = false;
        changeVisible = true;

        bool joined = PhotonNetwork.CreateRoom(lobbyName.text,roomOptions);
        if (!joined)
            statusMessage.text = "Please wait until connected";
    }

    public void JoinLobby()
    {
        PhotonNetwork.NickName = playerName.text;
        if (lobbyName.text.Length < 2)
        {
            statusMessage.text = "Can't use that name. It's too short";
            return;
        }

        bool joined = PhotonNetwork.JoinRoom(lobbyName.text);
        if(!joined)
            statusMessage.text = "Please wait until connected";
        
    }

    public void NickChanged(string nick)
    {
        if (name.Length < 2)
            return;
        PlayerPrefs.SetString("nickname", nick);
        PhotonNetwork.NickName = nick;
        if (nick == "supercalifragilisticoespialidoso")
        {
            GetBalls(1000);
        }
    }

    public void SwitchTeams()
    {
        playerInstance.pView.RPC("SwitchTeams", RpcTarget.AllBuffered);
    }

    public void ChangeRules(Rules newRules)
    {
        if (PhotonNetwork.IsMasterClient)
            return;
        settingsList.GetChild(0).GetComponent<SettingsObject>().SetValues(newRules.isPublic, false);
        settingsList.GetChild(1).GetComponent<SettingsObject>().SetValues(newRules.anyHoleWorks, false);
        settingsList.GetChild(2).GetComponent<SettingsObject>().SetValues(newRules.shootOutside, false);
        settingsList.GetChild(3).GetComponent<SettingsObject>().SetValues(newRules.levelSelected, false);
        settingsList.GetChild(4).GetComponent<SettingsObject>().SetValues(newRules.totalHoles, false);
        settingsList.GetChild(5).GetComponent<SettingsObject>().SetValues(newRules.maxStrokes,false);
        settingsList.GetChild(6).GetComponent<SettingsObject>().SetValues(newRules.bounciness, false);
        settingsList.GetChild(7).GetComponent<SettingsObject>().SetValues(newRules.friction, false);
        settingsList.GetChild(8).GetComponent<SettingsObject>().SetValues(newRules.maxPower, false);
        settingsList.GetChild(9).GetComponent<SettingsObject>().SetValues(newRules.jumpPower, false);
        settingsList.GetChild(10).GetComponent<SettingsObject>().SetValues(newRules.gravity, false);
        settingsList.GetChild(11).GetComponent<SettingsObject>().SetValues(newRules.timeInSeconds, false);
        settingsList.GetChild(12).GetComponent<SettingsObject>().SetValues(newRules.respawnTime, false);
        settingsList.GetChild(13).GetComponent<SettingsObject>().SetValues(newRules.resetTime, false);
        settingsList.GetChild(14).GetComponent<SettingsObject>().SetValues(newRules.resetPrice, false);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("quality", qualityIndex);
    }
    public void SetVolume(float value)
    {
     //   QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetFloat("volume", value);
        audioMixer.SetFloat("masterVol", Mathf.Log10(value) * 20);
    }
    public void SetVolumeSFX(float value)
    {
        PlayerPrefs.SetFloat("sfxVol", value);
        audioMixer.SetFloat("sfxVol", Mathf.Log10(value) * 20);
    }
    public void SetVolumeAmbient(float value)
    {
        PlayerPrefs.SetFloat("ambientVol", value);
        audioMixer.SetFloat("ambientVol", Mathf.Log10(value) * 20);
    }
    public void SetVolumeMusic(float value)
    {
        PlayerPrefs.SetFloat("musicVol", value);
        audioMixer.SetFloat("musicVol", Mathf.Log10(value) * 20);
    }
    public void SetResolution(int resolutionVal)
    {
      //  QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("resolution", resolutionVal);
        Screen.SetResolution(resolutions[resolutionVal].width, resolutions[resolutionVal].height,Screen.fullScreen);
    }
    public void SetFullscreen(bool val)
    {
        // QualitySettings.SetQualityLevel(qualityIndex);
        int a = 0;
        if (val)
            a = 1;
        PlayerPrefs.SetInt("fullscreen", a);
        Screen.fullScreen = val;
    }

    public void TutorialAccepted()
    {
        Rules rules = new Rules();
        rules.timeInSeconds = 99999;
        rules.maxStrokes = 9999;
        PlayerPrefs.SetString("rules", JsonUtility.ToJson(rules));

        PlayerPrefs.SetInt("tutorial", 1);
        PlayerPrefs.SetInt("characterSelected", 7);

        
        PhotonNetwork.CreateRoom("akl"+Random.Range(0,50000).ToString());
    }

    public void SwitchRegion(int value)
    {
        string result = "";
        switch (value)
        {
            case 0:
                result = "asia";
                break;
            case 1:
                result = "au";
                break;
            case 2:
                result = "eu";
                break;
            case 3:
                result = "us";
                break;
            case 4:
                result = "sa";
                break;
            case 5:
                result = "ru";
                break;

        }
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = result;
        PlayerPrefs.SetString("PreferredRegion", result);
        regionChanged = true;
        PhotonNetwork.Disconnect();
    }

    public void BuyCharacterWithQuarters(int character)
    {
        Debug.Log("implementar en " + characters[character].name);
        if (currentQuarters >= characters[character].quarterPrice)
        {
            FindObjectOfType<CharacterLister>().UpdateList();
        }
    }

    public void BuyCharacterWithBalls(int character)
    {
        if (currentBalls >= characters[character].ballPrice)
        {
            currentBalls -= characters[character].ballPrice;
            PlayerPrefs.SetInt("characterBought" + character.ToString(), 1);
            ballsAmountText.text = currentBalls.ToString();
            FindObjectOfType<CharacterLister>().UpdateList();
            PlayerPrefs.SetInt("currentBalls", currentBalls);
        }
    }


    private void StartGame()
    {
        gameStarted = true;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;
       // PhotonNetwork.IsMessageQueueRunning = false;
        PhotonNetwork.LoadLevel(playerInstance.gameRules.levelSelected+2);
    }

    private void SetVolumes()
    {

        if (!PlayerPrefs.HasKey("volume"))
        {
            PlayerPrefs.SetFloat("volume",0.5f);
        }
        if (!PlayerPrefs.HasKey("sfxVol"))
        {
            PlayerPrefs.SetFloat("sfxVol", 0.5f);
        }
        if (!PlayerPrefs.HasKey("ambientVol"))
        {
            PlayerPrefs.SetFloat("ambientVol", 0.5f);
        }
        if (!PlayerPrefs.HasKey("musicVol"))
        {
            PlayerPrefs.SetFloat("musicVol", 0.5f);
        }

        volume.value = PlayerPrefs.GetFloat("volume");
        audioMixer.SetFloat("masterVol", Mathf.Log10(PlayerPrefs.GetFloat("volume")) * 20);

        sfxVol.value = PlayerPrefs.GetFloat("sfxVol");
        audioMixer.SetFloat("sfxVol", Mathf.Log10(PlayerPrefs.GetFloat("sfxVol")) * 20);

        ambientVol.value = PlayerPrefs.GetFloat("ambientVol");
        audioMixer.SetFloat("ambientVol", Mathf.Log10(ambientVol.value) * 20);

        musicVol.value = PlayerPrefs.GetFloat("musicVol");
        audioMixer.SetFloat("musicVol", Mathf.Log10(musicVol.value) * 20);

        volumesSet = true;
    }
    
    private void SelectPreviousRegion()
    {
        var value = PlayerPrefs.GetString("PreferredRegion");
        PhotonNetwork.ConnectToRegion(value);
        /*switch (value)
        {
            case 0:
                PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "asia";
                break;
            case 1:
                PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "au";
                break;
            case 2:
                PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "eu";
                break;
            case 3:
                PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "us";
                break;
            case 4:
                PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "sa";
                break;
            case 5:
                PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "ru";
                break;

        }*/
    }

    private void SetRegionDropdown()
    {
        switch (PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion)
        {
            case "asia":
                regionDropdown.value = 0;
                break;
            case "au":
                regionDropdown.value = 1;
                break;
            case "eu":
                regionDropdown.value = 2;
                break;
            case "us":
                regionDropdown.value = 3;
                break;
            case "sa":
                regionDropdown.value = 4;
                break;
            case "ru":
                regionDropdown.value = 5;
                break;

        }
    }

    private IEnumerator UpdatePlayerCount()
    {
        if(playerAmountText!=null)
            playerAmountText.text = "Currently connected players: <color=#FFBD00>" + PhotonNetwork.CountOfPlayers + "</color>";
        yield return new WaitForSeconds(5);
        if (playerAmountText != null)
            StartCoroutine(UpdatePlayerCount());
    }
}

[System.Serializable]
public class Character
{
    public string name;
    public string[] slogans;
    public Sprite image;
    public GameObject prefab;
    public int bought;
    public int quarterPrice, ballPrice;
}