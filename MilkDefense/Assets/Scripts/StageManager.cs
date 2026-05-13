using TMPro;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private TextMeshProUGUI enemyAmountText;

    private const int EnemyLimit = 80;
    private int _enemyKillCount = 0;

    private void Start()
    {
        enemySpawner.OnEnemyDied += HandleEnemyDied;
        enemySpawner.StartSpawning();
    }

    private void HandleEnemyDied()
    {
        _enemyKillCount++;
        enemyAmountText.text = $"{_enemyKillCount} / {EnemyLimit}";

        if (_enemyKillCount >= EnemyLimit)
            GameOver();
    }

    private void GameOver()
    {
        enemySpawner.StopSpawning();
        Debug.Log("[StageManager] Game Over");
    }

    private void OnDestroy()
    {
        enemySpawner.OnEnemyDied -= HandleEnemyDied;
    }
}