using UnityEngine;

public class Pounce : MonoBehaviour
{
    [Header("Pounce Settings")]
    public float pounceForce = 10f;
    public float upwardForce = 5f;
    public float pounceRange = 3f;
    public float pounceCooldown = 2f;

    private Rigidbody2D rb;
    private Transform player;
    private float lastPounceTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        lastPounceTime = -pounceCooldown;
    }

    void Update()
    {
        if (CanPounce())
        {
            PouncePl();
        }
    }

    bool CanPounce()
    {
        if (player == null) return false;

        float timeSinceLastPounce = Time.time - lastPounceTime;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        return timeSinceLastPounce >= pounceCooldown && distanceToPlayer <= pounceRange;
    }

    void PouncePl()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        Vector2 pounceVelocity = new Vector2(direction.x * pounceForce, upwardForce);

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(pounceVelocity, ForceMode2D.Impulse);

        lastPounceTime = Time.time;
    }
}