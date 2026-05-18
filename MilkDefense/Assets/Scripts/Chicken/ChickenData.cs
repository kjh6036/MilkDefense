using System;
using UnityEngine;

[Serializable]
public class ChickenLevelStat
{
    public float maxHp;
    public int eggCount;
    public float hpRegenAmount;   // 회복량
    public float hpRegenInterval; // 회복 주기 (초)

    [Tooltip("등급별 가중치 (index 0 = Common, 1 = Uncommon, 2 = Rare, 3 = Epic)\n합이 100일 필요 없음, 상대적 비율로 계산됨")]
    public float[] eggGradeWeights = { 70f, 20f, 8f, 2f };
}

[CreateAssetMenu(fileName = "ChickenData", menuName = "Game/Entity/ChickenData")]
public class ChickenData : ScriptableObject
{
    [Header("Info")]
    public EntityInfoData infoData;
    public GameObject prefab;
    public Grade grade;

    [Header("Level Stats (index 0 = Lv1)")]
    public ChickenLevelStat[] levelStats;
}