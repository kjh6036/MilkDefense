using System.Collections;
using TMPro;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField] private StageConfig _config;
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private WaveData[] _designedWaves;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _waveText;
    [SerializeField] private TextMeshProUGUI _enemyCountText;
    [SerializeField] private TextMeshProUGUI _cooldownText;

    private int _currentWave = 0;
    private bool _isGameOver = false;
    private Coroutine _waveRoutine;

    private void Start()
    {
        _enemySpawner.OnEnemyDied += HandleEnemyDied;
        _enemySpawner.OnWaveSpawnComplete += HandleWaveSpawnComplete;
        StartNextWave();
    }

    private void StartNextWave()
    {
        if (_isGameOver) return;

        _currentWave++;

        if (_currentWave > _config.totalWaves)
        {
            StageClear();
            return;
        }

        UpdateWaveUI();
        _waveRoutine = StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        WaveData designedWave = GetDesignedWave(_currentWave);

        if (designedWave != null)
            _enemySpawner.StartWave(designedWave.entries);
        else
            _enemySpawner.StartWave(
                _config.GetEnemyCount(_currentWave),
                _config.GetSpawnInterval(_currentWave),
                _config.enemyPool
            );

        yield return StartCoroutine(CooldownRoutine());

        DependencyInjector.Get<EggPriceManager>().RollPrices();
        StartNextWave();
    }

    private IEnumerator CooldownRoutine()
    {
        float remaining = _config.waveCooldown;

        while (remaining > 0f)
        {
            _cooldownText.text = $"다음 웨이브까지 {remaining:F0}초";
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }

        _cooldownText.text = string.Empty;
    }

    private void HandleWaveSpawnComplete()
    {
        Debug.Log($"[StageManager] Wave {_currentWave} 스폰 완료");
    }

    private void HandleEnemyDied()
    {
        UpdateEnemyCountUI();
    }

    private void CheckGameOver()
    {
        if (_isGameOver) return;
        if (_enemySpawner.ActiveEnemyCount >= _config.gameOverEnemyLimit)
            GameOver();
    }

    public void OnEnemySpawned()
    {
        UpdateEnemyCountUI();
        CheckGameOver();
    }

    private void GameOver()
    {
        _isGameOver = true;
        _enemySpawner.StopSpawning();
        if (_waveRoutine != null) StopCoroutine(_waveRoutine);
        Debug.Log("[StageManager] Game Over");
    }

    private void StageClear()
    {
        Debug.Log("[StageManager] Stage Clear");
    }

    private WaveData GetDesignedWave(int waveNumber)
    {
        foreach (var wave in _designedWaves)
            if (wave.waveNumber == waveNumber) return wave;
        return null;
    }

    private void UpdateWaveUI()
    {
        _waveText.text = $"Wave {_currentWave} / {_config.totalWaves}";
    }

    private void UpdateEnemyCountUI()
    {
        _enemyCountText.text = $"적 {_enemySpawner.ActiveEnemyCount} / {_config.gameOverEnemyLimit}";
    }

    private void OnDestroy()
    {
        _enemySpawner.OnEnemyDied -= HandleEnemyDied;
        _enemySpawner.OnWaveSpawnComplete -= HandleWaveSpawnComplete;
    }
}