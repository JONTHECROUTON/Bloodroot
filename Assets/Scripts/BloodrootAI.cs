using UnityEngine;

public class BloodrootAI : MonoBehaviour
{
    [Header("Detection")]
    public float attackRange = 2f;

    [Header("Combat")]
    public int maxHealth = 3;
    public int attackDamage = 1;
    public float attackCooldown = 2.5f;
    public float attackWindUp = 0.5f;
    public float attackAnimDuration = 0.75f;

    [Header("Attack Hitbox")]
    public EnemyAttack attackHitbox;
    public float hitboxActiveDuration = 0.3f;

    [Header("Audio")]
    public AudioClip attackSound;
    public AudioClip hitSound;

    private Animator animator;
    private SpriteRenderer sr;
    private AudioSource audioSource;
    private Transform player;

    private int currentHealth;
    private float attackTimer = 0f;
    private float windUpTimer = 0f;
    private bool isAttackInProgress = false;
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (isDead) return;
        if (PlayerHealth.IsDead) return;

        attackTimer -= Time.deltaTime;

        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange && !isAttackInProgress)
        {
            FacePlayer();
            windUpTimer += Time.deltaTime;

            if (windUpTimer >= attackWindUp && attackTimer <= 0f)
                StartCoroutine(DoAttack());
        }
        else if (!isAttackInProgress)
        {
            windUpTimer = 0f;
        }
    }

    void FacePlayer()
    {
        if (player == null) return;
        bool facingRight = player.position.x > transform.position.x;
        sr.flipX = facingRight;
        if (attackHitbox != null)
            attackHitbox.SetFacing(facingRight);
    }

    System.Collections.IEnumerator DoAttack()
    {
        isAttackInProgress = true;
        attackTimer = attackCooldown;
        windUpTimer = 0f;

        animator.SetTrigger("Attack");

        if (attackSound != null && audioSource != null)
            audioSource.PlayOneShot(attackSound);

        if (attackHitbox != null)
            StartCoroutine(HitboxPulse());

        yield return new WaitForSeconds(attackAnimDuration);
        isAttackInProgress = false;
    }

    System.Collections.IEnumerator HitboxPulse()
    {
        attackHitbox.EnableHitbox();
        yield return new WaitForSeconds(hitboxActiveDuration);
        attackHitbox.DisableHitbox();
    }

    public void TakeDamage(int amount = 1)
    {
        if (isDead) return;
        currentHealth -= amount;
        if (animator != null)
            animator.SetTrigger("Hurt");
        if (hitSound != null && audioSource != null)
            audioSource.PlayOneShot(hitSound);
        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;
        if (animator != null)
        {
            animator.SetTrigger("Death");
            StartCoroutine(FreezeAfterDeath());
        }
        Destroy(gameObject, 3f);
    }

    System.Collections.IEnumerator FreezeAfterDeath()
    {
        yield return null;
        while (animator.IsInTransition(0))
            yield return null;

        // Wait until past halfway so we don't catch the start
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.5f)
            yield return null;

        // Then freeze just before the loop point
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.97f)
            yield return null;

        if (animator != null)
            animator.speed = 0f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
