using UnityEngine;
using UnityEngine.UI;

public class DeathManager : MonoBehaviour
{
    #region Variables
    [Header("Settings")]
    [Tooltip("How long the player can be out of a body roughly in seconds")]
    [SerializeField] private float maxFadeTime = 30f;
    private float fadeTimer = 0f;

    private bool isFading = false;

    private GameObject lossCanvas;
    private GameObject gameCanvas;

    private Image fadeBar;

    private PlayerController playerController;
    #endregion

    #region Getters/Setters
    public void SetIsFading(bool isFading)
    {
        this.isFading = isFading;
    }

    public bool GetIsFading()
    {
        return isFading;
    }
    #endregion

    private void Start()
    {
        fadeTimer = maxFadeTime;
        isFading = true;
        playerController = GameObject.FindAnyObjectByType<PlayerController>();
        fadeBar = GameObject.FindGameObjectWithTag("DecayBar").GetComponent<Image>();
    }

    public void ResetFadeTimer()
    {
        fadeTimer = maxFadeTime;
    }

    private void Update()
    {
        DoFade();
        UpdateFadeBar();
    }

    private void DoFade()
    {
        if (isFading)
        {
            if (fadeTimer <= 0f)
            {
                Cursor.lockState = CursorLockMode.None;
                playerController.SetIsDead(true);
                lossCanvas.gameObject.SetActive(true);
                gameCanvas.gameObject.SetActive(false);
                isFading = false;
            }
            fadeTimer -= Time.deltaTime;
            //Debug.Log("Fade Timer Left: " + fadeTimer);
        }
    }

    private void UpdateFadeBar()
    {
        if (isFading && fadeBar != null)
        {
            float progress = (fadeTimer / maxFadeTime);
            fadeBar.fillAmount = progress;
        }
    }
}
