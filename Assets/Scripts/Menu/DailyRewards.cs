using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Newtonsoft.Json;

public class DailyRewards : MonoBehaviour
{
    public GameObject dailyRewardScreen;
    public Transform menuesList;

    private bool opened = false;
    private int coinsToAward = 100;

    // Start is called before the first frame update
    void Start()
    {
        if (opened)
            return;
        opened = true;
        dailyRewardScreen.SetActive(true);
        for (int i = 0; i < menuesList.childCount-1; i++)
        {
            menuesList.GetChild(i).gameObject.SetActive(false);
        }

        menuesList.GetChild(PlayerPrefs.GetInt("gamePlayed", 0)).gameObject.SetActive(true);


        if (PlayerPrefs.GetInt("gamePlayed", 0) == 1)
        {
            if (PlayerPrefs.HasKey("lastDayPlayed"))
            {
                string dateData = PlayerPrefs.GetString("lastDayPlayed");
                DateTime lastDate = JsonConvert.DeserializeObject<DateTime>(dateData);
                var dateDif = DateTime.Today - lastDate;
                //Debug.Log("Today: " + DateTime.Today + "\n Last date: " + lastDate + "\n Datedif: " + dateDif);
                if (dateDif.Days > 1)
                {
                    PlayerPrefs.SetInt("daysInARow", 1);
                }
                else if (dateDif.Days == 1)
                {
                    PlayerPrefs.SetInt("daysInARow", PlayerPrefs.GetInt("daysInARow", 0) + 1);
                }
            }
            else
            {
                PlayerPrefs.SetInt("daysInARow", 1);
            }

            PlayerPrefs.SetString("lastDayPlayed", JsonConvert.SerializeObject(DateTime.Today));
            
            if (PlayerPrefs.GetInt("gamePlayed") == 1)
            {
                if (!PlayerPrefs.HasKey("daysInARow"))
                {
                    PlayerPrefs.SetInt("daysInARow", 1);
                }
                coinsToAward *= PlayerPrefs.GetInt("daysInARow");
                if (coinsToAward > 700)
                {
                    coinsToAward = 700;
                }

                PlayerPrefs.SetInt("daysInARow", PlayerPrefs.GetInt("daysInARow") + 1);
                menuesList.GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = "Collect " + coinsToAward;

            }
        }
    }

    public void CollectCoins()
    {
        MainMenu.main.GetBalls(coinsToAward);

        PlayerPrefs.SetInt("gamePlayed", 2);
        for (int i = 0; i < menuesList.childCount-1; i++)
        {
            menuesList.GetChild(i).gameObject.SetActive(false);
        }
        menuesList.GetChild(PlayerPrefs.GetInt("gamePlayed")).gameObject.SetActive(true);
        PlayerPrefs.SetString("lastDayPlayed", JsonConvert.SerializeObject(DateTime.Today));
    }
}
