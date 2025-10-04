using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayerController : MonoBehaviour, IController
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isJumpPressed = false;
    private bool isGrounded;

    private IA_Main controls;

    void Awake()
    {
        controls = new IA_Main();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => isJumpPressed = true;
    }

    void OnEnable()
    {
        if (controls == null)
        {
            controls = new IA_Main();

            controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

            controls.Player.Jump.performed += ctx => isJumpPressed = true;
        }

        controls.Player.Enable();
    }

    void OnDisable()
    {
        controls.Player.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Freeze Z position to keep movement 2D
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
    }

    void Update()
    {
        // Check if grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        // Jump
        if (isJumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, 0f);
        }

        isJumpPressed = false;
    }

    void FixedUpdate()
    {
        // Horizontal movement
        Vector3 velocity = rb.linearVelocity;
        velocity.x = moveInput.x * moveSpeed;
        rb.linearVelocity = velocity;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    public void EnableControl()
    {
        this.enabled = true;
    }

    public void DisableControl()
    {
        this.enabled = false;
    }
}