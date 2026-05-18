using UnityEngine;

public class RangedMercenary : MercenaryBase
{
    protected override void Attack(EnemyInstance target)
    {
        // 즉발 공격 — 추후 투사체 방식으로 교체 가능
        target.TakeDamage(_data.attackDamage);
    }
}
