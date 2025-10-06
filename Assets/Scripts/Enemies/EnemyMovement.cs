using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    public int direction = -1;
    private bool isChasing = false;

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

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (PlayerInSight())
        {
            isChasing = true;
            ChasePlayer();
        }
        else
        {
            isChasing = false;
            Patrol();
        }
    }

    private void GetNewPlayer()
    {

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

    bool PlayerInSight()
    {
        if (player == null) return false;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > sightRange) return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, (player.position - transform.position).normalized, distance, groundLayer | playerLayer);

        return hit.collider != null && hit.collider.gameObject.layer == playerLayer;
    }

    void Flip()
    {
        direction *= -1;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (direction == 1);
        }
    }
}