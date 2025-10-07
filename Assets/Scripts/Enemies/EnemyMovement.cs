using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    public int direction = 1;
    public bool isChasing = false;

    [Header("Raycast Settings")]
    public float groundCheckDistance = 1f;
    public float wallCheckDistance = 1f;
    public Vector2 raycastOffset = new Vector2(0.5f, 0f);
    public LayerMask groundLayer;

    [Header("Player Detection")]
    public Transform player;
    public float sightRange = 5f;
    public float attackRange = 1.5f;
    public LayerMask playerLayer;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2f;
    private float attackTimer = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private Health currHealth;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currHealth = GetComponent<Health>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        FindPlayer();
        if (PlayerInSight())
        {
            isChasing = true;
            ChasePlayer();
            AttackPlayer();
        }
        else
        {
            isChasing = false;
            Patrol();
        }
        HandleTimer();
    }


    void Patrol()
    {
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

        Vector2 origin = (Vector2)transform.position + new Vector2(raycastOffset.x * direction, raycastOffset.y);

        bool isGroundAhead = Physics2D.Raycast(origin + Vector2.up * 0.1f, Vector2.down, groundCheckDistance, groundLayer);
        bool isWallAhead = Physics2D.Raycast(origin, Vector2.right * direction, wallCheckDistance, groundLayer);

        Debug.DrawRay(origin + Vector2.up * 0.1f, Vector2.down * groundCheckDistance, Color.red);
        Debug.DrawRay(origin, Vector2.right * direction * wallCheckDistance, Color.blue);

        if (!isGroundAhead || isWallAhead)
        {
            Flip();
        }
    }

    void ChasePlayer()
    {
        if (player == null) return;

        float playerDirection = Mathf.Sign(player.position.x - transform.position.x);
        direction = (int)playerDirection;

        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (direction == 1);
        }
    }

    //LB: This finds the current player object, it will only ever read one player at a time because FindAnyObjectsByType searches only active components.
    private void FindPlayer()
    {
        PlayerController[] playerControllers = GameObject.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController playerController in playerControllers)
        {
            GameObject obj = playerController.gameObject;
            if (obj != null && obj.layer == LayerMask.NameToLayer("Player") && playerController.enabled)
            {
                player = obj.transform;
            }
        }
    }

    //LB: Added a function that if the player is in this attack range deal damage condition must be set by a timer of how often they can take damage.
    void AttackPlayer()
    {
        if (attackTimer > 0) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if(distance <= attackRange)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null && currHealth != null && playerHealth.teamID != currHealth.teamID)
            {
                playerHealth.TakeDamage(1);
                attackTimer = attackCooldown;
            }
        }
    }

    bool PlayerInSight()
    {
        if (player == null) return false;

        float distance = Vector2.Distance(transform.position, player.position);
        //Debug.Log("Checking their Distance");
        if (distance > sightRange)
        {
            //Debug.Log("Distance is too far");
            return false;
        }

        //Debug.Log("Distance is in range!");

        RaycastHit2D hit = Physics2D.Raycast(transform.position, (player.position - transform.position).normalized, distance, groundLayer | playerLayer);


        return hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Player");
    }

    void Flip()
    {
        direction *= -1;

        if (spriteRenderer != null)
        {
            //spriteRenderer.flipX = (direction == 1);
        }
    }

    void HandleTimer()
    {
        attackTimer -= Time.deltaTime;
    }
}