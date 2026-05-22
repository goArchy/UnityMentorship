using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerRigidbodyController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float jumpForce = 5f;

    const float GroundCheckDistance = 0.6f;
    const float FallCheckDistance = 1f;

    Rigidbody rb;
    Vector3 inputVector;
    bool jumpRequest;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsHoleFallSequence())
        {
            inputVector = Vector3.zero;
            return;
        }
        
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        inputVector = new Vector3(h, 0f, v);

        if (Input.GetKeyDown(KeyCode.Space))
            jumpRequest = true;

        if (inputVector.sqrMagnitude > 0.01f)
        {
            transform.forward = Vector3.Slerp(transform.forward, inputVector.normalized, 0.15f);
        }
    }

    void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsHoleFallSequence())
        {
            rb.useGravity = true;
            return;
        }
        
        bool grounded = IsGrounded(GroundCheckDistance);
        rb.useGravity = !grounded && !HasFloorBelow(FallCheckDistance);

        Vector3 velocity = inputVector.normalized * moveSpeed;
        Vector3 newPos = rb.position + velocity * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        if (jumpRequest && grounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequest = false;
        }
    }

    bool IsGrounded(float distance)
    {
        return Physics.Raycast(transform.position, Vector3.down, distance);
    }

    bool HasFloorBelow(float distance)
    {
        return Physics.Raycast(transform.position, Vector3.down, distance);
    }
}
