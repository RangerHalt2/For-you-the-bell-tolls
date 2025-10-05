// Created By: Ryan Lupoli
// This is a script meant manage the healthbars used by objects in game
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Tooltip("Healthbar GameObject.")]
    [SerializeField] private GameObject healthBar;

    [Tooltip("Gradient of colors which the health bar will use.")]
    [SerializeField] private Gradient gradient;

    [Tooltip("Fill object from Healthbar Prefab.")]
    [SerializeField] private Image fill;

    private float maxHealth = 100f;

    public void SetMaxHealth(float health)
    {
        maxHealth = health;
        SetHealth(health);

        // Update Color
        fill.color = gradient.Evaluate(1f);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetHealth(float health)
    {
        // Set the slider's value to the current percentage of health the object has
        float fillPercent = Mathf.Clamp01(health / maxHealth);
        fill.fillAmount = fillPercent;

        // Update Color
        fill.color = gradient.Evaluate(fillPercent);
    }

    // Enables the health Bar Gameobject
    public void Activate()
    {
        healthBar.SetActive(true);
    }
    public void Deactivate()
    {
        healthBar.SetActive(false);
    }
}
