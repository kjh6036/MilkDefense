using UnityEngine;

public class MeleeMercenary : MercenaryBase
{
    protected override void Attack(EnemyInstance target)
    {
        target.TakeDamage(_data.attackDamage);
    }
}
