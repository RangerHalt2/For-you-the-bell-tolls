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
    public Transform player;
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
        if (isSummoning)
        {
            PullWall();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= triggerRange && summonedWall == null && !isOnCooldown)
        {
            SummonWall();
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

    public void Flip()
    {
        direction *= -1;
        if (GetComponentInChildren<SpriteRenderer>() is SpriteRenderer sr)
        {
            sr.flipX = (direction == 1);
        }
    }
}