using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.PlayerLoop;
using Unity.VisualScripting;

public class PossessionChallengeBar : MonoBehaviour
{
    #region Variables
    [Header("Difficulty")]
    [Tooltip("The amount of time (in Seconds) it takes for the Hit Marker to make it from one end of the bar to the other.")]
    [SerializeField] float speed = 50f;
    [Tooltip("The size of the sweet spot the player must hit.")]
    [SerializeField] float sweetSpotSize = 60f;
    [Tooltip("The number of times the player needs to win the minigame in order to \"Win\" the minigame.")]
    [SerializeField] int requiredWins = 3;
    [Tooltip("The number of times the player is allowed to miss the hitmarker before the minigame ends in failure.")]
    [SerializeField] int allowedLosses = 3;
    private int currentWins;
    private int currentLosses;

    [Header("Image References")]
    [Tooltip("Reference to the main bar for the possession challenge")]
    [SerializeField] private Image backgroundBarImage;
    [Tooltip("Reference to the \"Sweet Spot\" of the bar")]
    [SerializeField] private Image sweetSpot;
    [Tooltip("Reference to the \"Hit Marker\" of the bar. Shows where the player will hit when they ineract with the challenge.")]
    [SerializeField] private Image hitMarker;
    [Tooltip("Reference to the border of the bar.")]
    [SerializeField] private Image border;

    [Header("Effect references")]
    [Tooltip("Effect which should play on successful hit.")]
    [SerializeField] private GameObject minigameHitEffect;
    [Tooltip("Effect which should play on a miss.")]
    [SerializeField] private GameObject minigameMissEffect;
    [Tooltip("Effect which should play when winning the minigame.")]
    [SerializeField] private GameObject minigameWinEffect;
    [Tooltip("Effect which should play when loosing the minigame.")]
    [SerializeField] private GameObject minigameLossEffect;

    private InputManager inputManager;

    private Coroutine challengeCoroutine;
    private bool possessionChallengeActive = false;
    private int challengeResult = -1;
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputManager = GameObject.FindAnyObjectByType<InputManager>();
        // Reset Hit Marker to starting position
        ResetHitMarker();
        // Update the width of the sweet spot
        ChangeSweetSpotWidth(sweetSpotSize);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Initiate the possession challenge
    public void StartChallenge()
    {
        // If there is already a challenge running, stop it
        if (challengeCoroutine != null)
        {
            StopCoroutine(challengeCoroutine);
        }

        // Ensure current wins/losses are at 0
        currentWins = 0;
        currentLosses = 0;

        // Ensure the hitmarker is at the starting position
        ResetHitMarker();

        // Start the challenge
        challengeCoroutine = StartCoroutine(RunPossessionChallenge());
    }

