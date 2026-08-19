using UnityEngine;

/// <summary>
/// Straight-flying bullet. Travels in the launch direction, damages what it hits on the
/// enemy layer, then despawns. Also despawns when its lifetime runs out.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Flight")]
    [Tooltip("Speed in units per second.")]
    [SerializeField] private float speed = 12f;

    [Tooltip("Seconds before the projectile despawns on its own.")]
    [SerializeField] private float lifetime = 3f;

    [Tooltip("Angle offset for the projectile sprite. Use -90 if it points UP, 0 if it points RIGHT.")]
    [SerializeField] private float spriteAngleOffset = -90f;

    [Header("Damage")]
    [Tooltip("Damage dealt to an enemy on hit.")]
    [SerializeField] private int damage = 1;

    [Tooltip("Layer(s) this projectile can hit.")]
    [SerializeField] private LayerMask hitLayers;

    [Tooltip("Optional extra filter. Leave empty to accept any collider on the hit layers.")]
    [SerializeField] private string enemyTag = "Enemy";

    [Tooltip("Require the collider to also carry the tag above.")]
    [SerializeField] private bool requireTag = false;

    private Rigidbody2D rb;
    private float lifeTimer;
    private bool hasHit;

    /// <summary>Damage this projectile carries. Read by enemy health later.</summary>
    public int Damage => damage;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    private void OnEnable()
    {
        lifeTimer = lifetime;
        hasHit = false;
    }

    /// <summary>Fires the projectile in a world-space direction. Call right after spawning.</summary>
    public void Launch(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f) direction = Vector2.up;
        direction.Normalize();

        transform.rotation = Quaternion.Euler(0f, 0f,
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + spriteAngleOffset);

        rb.linearVelocity = direction * speed;
    }

    private void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
            Despawn();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        if (other == null) return;

        // Layer check first, tag only if explicitly required.
        if ((hitLayers.value & (1 << other.gameObject.layer)) == 0) return;
        if (requireTag && !other.CompareTag(enemyTag)) return;

        hasHit = true;
        ApplyDamage(other);
        Despawn();
    }

    /// <summary>
    /// Single integration point for enemy damage.
    /// When EnemyHealth exists, do the damage call here - nothing else needs to change.
    /// </summary>
    private void ApplyDamage(Collider2D target)
    {
        // TODO: enemy health hook
        // if (target.TryGetComponent(out EnemyHealth health))
        //     health.TakeDamage(damage);

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterEnemyHit();
    }

    private void Despawn()
    {
        // Swapped for a pool return later; callers do not care either way.
        Destroy(gameObject);
    }
}
