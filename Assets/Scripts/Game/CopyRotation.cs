using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyRotation : MonoBehaviour
{
    public Transform rotTarget, posTarget;
    public Vector3 offsetRotation, offsetPosition, desiredRotation;

    Vector3 rot;

    private void Update()
    {
        
        float x = desiredRotation.x;
        if (desiredRotation.x<-0.1f &&desiredRotation.x>-2)
            x = rotTarget.eulerAngles.x;
        float y = desiredRotation.y;
        if (desiredRotation.y < -0.1f && desiredRotation.y > -2)
            y = rotTarget.eulerAngles.y;
        float z = desiredRotation.z;
        if (desiredRotation.z < -0.1f && desiredRotation.z > -2)
            z = rotTarget.eulerAngles.z;
        rot = new Vector3(x, y, z);

        transform.eulerAngles = rot+offsetRotation;
        transform.position = posTarget.position + offsetPosition;
    }
}