    public IEnumerator RunPossessionChallenge()
    {
        Debug.Log("Start Possession Challenge");
        possessionChallengeActive = true;
        EnableVisuals();
        currentWins = 0;
        currentLosses = 0;

        // Get Rect Transforms
        RectTransform backgroundRect = backgroundBarImage.rectTransform;
        RectTransform hitMarkerRect = hitMarker.rectTransform;

        // Get widths
        float backgroundWidth = backgroundRect.rect.width;
        float hitMarkerWidth = hitMarkerRect.rect.width;

        // Determine the bounds of the bar
        float leftBound = -backgroundWidth / 2f + hitMarkerWidth / 2f;
        float rightBound = backgroundWidth / 2f - hitMarkerWidth / 2f;

        // Get the local position of the hit marker
        UnityEngine.Vector3 position = hitMarkerRect.localPosition;

        // Ensure the hitmarker is at the starting position
        ResetHitMarker();
        // Randomize initial location
        SetRandomSweetSpot(sweetSpotSize);

        Debug.Log("Looping Begun");
        // Loop until outcome is determined
        while (true)
        {
            // Move the hit marker
            position.x += speed * Time.deltaTime;
            hitMarkerRect.localPosition = position;

            // If player presses the interact key
            if (inputManager.interactAction.WasPressedThisFrame())
            {
                bool hit = hitSweetSpot();

                // If player hit the sweet spot
                if (hit)
                {
                    // Increment current wins
                    currentWins++;
                    // Play the hit effect
                    if (minigameHitEffect != null && currentWins < requiredWins)
                    {
                        Instantiate(minigameHitEffect, transform.position, transform.rotation, null);
                    }
                    // If the player has earned the required amount of wins...
                    if (currentWins >= requiredWins)
                    {
                        // Play the win effect
                        if (minigameWinEffect != null)
                        {
                            Instantiate(minigameWinEffect, transform.position, transform.rotation, null);
                        }
                        
                        yield return new WaitForSeconds(0.25f);
                        // Set result to 0 (Won)
                        challengeResult = 0;
                        // Set challengeActive to false
                        possessionChallengeActive = false;
                        yield break;
                    }
                }
                // If the player missed the sweet spot
                else
                {
                    // Incrememnt current losses
                    currentLosses++;
                    // Play the miss effect
                    if (minigameMissEffect != null && currentLosses <= allowedLosses)
                    {
                        Instantiate(minigameMissEffect, transform.position, transform.rotation, null);
                    }
                    if (currentLosses > allowedLosses)
                    {
                        // Play the loss effect
                        if (minigameLossEffect != null)
                        {
                            Instantiate(minigameLossEffect, transform.position, transform.rotation, null);
                        }
                        yield return new WaitForSeconds(0.25f);
                        // Set result to 1 (Lost)
                        challengeResult = 1;
                        // Set challengeActive to false
                        possessionChallengeActive = false;
                        yield break;
                    }
                }
                ResetHitMarker();
                position = hitMarkerRect.localPosition;
                SetRandomSweetSpot(sweetSpotSize);
            }

            // If the hitmarker has hit the right bounds of the bar
            if (position.x >= rightBound)
            {
                // Incrememnt current losses
                currentLosses++;
                // Play the miss effect
                if (minigameMissEffect != null && currentLosses <= allowedLosses)
                {
                    Instantiate(minigameMissEffect, transform.position, transform.rotation, null);
                }
                if (currentLosses > allowedLosses)
                {
                    // Play the loss effect
                    if (minigameLossEffect != null)
                    {
                        Instantiate(minigameLossEffect, transform.position, transform.rotation, null);
                    }
                    yield return new WaitForSeconds(0.25f);
                    // Set result to 0 (Won)
                    challengeResult = 1;
                    // Set challengeActive to false
                    possessionChallengeActive = false;
                    yield break;
                }
                ResetHitMarker();
                position = hitMarkerRect.localPosition;
                SetRandomSweetSpot(sweetSpotSize);
            }
            yield return null;
        }
    }

    #region Hit Marker Management
    public bool hitSweetSpot()
    {
        // Get Rect Transforms
        RectTransform sweetSpotRect = sweetSpot.rectTransform;
        RectTransform hitMarkerRect = hitMarker.rectTransform;

        // Get local positions
        float sweetSpotX = sweetSpotRect.localPosition.x;
        float hitMarkerX = hitMarkerRect.localPosition.x;

        // Get widths
        float sweetSpotWidth = sweetSpotRect.rect.width;
        float hitMarkerWidth = hitMarkerRect.rect.width;

        // Calculate min and max X bounds for both the sweet spot and hitmarker
        float sweetMin = sweetSpotX - (sweetSpotWidth / 2f);
        float sweetMax = sweetSpotX + (sweetSpotWidth / 2f);
        float hitMin = hitMarkerX - (hitMarkerWidth / 2f);
        float hitMax = hitMarkerX + (hitMarkerWidth / 2f);

        // Check for overlap
        bool isOverlapping = hitMax >= sweetMin && hitMin <= sweetMax;
        return isOverlapping;
    }

