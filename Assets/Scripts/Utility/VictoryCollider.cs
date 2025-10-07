using UnityEngine;

public class VictoryCollider : MonoBehaviour
{

    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private GameObject gameScreen;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //LB: The player collided with it
        if (collision.GetComponent<PlayerController>().enabled)
        {
            victoryScreen.SetActive(true);
            gameScreen.SetActive(false);
        }   
    }
}
