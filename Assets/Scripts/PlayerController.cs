using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Attack")]
    public float attackCooldown = 0.5f;
    public float attackActiveDuration = 0.2f;
    public PlayerAttack playerAttack;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private bool isGrounded;
    private bool facingRight = true;
    private float attackTimer = 0f;
    private float attackActiveTimer = 0f;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        if (playerAttack != null)
            playerAttack.SetFacing(facingRight);
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        attackTimer -= Time.deltaTime;
        attackActiveTimer -= Time.deltaTime;

        if (attackActiveTimer <= 0f && playerAttack != null)
            playerAttack.DisableHitbox();

        if (Input.GetMouseButtonDown(0) && attackTimer <= 0f)
        {
            animator.SetTrigger("Attack");
            attackTimer = attackCooldown;

            if (playerAttack != null)
            {
                playerAttack.EnableHitbox();
                attackActiveTimer = attackActiveDuration;
            }
        }

        animator.SetBool("isWalking", moveInput != 0);
        animator.SetBool("isJumping", !isGrounded);

        if (moveInput > 0 && !facingRight)
            Flip();
        else if (moveInput < 0 && facingRight)
            Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        sr.flipX = !sr.flipX;

        if (playerAttack != null)
            playerAttack.SetFacing(facingRight);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
