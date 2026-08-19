using UnityEngine;

/// <summary>
/// Auto-fires projectiles while TankTargeting has a target.
/// Knows nothing about movement or how the target was chosen.
/// </summary>
[RequireComponent(typeof(TankTargeting))]
public class TankShooter : MonoBehaviour
{
    [Header("Projectile")]
    [Tooltip("Prefab spawned on every shot. Must have a Projectile component.")]
    [SerializeField] private Projectile projectilePrefab;

    [Tooltip("Muzzle transform. Its 'up' or the direction to the target defines where the shot goes.")]
    [SerializeField] private Transform firePoint;

    [Header("Fire Rate")]
    [Tooltip("Shots per second.")]
    [SerializeField] private float fireRate = 3f;

    [Tooltip("Turn off to disable automatic firing (useful for testing).")]
    [SerializeField] private bool autoFire = true;

    private TankTargeting targeting;
    private float cooldown;

    private void Awake()
    {
        targeting = GetComponent<TankTargeting>();

        if (projectilePrefab == null)
            Debug.LogWarning($"{name}: TankShooter has no projectile prefab assigned.", this);
        if (firePoint == null)
            Debug.LogWarning($"{name}: TankShooter has no fire point assigned.", this);
    }

    private void Update()
    {
        cooldown -= Time.deltaTime;

        if (!autoFire || !targeting.HasTarget) return;
        if (cooldown > 0f) return;

        Shoot(targeting.CurrentTarget);
        cooldown = fireRate > 0f ? 1f / fireRate : 0.1f;
    }

    private void Shoot(Transform target)
    {
        if (projectilePrefab == null || firePoint == null || target == null) return;

        Vector2 direction = ((Vector2)(target.position - firePoint.position)).normalized;
        if (direction.sqrMagnitude < 0.0001f) direction = firePoint.up;

        Projectile projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        projectile.Launch(direction);
    }
}
