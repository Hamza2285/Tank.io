using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns enemies in waves and tracks how many are still alive.
/// Reports wave start/completion to GameManager. Knows nothing about UI or enemy behaviour.
/// </summary>
public class WaveManager : MonoBehaviour
{
    [Header("Spawning")]
    [Tooltip("Enemy prefab to spawn. Assign once the enemy exists.")]
    [SerializeField] private GameObject enemyPrefab;

    [Tooltip("Positions enemies spawn at, picked at random.")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Wave Settings")]
    [Tooltip("Enemies spawned in wave 1.")]
    [SerializeField] private int startingEnemyCount = 5;

    [Tooltip("Extra enemies added to every following wave.")]
    [SerializeField] private int enemiesAddedPerWave = 2;

    [Tooltip("Seconds between two spawns inside the same wave.")]
    [SerializeField] private float spawnInterval = 0.5f;

    [Tooltip("Seconds to wait after a wave is cleared before the next one starts.")]
    [SerializeField] private float delayBetweenWaves = 3f;

    [Tooltip("Start wave 1 automatically on Start().")]
    [SerializeField] private bool autoStart = true;

    /// <summary>Wave currently running. 0 before the first wave.</summary>
    public int CurrentWave { get; private set; }

    /// <summary>Enemies of the current wave that are still alive.</summary>
    public int AliveEnemies { get; private set; }

    /// <summary>True while enemies of the current wave are still being spawned.</summary>
    public bool IsSpawning { get; private set; }

    private Coroutine spawnRoutine;

    private void Start()
    {
        if (autoStart) StartNextWave();
    }

    private void OnDisable()
    {
        // Stop spawning cleanly if the manager is turned off mid-wave.
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
            IsSpawning = false;
        }
    }

    /// <summary>Increments the wave counter and spawns that wave's enemies.</summary>
    public void StartNextWave()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (IsSpawning) return;

        CurrentWave++;
        int enemyCount = startingEnemyCount + (CurrentWave - 1) * enemiesAddedPerWave;

        if (GameManager.Instance != null)
            GameManager.Instance.StartWave(CurrentWave);

        spawnRoutine = StartCoroutine(SpawnWave(enemyCount));
    }

    /// <summary>Counts an enemy as alive. Called by the spawner, or manually for hand-placed enemies.</summary>
    public void RegisterEnemy()
    {
        AliveEnemies++;
    }

    /// <summary>Call from the enemy when it dies. Awards score and ends the wave when the last one falls.</summary>
    public void NotifyEnemyKilled()
    {
        AliveEnemies = Mathf.Max(0, AliveEnemies - 1);

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterEnemyKilled();

        if (!IsSpawning && AliveEnemies == 0)
            CompleteWave();
    }

    private IEnumerator SpawnWave(int enemyCount)
    {
        IsSpawning = true;

        WaitForSeconds wait = new WaitForSeconds(spawnInterval);
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();
            yield return wait;
        }

        IsSpawning = false;
        spawnRoutine = null;

        // Everything spawned may already be dead by now.
        if (AliveEnemies == 0) CompleteWave();
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning($"{name}: WaveManager has no enemy prefab assigned.", this);
            return;
        }

        Transform point = GetSpawnPoint();
        Vector3 position = point != null ? point.position : transform.position;

        Instantiate(enemyPrefab, position, Quaternion.identity);
        RegisterEnemy();
    }

    private Transform GetSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return null;
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    private void CompleteWave()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.CompleteWave(CurrentWave);

        StartCoroutine(StartNextWaveAfterDelay());
    }

    private IEnumerator StartNextWaveAfterDelay()
    {
        yield return new WaitForSeconds(delayBetweenWaves);
        StartNextWave();
    }
}
