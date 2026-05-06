using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    public PlayerHealthUI healthUI;

    [Header("Invincibility Frames")]
    public float invincibilityDuration = 0.75f;

    [Header("Audio")]
    public AudioClip hitSound;
    public AudioClip deathSound;
    public AudioSource hitAudioSource;
    public AudioSource musicSource;

    public static bool IsDead { get; private set; }

    private int currentHealth;
    private float invincibilityTimer = 0f;
    private Animator animator;

    void Start()
    {
        IsDead = false;
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

        if (hitSound != null && hitAudioSource != null)
            hitAudioSource.PlayOneShot(hitSound);

        if (animator != null)
            animator.SetTrigger("Hurt");

        if (healthUI != null)
            healthUI.UpdateHearts(currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        IsDead = true;
        if (deathSound != null && hitAudioSource != null)
            hitAudioSource.PlayOneShot(deathSound);

        if (musicSource != null)
            musicSource.Stop();
        if (animator != null)
        {
            animator.SetTrigger("Die");
            StartCoroutine(FreezeAfterDeath());
        }
    }

    System.Collections.IEnumerator FreezeAfterDeath()
    {
        yield return null;
        while (animator.IsInTransition(0))
            yield return null;

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.5f)
            yield return null;

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.97f)
            yield return null;

        if (animator != null)
            animator.speed = 0f;
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}
