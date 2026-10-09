using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector2 moveInput;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        ReadMovementInput();
    }
    private void FixedUpdate()
    {
        Move();
    }
    private void ReadMovementInput()
    {
        if (Keyboard.current == null)
        {
            moveInput = Vector2.zero;
            return;
        }
        float horizontal = 0f;
        float vertical = 0f;
        if (Keyboard.current.aKey.isPressed)
            horizontal -= 1f;
        if (Keyboard.current.dKey.isPressed)
            horizontal += 1f;
        if (Keyboard.current.sKey.isPressed)
            vertical -= 1f;
        if (Keyboard.current.wKey.isPressed)
            vertical += 1f;

        moveInput = new Vector2(horizontal, vertical).normalized;
    }

    private void Move()
    {
        Vector3 movement = new Vector3(
            moveInput.x,
            0f,
            moveInput.y
            ) * moveSpeed;
        rb.linearVelocity = new Vector3(
            movement.x,
            rb.linearVelocity.y,
            movement.z
            );
    }
}
