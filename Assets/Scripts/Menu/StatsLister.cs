using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsLister : MonoBehaviour
{
    public string versionSizes, titleSizes, normalFontSize, bigResult;

    private ScrollRect scrollRect;
    private GameObject statPrefab;
    private Stats stats;

    private void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        statPrefab = scrollRect.content.GetChild(0).gameObject;
        string result = "";
        bigResult = "";
        GameObject instance;

        if (PlayerPrefs.HasKey("stats"))
        {
            stats = JsonUtility.FromJson<Stats>(PlayerPrefs.GetString("stats"));
            result += "<b><size=" + titleSizes + ">Games played</size></b>\n<size=" + normalFontSize + ">";

            result += " - " + stats.gamesPlayed.ToString()+"\n";
            result += "</size>";
            instance = Instantiate(statPrefab, scrollRect.content);
            instance.GetComponent<Text>().text = result;
            bigResult += result;
            result = "";

            result += "<b><size=" + titleSizes + ">Games won</size></b>\n<size=" + normalFontSize + ">";

            result += " - " + stats.gamesWon.ToString() + "\n";
            result += "</size>";
            instance = Instantiate(statPrefab, scrollRect.content);
            instance.GetComponent<Text>().text = result;
            bigResult += result;
            result = "";

            result += "<b><size=" + titleSizes + ">Shots made</size></b>\n<size=" + normalFontSize + ">";

            result += " - " + stats.shotsMade.ToString() + "\n";
            result += "</size>";
            instance = Instantiate(statPrefab, scrollRect.content);
            instance.GetComponent<Text>().text = result;
            bigResult += result;
            result = "";

            result += "<b><size=" + titleSizes + ">Skills used</size></b>\n<size=" + normalFontSize + ">";

            result += " - " + stats.skillsUsed.ToString() + "\n";
            result += "</size>";
            instance = Instantiate(statPrefab, scrollRect.content);
            instance.GetComponent<Text>().text = result;
            bigResult += result;
            result = "";

            result += "<b><size=" + titleSizes + ">Games played</size></b>\n<size=" + normalFontSize + ">";
            for (int i = 0; i < stats.timesCharacterWasUsed.Length; i++)
            {
                result += " - " + MainMenu.main.characters[i].name+": "+ stats.timesCharacterWasUsed[i].ToString() + "\n";
            }
            result += "</size>";
            instance = Instantiate(statPrefab, scrollRect.content);
            instance.GetComponent<Text>().text = result;
            bigResult += result;
            result = "";


            bigResult += "\n";
            statPrefab.SetActive(false);
        }
        else
        {
            statPrefab.GetComponent<Text>().text = "<b><size=" + titleSizes + ">You have not played any complete games yet, go play some games</size></b>\n";
        }
    }
}
