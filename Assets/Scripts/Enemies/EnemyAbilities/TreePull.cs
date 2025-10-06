using UnityEngine;
using System.Collections;

public class TreePull : MonoBehaviour
{
    [Header("Wall Settings")]
    public GameObject wallPrefab;
    public float summonDistance = 10f;
    public float pullSpeed = 5f;
    public float wallLifetime = 1f;

    [Header("Trigger Settings")]
    public LayerMask playerLayer;
    public float triggerRange = 6f;

    [Header("Facing Direction")]
    public int direction = -1;

    [Header("Cooldown Settings")]
    public float cooldownDuration = 3f;
    private bool isOnCooldown = false;

    private Rigidbody2D rb;
    private GameObject summonedWall;
    private bool isSummoning = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, triggerRange, playerLayer);

        if (playerCollider != null)
        {
            Vector3 playerPos = playerCollider.transform.position;
            float dirToPlayer = playerPos.x - transform.position.x;

            if (Mathf.Abs(dirToPlayer) > 0.1f)
            {
                direction = dirToPlayer > 0 ? 1 : -1;

                if (GetComponentInChildren<SpriteRenderer>() is SpriteRenderer sr)
                {
                    sr.flipX = (direction == 1);
                }
            }

            if (summonedWall == null && !isOnCooldown)
            {
                SummonWall();
            }
        }

        if (isSummoning)
        {
            PullWall();
        }
    }

    void SummonWall()
    {
        isSummoning = true;
        isOnCooldown = true;

        Vector3 summonPosition = transform.position + Vector3.right * direction * summonDistance;
        summonedWall = Instantiate(wallPrefab, summonPosition, Quaternion.identity);

        StartCoroutine(CooldownCoroutine());
        StartCoroutine(DestroyWallAfterTime(wallLifetime));
    }

    IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(cooldownDuration);
        isOnCooldown = false;
    }

    IEnumerator DestroyWallAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        if (summonedWall != null)
        {
            Destroy(summonedWall);
            summonedWall = null;
            isSummoning = false;
        }
    }

    void PullWall()
    {
        if (summonedWall == null)
        {
            isSummoning = false;
            return;
        }

        summonedWall.transform.position = Vector3.MoveTowards(summonedWall.transform.position, transform.position, pullSpeed * Time.deltaTime);
    }
}