    // Resets the hitmarker to the beginning of the bar
    public void ResetHitMarker()
    {
        // Get RectTransfroms for the Backgound Bar and hitmarker
        RectTransform backgroundRect = backgroundBarImage.rectTransform;
        RectTransform hitMarkerRect = hitMarker.rectTransform;

        // Get Asset Widths
        float backgroundWidth = backgroundRect.rect.width;
        float hitMarkerWidth = hitMarkerRect.rect.width;

        // Calculate the leftmost position for the hit marker (fully inside the background)
        float leftmostX = -(backgroundWidth / 2f) + (hitMarkerWidth / 2f);

        // Set the hit marker's local position (keep Y and Z the same)
        hitMarkerRect.localPosition = new UnityEngine.Vector3(leftmostX, hitMarkerRect.localPosition.y, hitMarkerRect.localPosition.z);
    }
    #endregion

    #region Sweet Spot Management
    // Randomizes the width and position of the sweet spot
    public void SetRandomSweetSpot(float width)
    {
        // Set new width
        ChangeSweetSpotWidth(width);

        // Get the RectTransforms for the main bar and sweet spot
        RectTransform backgroundRect = backgroundBarImage.rectTransform;
        RectTransform sweetSpotRect = sweetSpot.rectTransform;

        // Get the updated widths of the bars
        float backgroundWidth = backgroundRect.rect.width;
        float sweetSpotWidth = sweetSpotRect.rect.width;

        // Cacluclate the min and max Y values to keep the sweet spot fully within the background
        float minX = -(backgroundWidth / 2f) + (sweetSpotWidth / 2f);
        float maxX = (backgroundWidth / 2f) - (sweetSpotWidth / 2f);

        // Generate a new Random x within bounds
        float randomX = UnityEngine.Random.Range(minX, maxX);

        // Apply the new position
        ChangeSweetSpotPosition(randomX);
    }

    // Changes the width of the Sweet Spot Indicator to a specified value
    public void ChangeSweetSpotWidth(float newWidth)
    {
        // Reference Sweet Spot's rect transform
        RectTransform sweetSpotRect = sweetSpot.rectTransform;

        // Get the current size
        UnityEngine.Vector2 size = sweetSpotRect.sizeDelta;

        size.x = newWidth;
        sweetSpotRect.sizeDelta = size;

    }

    // Changes the position of the Sweet Spot Indicator, keeping it within bounds of the backgound bar
    public void ChangeSweetSpotPosition(float newX)
    {
        RectTransform backgroundRect = backgroundBarImage.rectTransform;
        RectTransform sweetSpotRect = sweetSpot.rectTransform;

        float backgroundWidth = backgroundRect.rect.width;
        float sweetSpotWidth = sweetSpotRect.rect.width;

        // Calculate horizontal bounds
        float minX = -(backgroundWidth / 2f) + (sweetSpotWidth / 2f);
        float maxX = (backgroundWidth / 2f) - (sweetSpotWidth / 2f);

        // Clamp newX to ensure sweetSpot stays fully within the background
        float clampedX = Mathf.Clamp(newX, minX, maxX);

        // Apply clamped local position (Y and Z should remain the same)
        sweetSpotRect.localPosition = new UnityEngine.Vector3(clampedX, sweetSpotRect.localPosition.y, sweetSpotRect.localPosition.z);
    }
    #endregion

    public bool isChallengeActive()
    {
        return possessionChallengeActive;
    }

    public int getChallengeResult()
    {
        return challengeResult;
    }

    #region Visuals
    // Disables all visual components of the challenge bar
    public void DisableVisuals()
    {
        backgroundBarImage.gameObject.SetActive(false);
        sweetSpot.gameObject.SetActive(false);
        hitMarker.gameObject.SetActive(false);
        border.gameObject.SetActive(false);
    }

    // Enables all visual componenets of the challenge bar
    public void EnableVisuals()
    {
        backgroundBarImage.gameObject.SetActive(true);
        sweetSpot.gameObject.SetActive(true);
        hitMarker.gameObject.SetActive(true);
        border.gameObject.SetActive(true);
    }
    #endregion
}
