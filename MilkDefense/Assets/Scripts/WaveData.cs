using System;
using UnityEngine;

[Serializable]
public class WaveEnemyEntry
{
    public EnemyStatData enemyData;
    public int count;
    [Tooltip("스폰 간격 (초)")]
    public float spawnInterval;
}

[CreateAssetMenu(fileName = "WaveData", menuName = "Game/Stage/WaveData")]
public class WaveData : ScriptableObject
{
    [Tooltip("이 데이터를 적용할 웨이브 번호 (1-based)")]
    public int waveNumber;

    public WaveEnemyEntry[] entries;
}
