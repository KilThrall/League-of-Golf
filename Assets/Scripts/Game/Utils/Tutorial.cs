using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    public static Tutorial main;

    public Text textToReplace;
    public Animator anim;
    public TutorialPhase[] parts;

    private int currentPart=0, shotsSinceLast = 0, holesSinceLast = 0, leftSkillsLast = 0, rightSkillsLast = 0;

    private void Start()
    {
        main = this;
        textToReplace.text = parts[currentPart].message;
        anim.SetTrigger("partCompleted");
        StartCoroutine(SetRules());
    }

    public void PlayerShot()
    {
        if (currentPart >= parts.Length)
            return;
        shotsSinceLast++;
        CheckCompletion();
    }

    public void SkillUsed(bool left)
    {
        if (currentPart >= parts.Length)
            return;
        if (left)
            leftSkillsLast++;
        else
            rightSkillsLast++;
        CheckCompletion();
    }

    public void HoleCompleted()
    {
        if (currentPart >= parts.Length)
            return;
        holesSinceLast++;
        CheckCompletion();
    }

    private void CheckCompletion()
    {
        if (currentPart >= parts.Length)
            return;
        bool completed = !parts[currentPart].canBeClicked;

        if (shotsSinceLast < parts[currentPart].shotsMade)
        {
            completed = false;
        }
        if (holesSinceLast < parts[currentPart].holesCompleted)
        {
            completed = false;
        }
        if (leftSkillsLast < parts[currentPart].rightSkillsThrown)
        {
            completed = false;
        }
        if (rightSkillsLast < parts[currentPart].rightSkillsThrown)
        {
            completed = false;
        }

        if (completed)
        {
            PartComplete();
        }
    }

    private IEnumerator SetRules()
    {
        yield return new WaitForSeconds(1f);

        GameControl.main.rules = new Rules();
        GameControl.main.rules.maxStrokes = 9999;
        GameControl.main.rules.timeInSeconds = 9999;
        GameControl.main.controller.SetAwake();
        GameControl.main.playerInstance.GetComponent<SkillSet>().cooldown /= 2;
        GameControl.main.playerInstance.GetComponent<SkillSet>().cooldown /= 2;
        
    }

    private void PartComplete()
    {
        currentPart++;
        if (currentPart >= parts.Length)
            return;
        else
        {
            textToReplace.text = parts[currentPart].message;
            shotsSinceLast = 0;
            holesSinceLast = 0;
            leftSkillsLast = 0;
            rightSkillsLast = 0;
            anim.SetTrigger("partCompleted");
            GameControl.main.isDefending = parts[currentPart].defending;
            GameControl.main.playerInstance.GetComponent<BallManager>().isDefending = GameControl.main.isDefending;
            GameControl.main.controller.canMove = !parts[currentPart].canBeClicked;
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (currentPart >= parts.Length)
                return;
            if (parts[currentPart].canBeClicked)
            {
                PartComplete();
            }
        }
    }
}

//\nPress <color=#FFBD00>Left click</color> to continue
//<color=#FFBD00>Left click</color>

[System.Serializable]
public class TutorialPhase
{
    public int shotsMade, holesCompleted, leftSkillsThrown,rightSkillsThrown;
    [TextArea]
    public string message;
    public bool canBeClicked, defending;
}
