using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Finds the nearest enemy inside a detection radius and smoothly aims the cannon pivot at it.
/// Does not shoot - TankShooter reads <see cref="CurrentTarget"/>.
/// </summary>
public class TankTargeting : MonoBehaviour
{
    [Header("Cannon")]
    [Tooltip("Child transform that rotates independently from the tank body.")]
    [SerializeField] private Transform cannonPivot;

    [Tooltip("Angle offset for the cannon sprite. Use -90 if it points UP, 0 if it points RIGHT.")]
    [SerializeField] private float spriteAngleOffset = -90f;

    [Tooltip("How fast the cannon turns toward the target (degrees per second).")]
    [SerializeField] private float aimSpeed = 360f;

    [Header("Detection")]
    [Tooltip("Radius in world units used to search for enemies.")]
    [SerializeField] private float detectionRadius = 8f;

    [Tooltip("Layer(s) enemies live on.")]
    [SerializeField] private LayerMask enemyLayer;

    [Tooltip("Seconds between target searches. Small values are more responsive, larger ones cheaper.")]
    [SerializeField] private float retargetInterval = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool drawDetectionGizmo = true;

    /// <summary>Nearest valid enemy, or null when nothing is in range.</summary>
    public Transform CurrentTarget { get; private set; }

    /// <summary>True while a target is locked. Convenience for TankShooter / UI.</summary>
    public bool HasTarget => CurrentTarget != null;

    private readonly List<Collider2D> results = new List<Collider2D>();
    private ContactFilter2D contactFilter;
    private float retargetTimer;

    private void Awake()
    {
        contactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = enemyLayer,
            useTriggers = true
        };

        if (cannonPivot == null)
            Debug.LogWarning($"{name}: TankTargeting has no CannonPivot assigned.", this);
    }

    private void Update()
    {
        // Drop the target as soon as it dies or leaves the radius, then re-scan.
        if (!IsTargetValid(CurrentTarget))
            CurrentTarget = null;

        retargetTimer -= Time.deltaTime;
        if (CurrentTarget == null || retargetTimer <= 0f)
        {
            retargetTimer = retargetInterval;
            CurrentTarget = FindNearestEnemy();
        }

        AimCannon();
    }

    private bool IsTargetValid(Transform target)
    {
        if (target == null || !target.gameObject.activeInHierarchy) return false;
        return ((Vector2)(target.position - transform.position)).sqrMagnitude <= detectionRadius * detectionRadius;
    }

    private Transform FindNearestEnemy()
    {
        results.Clear();
        contactFilter.layerMask = enemyLayer; // keep in sync if changed in the Inspector at runtime
        Physics2D.OverlapCircle(transform.position, detectionRadius, contactFilter, results);

        Transform nearest = null;
        float nearestSqr = float.MaxValue;

        for (int i = 0; i < results.Count; i++)
        {
            Collider2D hit = results[i];
            if (hit == null || !hit.gameObject.activeInHierarchy) continue;

            float sqr = ((Vector2)(hit.transform.position - transform.position)).sqrMagnitude;
            if (sqr < nearestSqr)
            {
                nearestSqr = sqr;
                nearest = hit.transform;
            }
        }

        return nearest;
    }

    private void AimCannon()
    {
        if (cannonPivot == null || CurrentTarget == null) return;

        Vector2 direction = CurrentTarget.position - cannonPivot.position;
        if (direction.sqrMagnitude < 0.0001f) return;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + spriteAngleOffset;
        float next = Mathf.MoveTowardsAngle(cannonPivot.eulerAngles.z, targetAngle, aimSpeed * Time.deltaTime);
        cannonPivot.rotation = Quaternion.Euler(0f, 0f, next);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawDetectionGizmo) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
