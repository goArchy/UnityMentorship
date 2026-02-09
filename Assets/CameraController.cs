using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float followSpeed = 10f;
    public Vector3 offset = new Vector3(0f, 14f, -3f);

    void Update()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target);
        }
    }
}
