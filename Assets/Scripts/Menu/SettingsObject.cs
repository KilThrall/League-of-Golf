using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class SettingsObject : MonoBehaviour
{
    public Slider slider;
    public Text nameText, valueText;
    public Toggle toggle;
    public Dropdown dropdown;

    public float minValue, maxValue, defaultValue;
    public bool isInt, defBoolValue;

    private void Awake()
    {
        nameText.text = gameObject.name;

        if (slider != null)
        {
            slider.maxValue = maxValue;
            slider.minValue = minValue;
            slider.value = defaultValue;
            slider.wholeNumbers = isInt;
            string comaValue = "F2";
            if (isInt)
                comaValue = "F0";
            valueText.text = defaultValue.ToString(comaValue);
        }

        if (toggle != null)
        {
            toggle.isOn = defBoolValue;
        }
        
    }

    public void MoveSlider(float value)
    {
        SetValues(value, true);
    }

    public void ChangeToggle(bool state)
    {
        SetValues(state, true);
    }

    public void ChangeDropdown(int value)
    {
        SetValues(value, true);
    }

    public void SetValues(float value, bool changeRules)
    {
        if (slider == null)
            return;
        if(!changeRules)
            slider.value = value;

        string comaValue = "F2";
        if (isInt)
            comaValue = "F0";
        valueText.text = value.ToString(comaValue);

        if (!changeRules)
            return;

        if (MainMenu.main.playerInstance == null)
        {
           // SetValues(value, changeRules);
            return;
        }
            

        Rules newRules = MainMenu.main.playerInstance.gameRules;

        switch (gameObject.name)
        {
            case "Number of strokes":
                newRules.maxStrokes = (int)value;
                break;
            case "Bounciness":
                newRules.bounciness = value;
                break;
            case "Friction":
                newRules.friction = value;
                break;
            case "Max power":
                newRules.maxPower = value;
                break;
            case "Jump power":
                newRules.jumpPower = value;
                break;
            case "Gravity":
                newRules.gravity = value;
                break;
            case "Round time":
                newRules.timeInSeconds = value;
                break;
            case "Respawn time":
                newRules.respawnTime = value;
                break;
            case "Reset time":
                newRules.resetTime = value;
                break;
            case "Strokes used on reset":
                newRules.resetPrice = (int)value;
                break;
            case "Number of holes":
                newRules.totalHoles = (int)value;
                break;
        }
        string rulesS = JsonUtility.ToJson(newRules);

        if (PhotonNetwork.IsMasterClient)
            MainMenu.main.playerInstance.GetComponent<PhotonView>().RPC("RulesChanged", RpcTarget.AllBuffered, rulesS);
    }

    public void SetValues(bool state, bool changeRules)
    {
        if (toggle == null)
            return;

        if (!changeRules)
            toggle.isOn = state;

        if (!changeRules)
            return;

        Rules newRules = MainMenu.main.playerInstance.gameRules;

        switch (gameObject.name)
        {
            case "Any hole works":
                newRules.anyHoleWorks = state;
                break;
            case "Can shoot outside of the map":
                newRules.shootOutside = state;
                break;
            case "Public game":
                newRules.isPublic = state;
                break;
        }
        string rulesS = JsonUtility.ToJson(newRules);

        if(PhotonNetwork.IsMasterClient)
            MainMenu.main.playerInstance.GetComponent<PhotonView>().RPC("RulesChanged", RpcTarget.AllBuffered, rulesS);
    }

    public void SetValues(int value, bool changeRules)
    {
        if (dropdown == null)
            return;

        if (!changeRules)
            dropdown.SetValueWithoutNotify(value);

        if (!changeRules)
            return;

        Rules newRules = MainMenu.main.playerInstance.gameRules;

        switch (gameObject.name)
        {
            case "Map":
                newRules.levelSelected = value;
                break;
        }
        string rulesS = JsonUtility.ToJson(newRules);

        if (PhotonNetwork.IsMasterClient)
            MainMenu.main.playerInstance.GetComponent<PhotonView>().RPC("RulesChanged", RpcTarget.AllBuffered, rulesS);
    }

    public void SetInteractable(bool state)
    {
        if (slider != null)
        {
            slider.interactable = state;
        }

        if (toggle != null)
        {
            toggle.interactable = state;
        }

        if (dropdown != null)
        {
            dropdown.interactable = state;
        }
    }
}
