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

        //GameObject playerObj = GameObject.FindAnyObjectByType<PlayerController>();
        //if (playerObj != null)
        //{
        //    player = playerObj.transform;
        //}

        lastPounceTime = -pounceCooldown;
    }

    void Update()
    {
        FindPlayer();
        if (player == this.transform) return;

        if (CanPounce())
        {
            PouncePl();
        }
    }

    //LB: This finds the current player object, it will only ever read one player at a time because FindAnyObjectsByType searches only active components.
    private void FindPlayer()
    {
        GameObject go = GameObject.FindAnyObjectByType<PlayerController>().gameObject;
        if (go == null)
        {
            Debug.Log("It did not find an active player!");
            return;
        }
        player = go.transform;
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