using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PatchNotes : MonoBehaviour
{
    public Patch[] patches;

    public GameObject morePatchesPrefab;
    public string versionSizes,titleSizes, normalFontSize, bigResult;
    public bool html;

    private ScrollRect scrollRect;
    private GameObject patchPrefab, buttonInstance;

    private void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        patchPrefab = scrollRect.content.GetChild(0).gameObject;
        
        bigResult = "";
        for (int i = patches.Length-1; i >= patches.Length-5; i--)
        {
            ShowPatch(i);
        }

        patchPrefab.SetActive(false);
        buttonInstance = Instantiate(morePatchesPrefab, scrollRect.content);
        buttonInstance.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(ShowAllPatches);
    }

    public void ShowAllPatches()
    {
        patchPrefab.SetActive(true);
        for (int i = patches.Length - 6; i >= 0; i--)
        {
            ShowPatch(i);
        }
        patchPrefab.SetActive(false);
        buttonInstance.SetActive(false);
    }

    private void ShowPatch(int i)
    {
        string result = "";
        if (!html)
        {
            result = "<size=" + normalFontSize + ">" + patches[i].releaseDate + "</size>\n" + "<b><size=" + versionSizes + ">V" + patches[i].version + "</size></b>\n\n" + "<size=" + normalFontSize + ">" + patches[i].introductionText + "</size>\n\n";


            result += "<b><size=" + titleSizes + ">Additions </size></b>\n<size=" + normalFontSize + ">";
            foreach (string a in patches[i].additions)
            {
                result += " - " + a + "\n";
            }
            result += "</size>\n<b><size=" + titleSizes + ">Changes</size></b>\n<size=" + normalFontSize + ">";

            foreach (string a in patches[i].changes)
            {
                result += " - " + a + "\n";
            }

            result += "</size>\n<b><size=" + titleSizes + ">Bug fixes</size></b>\n<size=" + normalFontSize + ">";

            foreach (string a in patches[i].bugFixes)
            {
                result += " - " + a + "\n";
            }

            result += "</size>";
        }
        else
        {
            //HTML Version
            result = "<p>" + patches[i].releaseDate + "</p>\n" + "<b><h1>V" + patches[i].version + "</h1></b>\n\n" + "<p>" + patches[i].introductionText + "</p>\n\n";


            result += "<b><h3>Additions </h3></b>\n";
            foreach (string a in patches[i].additions)
            {
                result += "<p> - " + a + "\n</p>";
            }
            result += "\n<b><h3>Changes</h3></b>\n";

            foreach (string a in patches[i].changes)
            {
                result += "<p> - " + a + "\n</p>";
            }

            result += "\n<b><h3>Bug fixes</h3></b>\n";

            foreach (string a in patches[i].bugFixes)
            {
                result += "<p> - " + a + "\n</p>";
            }
            result += "<p>___________________________________<p>";
        }

        GameObject instance = Instantiate(patchPrefab, scrollRect.content);
        instance.GetComponent<Text>().text = result;
        bigResult += result += "\n";
    }
}

[System.Serializable]
public class Patch
{
    public string version, releaseDate;
    
    [TextArea(15,20)]
    public string  introductionText;
    [TextArea]
    public string[] bugFixes, additions, changes;
}
