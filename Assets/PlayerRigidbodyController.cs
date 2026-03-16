using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerRigidbodyController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float jumpForce = 5f;
    public Transform orientation;

    Rigidbody rb;
    Vector3 inputVector;
    bool jumpRequest;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Read input in Update (frame rate dependent) but apply in FixedUpdate
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        inputVector = new Vector3(h, 0f, v);

        if (Input.GetKeyDown(KeyCode.Space))
            jumpRequest = true;

        // Optional: rotate to face movement direction
        if (inputVector.sqrMagnitude > 0.01f)
        {
            Vector3 lookDir = (orientation != null) ? 
                (orientation.transform.TransformDirection(inputVector)) : inputVector;
            lookDir.y = 0f;
            transform.forward = Vector3.Slerp(transform.forward, lookDir.normalized, 0.15f);
        }
    }

    void FixedUpdate()
    {
        // Apply horizontal movement using MovePosition for kinematic-like movement
        Vector3 worldMove = (orientation != null) ? orientation.TransformDirection(inputVector) : inputVector;
        Vector3 velocity = worldMove.normalized * moveSpeed;
        Vector3 newPos = rb.position + velocity * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        // Jump (simple)
        if (jumpRequest && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequest = false;
        }
    }

    bool IsGrounded()
    {
        // Raycast down to check ground
        float distance = 0.6f;
        return Physics.Raycast(transform.position, Vector3.down, distance);
    }
}
