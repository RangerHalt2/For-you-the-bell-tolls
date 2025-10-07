// Created By: Ryan Lupoli
// This script manages the player's possession mechanic.
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PossessionManager : MonoBehaviour
{
    #region Variables
    [Tooltip("Determines if the object this script is assigned to is the currently possessed by the player.")]
    [SerializeField] private bool isCurrentBody;

    [Header("Ghost Settings")]
    [Tooltip("Determines if the object this script is assigned to is the player's ghost body.")]
    [SerializeField] private bool isPlayerGhost;
    [SerializeField] private GameObject playerGhost;

    [Header("Controller Settings")]
    [SerializeField] private PlayerController controller;
    private IController _controller;

    [Header("Possession Settings")]
    [Tooltip("The radius (in units) checked when trying to possess something.")]
    [SerializeField] private float detectionRadius = 5f;
    [Tooltip("The layer mask checked when looking for objects to possess.")]
    [SerializeField] LayerMask possessionLayerMask;
    [Tooltip("Possession Intiation Cooldown Internally")]
    [SerializeField] private float possessCooldown = 1f;
                     private float possessTimer = 0f;

    private PossessionManager currentTarget;
    private Decay targetDecay;

    [Header("Possession Challenge")]
    [SerializeField] private PossessionChallengeBar possessionChallengeBar;

    private InputManager inputManager;

    #endregion


    public void Awake()
    {
        // Confirm assigned controller is part of the interface
        _controller = controller as IController;
        if (_controller == null)
        {
            Debug.LogError("Assigned Controller does not implement IController");
        }

        // Assign Player Ghost Game Object if it is not already assigned
        if (!isPlayerGhost && playerGhost == null)
        {
            FindGhostInScene();
        }

        if (possessionChallengeBar != null)
        {
            possessionChallengeBar.DisableVisuals();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputManager = GameObject.FindAnyObjectByType<InputManager>();
        // If this is the player's ghost
        if (isPlayerGhost)
        {

        }
        // Else if this a possessible object
        else
        {

        }
    }

    // Update is called once per frame
    void Update()
    {

        if (inputManager.PossessInput && possessTimer <= 0) OnPossessPerformed();
        if (inputManager.ExitInput) OnExitPerformed();

        possessTimer -= Time.deltaTime;

        if (isCurrentBody)
        {
            return;
        }
    }

    private void OnPossessPerformed()
    {
        // If the player is in this body, and they are currently the ghost
        if (isCurrentBody && isPlayerGhost)
        {
            possessTimer = possessCooldown; //gives like a half second delay so it doesn't spam it when they press it
            AttemptPossession();
        }
    }

    private void OnExitPerformed()
    {
        // If the player is in this body and it is not the ghost
        if (isCurrentBody && !isPlayerGhost)
        {
            ExitHost();
        }
    }

    #region Possession Logic
    // Check if there is an object with a Possession Manager script attatched
    void AttemptPossession()
    {
        // Create an array of all objects within the detectionRadius in the possessionLayerMask
        Collider2D [] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, possessionLayerMask);

        float closestDistance = Mathf.Infinity;
        PossessionManager closestPossession = null;

        // Check each collider hit
        foreach (Collider2D hit in hits)
        {
            // If the object has the possession manager
            PossessionManager pm = hit.GetComponent<PossessionManager>();
            // If pm is found
            if (pm != null && pm.isCurrentBody == false && pm.isPlayerGhost == false)
            {
                // Check the distance of the hit object from the player
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                // If the object is closer than he currently recorded one, record this object as the new closest
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPossession = pm;
                }
            }
        }

        // If an object was found
        if (closestPossession != null)
        {
            // Set the closest object as the new possesion target
            currentTarget = closestPossession;
            targetDecay = closestPossession.gameObject.GetComponent<Decay>();
            Debug.Log("Ready to possess: " + currentTarget.gameObject.name);

            //LB: If the player has possessed this target already, skip the mini-game
            if (targetDecay != null && targetDecay.GetPossessed())
            {
                Possess();
                return;
            }

            currentTarget.possessionChallengeBar.StartChallenge();

            StartCoroutine(WaitForPossessionChallenge(currentTarget));
        }
        else
        {
            Debug.Log("No possessable objects nearby.");
        }
    }

    private IEnumerator WaitForPossessionChallenge(PossessionManager target)
    {

        EnemyMovement em = currentTarget.GetComponent<EnemyMovement>();
        EnemyFlyingMovement emFlying = currentTarget.GetComponent<EnemyFlyingMovement>();
        Pounce pounce = currentTarget.GetComponent<Pounce>();

        if (em != null)
        {
            em.enabled = false;
        }
        if (emFlying != null)
        {
            emFlying.enabled = false;
        }
        if (pounce != null)
        {
            pounce.enabled = false;
        }

        // Disable Player controls while waiting for challenge to finish
        _controller.DisableControl();

        while (target.possessionChallengeBar.isChallengeActive())
        {
            yield return null; // Yield to prevent freezing
        }

        target.possessionChallengeBar.DisableVisuals();

        int result = target.possessionChallengeBar.getChallengeResult();
        if (result == 0)
        {
            Debug.Log("Possession Challenge passed.");
            Possess();
        }
        else
        {

            if (em != null)
            {
                em.enabled = true;
            }
            if (emFlying != null)
            {
                emFlying.enabled = true;
            }
            if(pounce != null)
            {
                pounce.enabled = true;
            }
            _controller.EnableControl();
            Debug.Log("Possession Challenge Failed.");
            currentTarget = null;
        }
    }

    // Allows for one object to possess another
    void Possess()
    {
        // If there is no current target, or the current target is this object, do nothing
        if (currentTarget == null || currentTarget == this)
        {
            return;
        }
        
        //LB: Player Ghost is coming up null the first possession here
        if(playerGhost == null)
        {
            FindGhostInScene();
        }

        //LB: Get the Death Manager and disable the fading of the ghost
        DeathManager deathManager = playerGhost.GetComponent<DeathManager>();
        deathManager.SetIsFading(false);

        // Disable Current Body
        //_controller.DisableControl(); //Already done in the minigame??
        isCurrentBody = false;

        // Enable New Body
        currentTarget.isCurrentBody = true;
        currentTarget.gameObject.layer = LayerMask.NameToLayer("Player");
        currentTarget._controller.EnableControl();

        CameraTracker tracker = GameObject.FindAnyObjectByType<CameraTracker>();
        tracker.SetPlayer(currentTarget.transform);

        //LB: Start the decay
        targetDecay.SetDecaying(true);
        targetDecay.SetPossessed(true);
        targetDecay.SetIsDestroying(false);
        targetDecay.ResetDestroyTimer();

        Health targetHealth = currentTarget.GetComponent<Health>();
        targetHealth.teamID = 0;

        Debug.Log("Possession Manager: Moved from " + gameObject.name + " to " + currentTarget.name + ".");

        if (isPlayerGhost)
        {
            gameObject.SetActive(false);
        }
    }

    // Allows for the ghost to exit a host body
    public void ExitHost()
    {
        // Ensure player's ghost is not performing this check
        if (isPlayerGhost)
        {
            Debug.LogWarning("Ghost cannot exit itself");
            return;
        }

        // Ensure a player ghost is assigned
        if (playerGhost == null)
        {
            Debug.LogError("Player ghost reference not set. Cannot exit host.");
            return;
        }

        //LB: Find the Decay script of the thing the player is leaving
        targetDecay = GetComponent<Decay>();

        // Move the ghost to the current body's position
        playerGhost.transform.position = transform.position;

        //LB: Handle Exit Decay Elements
        if(targetDecay != null)
        {
            //Debug.Log("Target Decay Not Null");
            targetDecay.SetDecaying(false);
            targetDecay.SetIsDestroying(true);
            targetDecay.EmptyDecayBar();
            targetDecay.gameObject.layer = LayerMask.NameToLayer("Attackable");
        }
        else
        {
            Debug.Log("Target Decay is Null");
        }


            // Reactivate the player ghost object
            playerGhost.SetActive(true);

        // Find Ghost's Possession Manager
        PossessionManager ghostPM = playerGhost.GetComponent<PossessionManager>();

        // Disable current body's controls
        _controller.DisableControl();

        // If Ghost's Possession Manager was found
        if (ghostPM != null)
        {
            ghostPM.isCurrentBody = true;
            ghostPM._controller.EnableControl();
            Debug.Log("Returned control to ghost");
        }

        
        isCurrentBody = false;

        //LB: Get their death manager and then enable the death manager fading to be true
        DeathManager deathManager = playerGhost.GetComponent<DeathManager>();
        deathManager.SetIsFading(true);
        deathManager.ResetFadeTimer();

        CameraTracker tracker = GameObject.FindAnyObjectByType<CameraTracker>();
        tracker.SetPlayer(playerGhost.transform);

        Debug.Log(gameObject.name + " exited. Control returned to ghost");
    }

    #endregion

    #region Utility
    // Searches the current scene for the player ghost game object and assigns it to the appropriate field
    private void FindGhostInScene()
    {
        // Checking for any object with PlayerGhost tag
        GameObject foundGhost = GameObject.FindGameObjectWithTag("PlayerGhost");

        // If the Player Ghost was found
        if (foundGhost != null)
        {
            playerGhost = foundGhost;
            Debug.Log("Player ghost found in scene and assigned.");
        }
        else
        {
            Debug.LogWarning("No object with tag 'PlayerGhost' found in scene. Possession mechanic may not work as intended");
        }
    }
    #endregion
}
