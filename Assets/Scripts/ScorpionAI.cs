using UnityEngine;

public class ScorpionAI : MonoBehaviour
{
    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("Patrol")]
    public float patrolDistance = 5f;
    public float waitTime = 1.5f;

    [Header("Detection")]
    public float detectionRange = 6f;
    public float attackRange = 1.5f;

    [Header("Combat")]
    public int maxHealth = 3;
    public int attackDamage = 1;
    public float attackCooldown = 3f;
    public float attackWindUp = 1.5f;
    public float contactKnockback = 5f;

    [Header("Attack Hitbox")]
    public EnemyAttack attackHitbox;
    public float hitboxActiveDuration = 0.3f;

    [Header("Audio")]
    public AudioClip swingSound;

    private Animator animator;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private Transform player;

    private int currentHealth;
    private Vector2 startPosition;
    private bool movingRight = true;
    private float hitboxOffsetX;
    private float waitTimer = 0f;
    private float attackTimer = 0f;
    private float attackLockTimer = 0f;
    private float windUpTimer = 0f;
    private bool isWaiting = false;
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        currentHealth = maxHealth;
        startPosition = transform.position;

        if (attackHitbox != null)
            hitboxOffsetX = Mathf.Abs(attackHitbox.transform.localPosition.x);

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (isDead) return;

        attackTimer -= Time.deltaTime;
        attackLockTimer -= Time.deltaTime;

        if (player == null) { DoPatrol(); return; }

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange || attackLockTimer > 0f)
        {
            windUpTimer += Time.deltaTime;
            DoAttack();
        }
        else
        {
            windUpTimer = 0f;
            if (dist <= detectionRange)
                DoChase();
            else
                DoPatrol();
        }
    }

    void DoPatrol()
    {
        if (isWaiting)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            SetAnimState(walking: false, waiting: true, attacking: false);

            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                movingRight = !movingRight;
            }
            return;
        }

        SetAnimState(walking: true, waiting: false, attacking: false);

        float dir = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * patrolSpeed, rb.linearVelocity.y);
        sr.flipX = movingRight;
        FlipHitbox(movingRight);

        float offset = transform.position.x - startPosition.x;
        if ((movingRight && offset >= patrolDistance) || (!movingRight && offset <= -patrolDistance))
        {
            isWaiting = true;
            waitTimer = waitTime;
        }
    }

    void DoChase()
    {
        SetAnimState(walking: true, waiting: false, attacking: false);

        float dir = player.position.x > transform.position.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);
        sr.flipX = dir > 0;
        FlipHitbox(dir > 0);
    }

    void DoAttack()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        bool facingRight = player.position.x > transform.position.x;
        sr.flipX = facingRight;
        FlipHitbox(facingRight);

        if (attackTimer > 0f || windUpTimer < attackWindUp)
        {
            SetAnimState(walking: false, waiting: true, attacking: false);
            return;
        }

        attackTimer = attackCooldown;
        attackLockTimer = attackCooldown;
        windUpTimer = 0f;
        SetAnimState(walking: false, waiting: false, attacking: true);

        if (swingSound != null && audioSource != null)
            audioSource.PlayOneShot(swingSound);

        if (attackHitbox != null)
            StartCoroutine(HitboxPulse());
    }

    public void TakeDamage(int amount = 1)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
            Die();
    }

    System.Collections.IEnumerator HitboxPulse()
    {
        attackHitbox.EnableHitbox();
        yield return new WaitForSeconds(hitboxActiveDuration);
        attackHitbox.DisableHitbox();
    }

    System.Collections.IEnumerator StopAnimatorAfterDeath()
    {
        yield return null;
        while (animator.IsInTransition(0))
            yield return null;

        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);
        if (animator != null)
            animator.speed = 0f;
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isWaiting", false);
            animator.SetBool("isAttacking", false);
            animator.SetTrigger("Death");
        }

        // Ignore collision with player so corpse doesn't block them,
        // but keep colliders active so the body stays on the ground.
        if (player != null)
        {
            Collider2D playerCol = player.GetComponent<Collider2D>();
            foreach (Collider2D col in GetComponents<Collider2D>())
            {
                if (playerCol != null)
                    Physics2D.IgnoreCollision(col, playerCol);
            }
        }

        Destroy(gameObject, 2f);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(attackDamage);

            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 knockDir = (collision.transform.position - transform.position).normalized;
                playerRb.AddForce(knockDir * contactKnockback, ForceMode2D.Impulse);
            }
        }
    }

    void FlipHitbox(bool facingRight)
    {
        if (attackHitbox == null) return;
        attackHitbox.SetFacing(facingRight);
    }

    void SetAnimState(bool walking, bool waiting, bool attacking)
    {
        if (animator == null) return;
        animator.SetBool("isWalking", walking);
        animator.SetBool("isWaiting", waiting);
        animator.SetBool("isAttacking", attacking);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
