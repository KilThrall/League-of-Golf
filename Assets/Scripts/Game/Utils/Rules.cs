using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Rules
{
    public bool collisions = true, anyHoleWorks = true, shootOutside = true, isPublic=true;
    public int maxStrokes, numberOfBots, totalHoles, resetPrice, levelSelected;
    public float bounciness, friction, maxPower, jumpPower, gravity, timeInSeconds, respawnTime, resetTime;

    public Rules()
    {
        maxStrokes = 20;
        numberOfBots = 0;
        bounciness = 0.7f;
        friction = 1;
        maxPower = 1;
        jumpPower = 1;
        gravity = 1;
        timeInSeconds = 90;
        anyHoleWorks = true;
        shootOutside = true;
        isPublic = true;
        respawnTime = 2;
        resetTime = 4;
        totalHoles = 18;
        resetPrice = 0;
        levelSelected = 0;
    }
}
