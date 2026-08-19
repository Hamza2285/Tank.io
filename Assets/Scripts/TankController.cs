using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player tank movement only.
/// Cannon aiming and shooting live in TankTargeting / TankShooter.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TankController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in units per second.")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("How fast the tank body turns toward the movement direction (degrees per second).")]
    [SerializeField] private float turnSpeed = 540f;

    [Header("Body")]
    [Tooltip("Transform that gets rotated. Leave empty to rotate the Rigidbody2D itself.")]
    [SerializeField] private Transform bodyTransform;

    [Tooltip("Angle offset for the body sprite. Use -90 if the sprite points UP, 0 if it points RIGHT.")]
    [SerializeField] private float spriteAngleOffset = -90f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    /// <summary>Current normalized input direction (zero when idle).</summary>
    public Vector2 MoveInput => moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true; // body rotation is driven manually, not by physics
    }

    private void Update()
    {
        moveInput = ReadInput();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;

        if (moveInput.sqrMagnitude > 0.01f)
            RotateBodyToward(moveInput);
    }

    private Vector2 ReadInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return Vector2.zero;

        Vector2 input = Vector2.zero;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;

        return input.normalized;
    }

    private void RotateBodyToward(Vector2 direction)
    {
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + spriteAngleOffset;

        if (bodyTransform != null)
        {
            float current = bodyTransform.eulerAngles.z;
            float next = Mathf.MoveTowardsAngle(current, targetAngle, turnSpeed * Time.fixedDeltaTime);
            bodyTransform.rotation = Quaternion.Euler(0f, 0f, next);
        }
        else
        {
            float next = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, turnSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(next);
        }
    }
}
