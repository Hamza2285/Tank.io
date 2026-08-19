using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string playerTag = "Player";

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [Tooltip("Angle correction if the sprite doesn't face right by default. Use -90 for up-facing sprites, 0 for right-facing sprites.")]
    [SerializeField] private float spriteAngleOffset = -90f;

    [Header("Combat")]
    [Tooltip("Enemy stops moving and starts attacking once the Player is within this range.")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 8f;

    private Rigidbody2D rb;
    private Transform player;
    private float nextFireTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (firePoint == null)
            firePoint = transform;
    }

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        RotateTowardsPlayer();

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange && Time.time >= nextFireTime)
        {
            Attack();
            nextFireTime = Time.time + attackCooldown;
        }
    }

    private void FixedUpdate()
    {
        if (player == null)
            return;

        float distance = Vector2.Distance(rb.position, player.position);

        if (distance > attackRange)
        {
            Vector2 direction = ((Vector2)player.position - rb.position).normalized;
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
        }
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
            player = playerObject.transform;
    }

    private void RotateTowardsPlayer()
    {
        Vector2 direction = (Vector2)player.position - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + spriteAngleOffset;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Attack()
    {
        if (bulletPrefab == null)
            return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

        if (bulletRb != null)
        {
            Vector2 direction = ((Vector2)player.position - (Vector2)firePoint.position).normalized;
            bulletRb.linearVelocity = direction * bulletSpeed;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
