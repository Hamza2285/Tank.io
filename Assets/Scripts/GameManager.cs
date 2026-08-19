using System;
using UnityEngine;

/// <summary>
/// Central game state: score, current wave, game over.
/// Broadcasts changes through events - it never touches UI directly.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Score Values")]
    [Tooltip("Score awarded each time an enemy is hit.")]
    [SerializeField] private int scorePerHit = 1;

    [Tooltip("Score awarded each time an enemy is killed.")]
    [SerializeField] private int scorePerKill = 10;

    public int Score { get; private set; }
    public int CurrentWave { get; private set; }
    public bool IsGameOver { get; private set; }

    public event Action<int> OnScoreChanged;
    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCompleted;
    public event Action OnGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void RegisterEnemyHit()
    {
        if (IsGameOver) return;
        AddScore(scorePerHit);
    }

    public void RegisterEnemyKilled()
    {
        if (IsGameOver) return;
        AddScore(scorePerKill);
    }

    public void StartWave(int waveNumber)
    {
        if (IsGameOver) return;

        CurrentWave = waveNumber;
        OnWaveStarted?.Invoke(CurrentWave);
    }

    public void CompleteWave(int waveNumber)
    {
        if (IsGameOver) return;
        OnWaveCompleted?.Invoke(waveNumber);
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        OnGameOver?.Invoke();
    }

    /// <summary>Resets score, wave and game over state. Call before restarting a run.</summary>
    public void ResetGame()
    {
        IsGameOver = false;
        CurrentWave = 0;
        Score = 0;
        OnScoreChanged?.Invoke(Score);
    }

    private void AddScore(int amount)
    {
        if (amount == 0) return;

        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }
}
