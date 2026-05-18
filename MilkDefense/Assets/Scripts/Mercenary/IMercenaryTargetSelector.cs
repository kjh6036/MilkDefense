using System.Collections.Generic;
using UnityEngine;

public interface IMercenaryTargetSelector
{
    EnemyInstance Select(List<EnemyInstance> candidates, Transform origin);
}

public class NearestTargetSelector : IMercenaryTargetSelector
{
    public EnemyInstance Select(List<EnemyInstance> candidates, Transform origin)
    {
        EnemyInstance nearest = null;
        float minDist = float.MaxValue;

        foreach (var enemy in candidates)
        {
            if (enemy == null || !enemy.gameObject.activeSelf) continue;

            float dist = (enemy.transform.position - origin.position).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }

        return nearest;
    }
}
