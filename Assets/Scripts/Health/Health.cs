// Created By: Ryan Lupoli
// This is a script meant to track a game object's health
using UnityEngine;
using System.Collections;
using TMPro;

public class Health : MonoBehaviour
{
    #region Variables
    [Header("Team Settings")]
    [Tooltip("Determines the \"Team\" the object is on. Objects on the same team cannot damage each other.")]
    public int teamID;

    [Header("Health Settings")]
    [Tooltip("The maximum amount of health an object can. Value must be greater than 0.")]
    [SerializeField] public float maxHealth = 1f;
    [Tooltip("The amount of health the object currently has. If the current health is 0, the object is dead.")]
    [SerializeField] public float currentHealth = 1f;

    [Header("Display Settings")]
    [Tooltip("Determines if the Health Bar is active before taking damage")]
    [SerializeField] private bool healthBarActiveOnStartup;
    // Tracks if the healthbar is currently active
    private bool healthBarActive;
    [Tooltip("Reference to healthbar prefab. Optional.")]
    [SerializeField] private HealthBar healthBar;
    [Tooltip("Reference to TMPro Object used to track current AP. Optional.")]
    [SerializeField] private TextMeshProUGUI healthDisplayText;

    [Header("Effect Settings")]
    [Tooltip("Reference to prefab for an effect which triggers when the object recieves damage. Optional.")]
    public GameObject hitEffect;
    [Tooltip("Reference to prefab for an effect which triggers when the object is destroyed. Optional.")]
    public GameObject deathEffect;

    [Tooltip("Toggle this if this is on the player")]
    [SerializeField] private bool isPlayer;

    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private GameObject inGameCanvas;

    public bool isDead;

    private PossessionManager pm;

    [SerializeField] private GameObject damageNoise;
    private float damageTimer = 0f;
    private float damageCooldown = 0.5f;

    [SerializeField] private bool willDie;
    [SerializeField] float stunTime;
    private IController controller;

    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isDead = false;
        // Automatically kill object if it has 0 or less health
        if (currentHealth <= 0)
        {
            Debug.Log(gameObject.name + "'s Initial Health was equal to or less than 0. They have been automatically destroyed.");
            Die();
        }

        // If there is a healthbar assigned, set the max health
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
        }

        updateDisplay();

        // If the health bar is not meant to be active immediatly, deactivate it
        if (healthBar != null && !healthBarActiveOnStartup)
        {
            healthBar.Deactivate();
        }

        controller = GetComponent<IController>();
    }

    // Update is called once per frame
    void Update()
    {
        damageTimer -= Time.deltaTime;
    }

    // Applies a certain amount of damage to an object
    public void TakeDamage(float damageAmount)
    {
        // Subtract the damage amount from the health of the object
        currentHealth -= damageAmount;
        if(damageNoise != null && damageTimer <= 0)
        {
            damageTimer = damageCooldown;
            Instantiate(damageNoise, transform.position, transform.rotation, null);
        }
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Current Health: " + currentHealth + "/" + maxHealth + ".");
        updateDisplay();

        // If the object has 0 or less current health
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // If a hit effect has been assigned
            if (hitEffect != null)
            {
                // Play the hit effect for the object
                Instantiate(hitEffect, transform.position, transform.rotation, null);
            }
        }
    }

    // Applies a certain amount of healing to an object
    public void ReceiveHealing(float healingAmount)
    {
        // Add the healing amount to the object's current health
        currentHealth += healingAmount;
        Debug.Log(gameObject.name + " received " + healingAmount + " healing. Current Health: " + currentHealth + "/" + maxHealth + ".");
        updateDisplay();

        // If the object's current health is now greater than the max...
        if (currentHealth > maxHealth)
        {
            // Set current health to max health
            currentHealth = maxHealth;
        }
    }

    public void Die()
    {

        pm = GetComponent<PossessionManager>();
        if (!pm.isPlayerGhost)
        {
            pm.ExitHost();
        }

        // If a death effect has been assigned
        if (deathEffect != null)
        {
            // Play the death effect for the object
            Instantiate(deathEffect, transform.position, transform.rotation, null);
        }

        // Destroy the game object
        Debug.Log(gameObject.name + " has died.");
        if(!isPlayer)
            Destroy(gameObject);
        else if (isPlayer)
        {
            if (willDie)
            {
                isDead = true;
                gameOverCanvas.SetActive(true);
                inGameCanvas.SetActive(false);
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                StartCoroutine(Stun());
            }
        }
    }

    private IEnumerator Stun()
    {
        if (controller != null)
        {
            Debug.Log(gameObject.name + " has been stunned for " + stunTime + " seconds!");
            controller.DisableControl();
            yield return new WaitForSeconds(stunTime);
            controller.EnableControl();
            currentHealth = maxHealth;
            updateDisplay();
        }
        
    }

    public void updateDisplay()
    {
        // Used if there is a TMPro Display assinged
        if (healthDisplayText != null)
        {
            healthDisplayText.text = string.Format(currentHealth + " / " + maxHealth);
        }

        // If there is a healthBar assigned
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
            healthBar.Activate();
            healthBarActive = true;
        }
    }
}
