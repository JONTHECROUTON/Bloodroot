using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector-configurable values
    // -------------------------------------------------------------------------

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float jumpForce = 8f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Edge / Wall Detection")]
    public Transform edgeCheck;          // place at feet, forward of enemy
    public float edgeCheckRadius = 0.15f;
    public LayerMask wallLayer;          // assign the same layer(s) as walls/platforms

    [Header("Patrol")]
    public float patrolDistance = 4f;    // how far from spawn point to patrol
    public float patrolWaitTime = 1f;    // pause at each patrol end-point

    [Header("Chase / Detection")]
    public float detectionRange = 6f;    // distance at which enemy notices player
    public float attackRange = 1f;       // distance at which enemy attacks
    public Transform player;             // assign the Player transform in Inspector

    [Header("Combat")]
    public int maxHealth = 3;
    public float attackCooldown = 1f;
    public float contactDamageKnockback = 5f;
    public int attackDamage = 1;         // damage dealt to player per hit

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;

    private bool isGrounded;
    private bool facingRight = true;

    private int currentHealth;
    private float attackTimer = 0f;

    private Vector2 spawnPosition;
    private float patrolTimer = 0f;
    private bool patrollingRight = true;
    private bool waitingAtEdge = false;

    private enum State { Patrol, Chase, Attack, Dead }
    private State currentState = State.Patrol;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
        spawnPosition = transform.position;

        // Auto-find player by tag if not assigned in Inspector
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void Update()
    {
        if (currentState == State.Dead) return;

        attackTimer -= Time.deltaTime;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        float distanceToPlayer = player != null ? Vector2.Distance(transform.position, player.position) : Mathf.Infinity;

        // ---- State transitions ----
        if (distanceToPlayer <= attackRange)
            currentState = State.Attack;
        else if (distanceToPlayer <= detectionRange)
            currentState = State.Chase;
        else
            currentState = State.Patrol;

        // ---- State behaviour ----
        switch (currentState)
        {
            case State.Patrol: DoPatrol(); break;
            case State.Chase:  DoChase();  break;
            case State.Attack: DoAttack(); break;
        }

        UpdateAnimator();
    }

    // -------------------------------------------------------------------------
    // State behaviours
    // -------------------------------------------------------------------------

    void DoPatrol()
    {
        if (waitingAtEdge)
        {
            patrolTimer -= Time.deltaTime;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (patrolTimer <= 0f)
            {
                waitingAtEdge = false;
                patrollingRight = !patrollingRight;
                FaceDirection(patrollingRight);
            }
            return;
        }

        float targetX = spawnPosition.x + (patrollingRight ? patrolDistance : -patrolDistance);
        bool reachedEnd = patrollingRight
            ? transform.position.x >= targetX
            : transform.position.x <= targetX;

        bool wallAhead  = IsWallAhead();
        bool edgeAhead  = !IsGroundAhead();

        if (reachedEnd || wallAhead || edgeAhead)
        {
            waitingAtEdge = true;
            patrolTimer = patrolWaitTime;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float dir = patrollingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }

    void DoChase()
    {
        if (player == null) return;

        float dir = player.position.x > transform.position.x ? 1f : -1f;
        FaceDirection(dir > 0);

        bool wallAhead = IsWallAhead();
        bool edgeAhead = !IsGroundAhead();

        // Try to jump over walls when chasing
        if (wallAhead && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Don't walk off edges while chasing (optional — remove to make more aggressive)
        if (!edgeAhead)
            rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void DoAttack()
    {
        if (player == null) return;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        FaceDirection(player.position.x > transform.position.x);

        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;

            if (animator != null)
                animator.SetTrigger("Attack");

            // Try to deal damage via a PlayerHealth component on the player
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(attackDamage);
        }
    }

    // -------------------------------------------------------------------------
    // Health / damage
    // -------------------------------------------------------------------------

    public void TakeDamage(int amount)
    {
        if (currentState == State.Dead) return;

        currentHealth -= amount;

        if (animator != null)
            animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        currentState = State.Dead;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetTrigger("Die");

        // Disable collider so the corpse doesn't block the player
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // Destroy after a short delay to allow death animation to play
        Destroy(gameObject, 1.5f);
    }

    // -------------------------------------------------------------------------
    // Contact damage — hurts the player when they walk into the enemy
    // -------------------------------------------------------------------------

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.Dead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(1);

            // Knock the player back away from the enemy
            Vector2 knockDir = (collision.transform.position - transform.position).normalized;
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
                playerRb.AddForce(knockDir * contactDamageKnockback, ForceMode2D.Impulse);
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    bool IsGroundAhead()
    {
        if (edgeCheck == null) return true; // fail-safe: assume ground exists
        return Physics2D.OverlapCircle(edgeCheck.position, edgeCheckRadius, groundLayer);
    }

    bool IsWallAhead()
    {
        if (edgeCheck == null) return false;
        return Physics2D.OverlapCircle(edgeCheck.position, edgeCheckRadius, wallLayer);
    }

    void FaceDirection(bool right)
    {
        if (facingRight == right) return;
        facingRight = right;
        if (sr != null)
            sr.flipX = !sr.flipX;
    }

    void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetBool("isWalking", Mathf.Abs(rb.linearVelocity.x) > 0.05f);
        animator.SetBool("isGrounded", isGrounded);
    }

    // -------------------------------------------------------------------------
    // Editor gizmos
    // -------------------------------------------------------------------------

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (edgeCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(edgeCheck.position, edgeCheckRadius);
        }

        // Detection range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
