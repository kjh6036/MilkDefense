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

    [Tooltip("보스 처치 제한 시간 (초)")]
    public float bossTimeLimit = 60f;

    [Header("일반 웨이브 공식")]
    [Tooltip("1웨이브 기준 스폰 수")]
    public int baseEnemyCount = 10;

    [Tooltip("웨이브당 적 수 증가량")]
    public int enemyCountPerWave = 3;

    [Tooltip("스폰 간격 (초)")]
    public float baseSpawnInterval = 1f;


    [Tooltip("일반 웨이브에서 사용할 적 목록")]
    public EnemyStatData[] enemyPool;

    [Header("보스")]
    [Tooltip("보스 웨이브별 보스 데이터 (index 0 = 1보스, 1 = 2보스...)")]
    public EnemyStatData[] bossDataPerStage;

    [Header("웨이브 클리어 보너스")]
    [Tooltip("매 웨이브 클리어 시 기본 보너스")]
    public int waveBaseBonus = 10;
    [Tooltip("웨이브마다 보너스 증가량")]
    public int waveBonusPerWave = 2;
    [Tooltip("보스 웨이브 클리어 추가 보너스")]
    public int bossClearBonus = 50;

    public int GetWaveBonus(int wave) => waveBaseBonus + waveBonusPerWave * (wave - 1);

    [Header("적 HP 스케일")]
    [Tooltip("1웨이브 기준 HP 배율")]
    public float baseHpMultiplier = 1f;
    [Tooltip("웨이브마다 HP 배율 증가량")]
    public float hpMultiplierPerWave = 0.1f;

    public int GetEnemyCount(int wave)
        => baseEnemyCount + enemyCountPerWave * (wave - 1);

    public float GetSpawnInterval(int wave) => baseSpawnInterval;

    public float GetHpMultiplier(int wave)
        => baseHpMultiplier + hpMultiplierPerWave * (wave - 1);

    public bool IsBossWave(int wave)
        => wave % bossWaveInterval == 0;

    public bool IsLastWave(int wave)
        => wave == totalWaves;
}