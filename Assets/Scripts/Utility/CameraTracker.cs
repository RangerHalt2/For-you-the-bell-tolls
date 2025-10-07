using Newtonsoft.Json.Bson;
using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    [SerializeField] private Transform player;




    public void SetPlayer(Transform player)
    {
        this.player = player;
    }

    private void Awake()
    {
        FindPlayer();
    }

    private void Update()
    {
        Vector3 position = transform.position;
        position.x = player.position.x;
        position.y = player.position.y;

        transform.position = position;
    }

    void FindPlayer()
    {
        Transform player = GameObject.FindAnyObjectByType<PlayerController>().gameObject.transform;
        SetPlayer(player);
    }

}
