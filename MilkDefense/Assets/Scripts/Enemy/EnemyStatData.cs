using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStatData", menuName = "Game/Enemy/StatData")]
public class EnemyStatData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName;
    public EnemyType enemyType;
    public GameObject prefab;

    [Header("Combat Stats")]
    public float maxHp;
    public float moveSpeed;

    [Header("Reward")]
    public int reward;
}