using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantRotation : MonoBehaviour
{
    public Vector3 desiredRotation;

    private void Update()
    {

        transform.eulerAngles = desiredRotation;
    }
}
