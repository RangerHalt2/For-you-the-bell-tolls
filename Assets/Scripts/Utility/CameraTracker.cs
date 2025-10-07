using Newtonsoft.Json.Bson;
using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    [SerializeField] private Transform player;




    public void SetPlayer(Transform player)
    {
        this.player = player;
    }

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (player == null) return;
        Vector3 position = transform.position;
        position.x = player.position.x;
        position.y = player.position.y;

        transform.position = position;
    }

    void FindPlayer()
    {
        Transform player;
        PlayerController [] playerControllers = GameObject.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController playerController in playerControllers)
        {
            GameObject obj = playerController.gameObject;
            if (obj != null && obj.layer == LayerMask.NameToLayer("Player"))
            {
                player = obj.transform;
                SetPlayer(player);
                return;
            }
        }
        
    }

}
