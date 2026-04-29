using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack")]
    public int attackDamage = 1;
    public float knockbackForce = 5f;

    private Collider2D hitbox;
    private float colliderOffsetX;

    void Start()
    {
        hitbox = GetComponent<Collider2D>();
        hitbox.enabled = false;
        colliderOffsetX = Mathf.Abs(hitbox.offset.x);
    }

    public void SetFacing(bool facingRight)
    {
        Vector2 offset = hitbox.offset;
        offset.x = facingRight ? colliderOffsetX : -colliderOffsetX;
        hitbox.offset = offset;
    }

    // Called by animation event at the start of the attack active frame
    public void EnableHitbox()
    {
        hitbox.enabled = true;
    }

    // Called by animation event at the end of the attack active frame
    public void DisableHitbox()
    {
        hitbox.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph != null)
            ph.TakeDamage(attackDamage);

        Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            Vector2 knockDir = (other.transform.position - transform.parent.position).normalized;
            playerRb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
        }
    }
}
