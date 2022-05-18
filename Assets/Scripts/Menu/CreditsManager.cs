using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditsManager : MonoBehaviour
{
    public Credits credits;

    public string versionSizes, titleSizes, normalFontSize, bigResult;

    private ScrollRect scrollRect;
    private GameObject credPrefab;

    private void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        credPrefab = scrollRect.content.GetChild(0).gameObject;
        string result = "";
        bigResult = "";
        GameObject instance;

        result += "<b><size=" + titleSizes + ">Game designers</size></b>\n <size=" + normalFontSize + ">";
        foreach (string a in credits.gameDesign)
        {
            result += " - " + a + "\n";
        }
        result += "</size>";
        instance = Instantiate(credPrefab, scrollRect.content);
        instance.GetComponent<Text>().text = result;
        bigResult += result;
        result = "";

        result += "<b><size=" + titleSizes + ">Programmers</size></b>\n<size=" + normalFontSize + ">";

        foreach (string a in credits.programming)
        {
            result += " - " + a + "\n";
        }
        result += "</size>";
        instance = Instantiate(credPrefab, scrollRect.content);
        instance.GetComponent<Text>().text = result;
        bigResult += result;
        result = "";

        result += "<b><size=" + titleSizes + ">UI design</size></b>\n<size=" + normalFontSize + ">";

        foreach (string a in credits.uiDesign)
        {
            result += " - " + a + "\n";
        }
        result += "</size>";
        instance = Instantiate(credPrefab, scrollRect.content);
        instance.GetComponent<Text>().text = result;
        bigResult += result;
        result = "";

        result += "<b><size=" + titleSizes + ">Modellers</size></b>\n<size=" + normalFontSize + ">";

        foreach (string a in credits.modelling)
        {
            result += " - " + a + "\n";
        }
        result += "</size>";
        instance = Instantiate(credPrefab, scrollRect.content);
        instance.GetComponent<Text>().text = result;
        bigResult += result;
        result = "";

        result += "<b><size=" + titleSizes + ">Playtester</size></b>\n<size=" + normalFontSize + ">";

        foreach (string a in credits.playtesting)
        {
            result += " - " + a + "\n";
        }
        result += "</size>";
        instance = Instantiate(credPrefab, scrollRect.content);
        instance.GetComponent<Text>().text = result;
        bigResult += result;
        result = "";

        result += "<b><size=" + titleSizes + ">Supporters</size></b>\n<size=" + normalFontSize + ">";

        foreach (string a in credits.supporters)
        {
            result += " - " + a + "\n";
        }
        result += "</size>";
        instance = Instantiate(credPrefab, scrollRect.content);
        instance.GetComponent<Text>().text = result;
        bigResult += result;
        result = "";

        result += "<b><size=" + titleSizes + ">Special thanks</size></b>\n<size=" + normalFontSize + ">";

        foreach (string a in credits.specialThanks)
        {
            result += " - " + a + "\n";
        }
        result += "</size>";
        instance = Instantiate(credPrefab, scrollRect.content);
        instance.GetComponent<Text>().text = result;
        bigResult += result;

        bigResult += "\n";


        credPrefab.SetActive(false);
    }
}

[System.Serializable]
public class Credits
{
    public string[] gameDesign, programming, uiDesign, modelling, playtesting, specialThanks, supporters;
}