using System.Collections;
using TMPro;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField] private StageConfig _config;
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private WaveData[] _designedWaves;

    [Header("UI")]
    [SerializeField] private BossTimerUI _bossTimerUI;
    [SerializeField] private TextMeshProUGUI _waveText;
    [SerializeField] private TextMeshProUGUI _enemyCountText;
    [SerializeField] private TextMeshProUGUI _cooldownText;
    [SerializeField] private TextMeshProUGUI _bossTimerText;

    private int _currentWave = 0;
    private bool _isGameOver = false;
    private bool _isLastBossDefeated = false;
    private bool _isSpawnComplete = false;
    private bool _isBossDefeated = false;
    private Coroutine _waveRoutine;
    private Coroutine _bossTimerRoutine;

    private void Start()
    {
        _bossTimerUI?.Hide();
        _enemySpawner.OnEnemyDied += HandleEnemyDied;
        _enemySpawner.OnEnemySpawned += HandleEnemySpawned;
        _enemySpawner.OnWaveSpawnComplete += HandleWaveSpawnComplete;
        _enemySpawner.OnBossSpawned += HandleBossSpawned;
        _enemySpawner.OnBossDied += HandleBossDied;
        StartNextWave();
    }

    private void StartNextWave()
    {
        if (_isGameOver) return;

        _currentWave++;

        if (_currentWave > _config.totalWaves)
        {
            if (!_config.IsBossWave(_config.totalWaves))
                Victory();
            return;
        }

        UpdateWaveUI();
        _waveRoutine = StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        bool isBossWave = _config.IsBossWave(_currentWave);

        // ─── 스폰 ─────────────────────────────────────────
        _isSpawnComplete = false;
        _isBossDefeated = false;

        WaveData designedWave = GetDesignedWave(_currentWave);
        float hpMultiplier = _config.GetHpMultiplier(_currentWave);

        if (designedWave != null)
        {
            _enemySpawner.StartWave(designedWave.entries, hpMultiplier);
        }
        else if (isBossWave)
        {
            int bossIndex = (_currentWave / _config.bossWaveInterval) - 1;
            if (bossIndex >= 0 && bossIndex < _config.bossDataPerStage.Length)
                _enemySpawner.StartWave(1, 0f, new[] { _config.bossDataPerStage[bossIndex] }, hpMultiplier);
            else
                Debug.LogError($"[StageManager] 보스 데이터 없음: index {bossIndex}");
        }
        else
        {
            _enemySpawner.StartWave(
                _config.GetEnemyCount(_currentWave),
                _config.GetSpawnInterval(_currentWave),
                _config.enemyPool,
                hpMultiplier
            );
        }

        // ─── 대기 ─────────────────────────────────────────
        if (isBossWave)
        {
            // 보스 처치까지 대기
            yield return new WaitUntil(() => _isBossDefeated || _isGameOver);
        }
        else
        {
            // 스폰 완료까지 대기
            yield return new WaitUntil(() => _isSpawnComplete || _isGameOver);
        }

        if (_isGameOver) yield break;

        // ─── 클리어 보너스 ────────────────────────────────
        int bonus = _config.GetWaveBonus(_currentWave);
        if (isBossWave) bonus += _config.bossClearBonus;
        DependencyInjector.Get<ResourceManager>().Earn(bonus);
        Debug.Log($"[StageManager] Wave {_currentWave} 클리어 보너스 +{bonus}G");

        // ─── 쿨다운 ───────────────────────────────────────
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

    // ─── 보스 타이머 ──────────────────────────────────────

    private IEnumerator BossTimerRoutine()
    {
        float remaining = _config.bossTimeLimit;

        yield return null;
        EnemyInstance boss = FindBoss();
        if (boss != null)
            _bossTimerUI?.Show(boss.transform, remaining);

        while (remaining > 0f)
        {
            if (_isBossDefeated) yield break;
            if (_bossTimerText != null) _bossTimerText.text = $"보스 처치까지 {remaining:F0}초";
            _bossTimerUI?.UpdateText(remaining);
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }

        if (_bossTimerText != null) _bossTimerText.text = string.Empty;
        _bossTimerUI?.Hide();

        if (!_isBossDefeated)
            GameOver();
    }

    private void HandleBossSpawned()
    {
        if (_bossTimerRoutine != null) StopCoroutine(_bossTimerRoutine);
        _bossTimerRoutine = StartCoroutine(BossTimerRoutine());
    }

    private void HandleBossDied()
    {
        _isBossDefeated = true;

        if (_bossTimerRoutine != null)
        {
            StopCoroutine(_bossTimerRoutine);
            _bossTimerRoutine = null;
        }
        if (_bossTimerText != null) _bossTimerText.text = string.Empty;
        _bossTimerUI?.Hide();

        if (_config.IsLastWave(_currentWave))
        {
            _isLastBossDefeated = true;
            Victory();
        }
    }

    private EnemyInstance FindBoss()
    {
        var registry = DependencyInjector.Get<EnemyRegistry>();
        if (registry == null) return null;
        foreach (var enemy in registry.ActiveEnemies)
            if (enemy.StatData.enemyType == EnemyType.Boss)
                return enemy;
        return null;
    }

    // ─── 적 처치 / 게임오버 체크 ──────────────────────────

    private void HandleWaveSpawnComplete()
    {
        _isSpawnComplete = true;
        Debug.Log($"[StageManager] Wave {_currentWave} 스폰 완료");
    }

    private void HandleEnemySpawned()
    {
        UpdateEnemyCountUI();
        CheckGameOver();
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

    // ─── 승리 / 패배 ──────────────────────────────────────

    private void GameOver()
    {
        if (_isGameOver) return;
        _isGameOver = true;

        _enemySpawner.StopSpawning();
        if (_waveRoutine != null) StopCoroutine(_waveRoutine);
        if (_bossTimerRoutine != null) StopCoroutine(_bossTimerRoutine);

        DependencyInjector.Get<GameResultUI>().ShowDefeat();
        Debug.Log("[StageManager] Game Over");
    }

    private void Victory()
    {
        _enemySpawner.StopSpawning();
        if (_waveRoutine != null) StopCoroutine(_waveRoutine);

        DependencyInjector.Get<GameResultUI>().ShowVictory();
        Debug.Log("[StageManager] Stage Clear");
    }

    // ─── 유틸 ─────────────────────────────────────────────

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
        _enemySpawner.OnEnemySpawned -= HandleEnemySpawned;
        _enemySpawner.OnWaveSpawnComplete -= HandleWaveSpawnComplete;
        _enemySpawner.OnBossSpawned -= HandleBossSpawned;
        _enemySpawner.OnBossDied -= HandleBossDied;
    }
}