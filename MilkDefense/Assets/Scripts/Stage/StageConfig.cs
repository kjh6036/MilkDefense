using UnityEngine;

[CreateAssetMenu(fileName = "StageConfig", menuName = "Game/Stage/StageConfig")]
public class StageConfig : ScriptableObject
{
    [Header("웨이브 기본 설정")]
    [Tooltip("총 웨이브 수")]
    public int totalWaves = 30;

    [Tooltip("웨이브 사이 대기 시간 (초)")]
    public float waveCooldown = 10f;

    [Tooltip("게임오버 임계값 (화면에 살아있는 적 수)")]
    public int gameOverEnemyLimit = 100;

    [Header("보스 웨이브")]
    [Tooltip("몇 웨이브마다 보스 웨이브인지")]
    public int bossWaveInterval = 10;

    [Header("일반 웨이브 공식")]
    [Tooltip("1웨이브 기준 스폰 수")]
    public int baseEnemyCount = 10;

    [Tooltip("웨이브당 적 수 증가량")]
    public int enemyCountPerWave = 3;

    [Tooltip("스폰 간격 (초)")]
    public float baseSpawnInterval = 1f;

    [Tooltip("웨이브마다 스폰 간격 감소량 (최소 0.2초)")]
    public float spawnIntervalDecreasePerWave = 0.02f;

    [Tooltip("일반 웨이브에서 사용할 적 목록")]
    public EnemyStatData[] enemyPool;

    public int GetEnemyCount(int wave)
        => baseEnemyCount + enemyCountPerWave * (wave - 1);

    public float GetSpawnInterval(int wave)
        => Mathf.Max(0.2f, baseSpawnInterval - spawnIntervalDecreasePerWave * (wave - 1));
}
