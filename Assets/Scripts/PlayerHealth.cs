using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;

    [Header("Invincibility Frames")]
    public float invincibilityDuration = 0.75f;

    private int currentHealth;
    private float invincibilityTimer = 0f;
    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (invincibilityTimer > 0f)
            invincibilityTimer -= Time.deltaTime;
    }

    public void TakeDamage(int amount)
    {
        if (invincibilityTimer > 0f) return; // still invincible

        currentHealth -= amount;
        invincibilityTimer = invincibilityDuration;

        if (animator != null)
            animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (animator != null)
            animator.SetTrigger("Die");

        // TODO: hook into your game manager (scene reload, game-over screen, etc.)
        Debug.Log("Player died.");
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}
