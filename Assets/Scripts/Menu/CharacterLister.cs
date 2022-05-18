using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterLister : MonoBehaviour
{
    
    public float nameSize, normalSize;

    private ScrollRect scrollRect;
    private GameObject charaPrefab;
    private Character[] characters;

    private void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        charaPrefab = scrollRect.content.GetChild(0).gameObject;


        SetCharacters();
    }

    public void UpdateList()
    {
        for (int i = 1; i < scrollRect.content.childCount; i++)
        {
            if (PlayerPrefs.GetInt("characterBought" + (i-1).ToString()) == 1)
            {
                Transform instance = scrollRect.content.GetChild(i);
                instance.GetChild(4).gameObject.SetActive(false);
                instance.GetChild(5).gameObject.SetActive(false);
            }
        }
       // charaPrefab.SetActive(true);
    }

    private void SetCharacters()
    {
        characters = MainMenu.main.characters;

        for (int i = 0; i < characters.Length; i++)
        {
            SkillSet chara = characters[i].prefab.GetComponent<SkillSet>();
            Transform instance = Instantiate(charaPrefab, scrollRect.content).transform;
            instance.GetChild(1).GetComponent<Image>().sprite = chara.characterSprite;
            string description = "<b><size=" + nameSize.ToString() + ">" + chara.characterName + "</size></b>\n\n<size=" + normalSize.ToString() + "><b>Pasive:</b> " + chara.passiveDescription + "\n\n<b>Active:</b> " + chara.activeDescription + "</size>";
            instance.GetChild(2).GetComponent<Text>().text = description;
            instance.GetChild(3).GetComponent<Image>().sprite = chara.skillSprite;
            if (PlayerPrefs.GetInt("characterBought" + i.ToString()) == 0)
            {
                //child n4 is quarters button. TODO: Need better arrangement for this
              //  instance.GetChild(4).gameObject.SetActive(true);
                instance.GetChild(5).gameObject.SetActive(true);
              //  instance.GetChild(4).GetChild(0).GetComponent<Text>().text = characters[i].quarterPrice.ToString();
                instance.GetChild(5).GetChild(0).GetComponent<Text>().text = "Buy for: "+characters[i].ballPrice.ToString();
                int copy = i;
                instance.GetChild(4).GetComponent<Button>().onClick.AddListener(() => MainMenu.main.BuyCharacterWithQuarters(copy));
                instance.GetChild(5).GetComponent<Button>().onClick.AddListener(() => MainMenu.main.BuyCharacterWithBalls(copy));
            }
        }
        charaPrefab.SetActive(false);
    }
}
