using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingObject : MonoBehaviour
{
    public Vector3 rotatingDir;
    public Transform target;

    [SerializeField]
    private bool moving = false;
    private Rigidbody rb;

    private void FixedUpdate()
    {
        if (moving)
        {
            if (rb == null)
                target.eulerAngles += rotatingDir * Time.deltaTime;
            else
                rb.AddTorque(rotatingDir,ForceMode.VelocityChange);
        }
    }

    public void StartMovement()
    {
        rb = target.GetComponent<Rigidbody>();
        moving = true;
    }
}
