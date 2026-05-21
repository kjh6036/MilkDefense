using UnityEngine;

public enum MercenaryType { Melee, Ranged }

[CreateAssetMenu(fileName = "MercenaryStatData", menuName = "Game/Mercenary/StatData")]
public class MercenaryStatData : ScriptableObject
{
    [Header("Info")]
    public string mercenaryName;
    public MercenaryType mercenaryType;
    public Grade grade;
    public GameObject prefab;

    [Header("Combat Stats")]
    public float attackDamage;
    public float attackRange;
    public float attackCooldown;

    [Header("Purchase")]
    public int purchaseCost;

    [Header("Skills")]
    public SkillData[] skills;
}