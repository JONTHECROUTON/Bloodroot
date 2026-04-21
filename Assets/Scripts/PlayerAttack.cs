using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    public int attackDamage = 1;
    public float knockbackForce = 6f;

    private Collider2D hitbox;
    private float transformOffsetX;
    private float colliderOffsetX;

    void Awake()
    {
        hitbox = GetComponent<Collider2D>();
        hitbox.enabled = false;

        transformOffsetX = Mathf.Abs(transform.localPosition.x);
        colliderOffsetX = Mathf.Abs(hitbox.offset.x);
    }

    public void SetFacing(bool facingRight)
    {
        Vector3 pos = transform.localPosition;
        pos.x = facingRight ? transformOffsetX : -transformOffsetX;
        transform.localPosition = pos;

        Vector2 offset = hitbox.offset;
        offset.x = facingRight ? colliderOffsetX : -colliderOffsetX;
        hitbox.offset = offset;
    }

    public void EnableHitbox()  { hitbox.enabled = true; }
    public void DisableHitbox() { hitbox.enabled = false; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        ScorpionAI scorpion = other.GetComponentInParent<ScorpionAI>();
        if (scorpion != null)
        {
            scorpion.TakeDamage(attackDamage);
        }
        else
        {
            EnemyController enemy = other.GetComponentInParent<EnemyController>();
            if (enemy != null)
                enemy.TakeDamage(attackDamage);
        }

        Rigidbody2D enemyRb = other.GetComponentInParent<Rigidbody2D>();
        if (enemyRb != null)
        {
            Vector2 knockDir = (other.transform.position - transform.parent.position).normalized;
            enemyRb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
        }
    }
}
