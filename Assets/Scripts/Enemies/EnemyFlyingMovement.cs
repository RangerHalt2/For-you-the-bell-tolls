using UnityEngine;

public class EnemyFlyingMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    private bool isChasing = false;

    [Header("Patrol Points")]
    public Transform patrolPointA;
    public Transform patrolPointB;
    private Transform currentPatrolTarget;

    [Header("Player Detection")]
    public Transform player;
    public float chaseStartRange = 5f;
    public float chaseStopRange = 7f;
    public LayerMask playerLayer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        currentPatrolTarget = patrolPointA;
    }

    void FixedUpdate()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isChasing && distanceToPlayer <= chaseStartRange)
        {
            isChasing = true;
        }
        else if (isChasing && distanceToPlayer >= chaseStopRange)
        {
            isChasing = false;
            currentPatrolTarget = GetClosestPatrolPoint();
        }

        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (currentPatrolTarget == null) return;

        Vector2 direction = (currentPatrolTarget.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;

        if (spriteRenderer != null)
            spriteRenderer.flipX = direction.x > 0;

        if (Vector2.Distance(transform.position, currentPatrolTarget.position) < 0.1f)
        {
            currentPatrolTarget = currentPatrolTarget == patrolPointA ? patrolPointB : patrolPointA;
        }
    }

    void ChasePlayer()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;

        if (spriteRenderer != null)
            spriteRenderer.flipX = direction.x > 0;
    }

    Transform GetClosestPatrolPoint()
    {
        float distA = Vector2.Distance(transform.position, patrolPointA.position);
        float distB = Vector2.Distance(transform.position, patrolPointB.position);
        return distA < distB ? patrolPointA : patrolPointB;
    }
}