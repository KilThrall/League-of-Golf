using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerDetails : MonoBehaviour
{
    public Dropdown classDropdown;
    public Text nickname;
    public Button readyButton;

    internal bool redTeam = false, ready=false;

    internal PhotonView pView;
    internal Rules gameRules= new Rules();

    private void Start()
    {
        pView = GetComponent<PhotonView>();
        nickname.text = pView.Owner.NickName;
        if (pView.IsMine)
        {
            readyButton.interactable = true;
            classDropdown.interactable = true;
            readyButton.onClick.AddListener(SwitchReady);
            if (PlayerPrefs.HasKey("characterSelected"))
                pView.RPC("SetClass", RpcTarget.AllBuffered, PlayerPrefs.GetInt("characterSelected"));
            
        }

        if (pView.IsMine && PhotonNetwork.IsMasterClient) 
            MainMenu.main.ChangeRules(gameRules);
    }

    public void DropdownOpened()
    {
        if (pView.IsMine)
        {
            for (int i = 0; i < MainMenu.main.characters.Length; i++)
            {

                if (PlayerPrefs.GetInt("characterBought" + i.ToString()) == 0)
                {
                    classDropdown.transform.GetChild(4).GetComponent<ScrollRect>().content.GetChild(i + 1).GetComponent<Toggle>().interactable = false;
                }
                    
            }
        }
    }

    [PunRPC]
    public void SetClass(int value)
    {
        classDropdown.value = value;
    }

    [PunRPC]
    public void ChangeClass(int value)
    {
        if (pView.IsMine)
        {
            PlayerPrefs.SetInt("characterSelected",value);
            pView.RPC("ChangeClass", RpcTarget.OthersBuffered, value);
        }
            
        else
            classDropdown.value = value;
    }

    [PunRPC]
    public void SwitchTeams()
    {
        redTeam = !redTeam;
        if (redTeam)
            transform.SetParent(MainMenu.main.redTeamList);
        else
            transform.SetParent( MainMenu.main.blueTeamList);

        if (pView == null)
            pView = GetComponent<PhotonView>();

        if (pView.IsMine)
        {
            if(!redTeam)
                PlayerPrefs.SetInt("redTeam",0);
            else
                PlayerPrefs.SetInt("redTeam", 1);
        }
    }

    [PunRPC]
    public void SwitchReady()
    {
        ready = !ready;
        if (ready)
            readyButton.GetComponent<Image>().color = Color.green;
        else
            readyButton.GetComponent<Image>().color = Color.red;
        if (pView.IsMine)
            pView.RPC("SwitchReady", RpcTarget.OthersBuffered);
    }

    [PunRPC]
    public void RulesChanged(string values)
    {
        Rules rules=JsonUtility.FromJson<Rules>(values);
        gameRules = rules;
        PlayerPrefs.SetString("rules", values);
        MainMenu.main.ChangeRules(rules);
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.CurrentRoom.IsVisible = rules.isPublic;
    }
}
