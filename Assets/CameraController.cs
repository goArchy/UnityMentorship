using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform target;
    public float followSpeed = 10f;
    public float rotationSpeed = 5f;
    public Vector3 offset = new Vector3(0f, 6f, -8f);

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
            transform.position = target.position + offset;
    }
}
