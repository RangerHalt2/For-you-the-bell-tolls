// Created By: Ryan Lupoli
// Automatically destroys a game object after a set delay
using UnityEngine;

public class TimedObjectDestroyer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("The amount of time (in seconds) before the object this script is attatched to is destroyed.")]
    [SerializeField] private float lifeTime = 5;

    // The amount of time which has passed since this script was initialized.
    private float elapsedTime = 0;

    // Update is called once per frame
    void Update()
    {
        // Track time which has passed
        elapsedTime += Time.deltaTime;

        // Once the lifeTime of the object has passed...
        if (elapsedTime >= lifeTime)
        {
            // Destroy the game object
            Debug.Log("Timed Object Destroyer: Destroyed " + gameObject.name + ".");
            Destroy(gameObject);
        }
    }
}
