using System.Collections.Generic;
using UnityEngine;

public class EnemyRegistry : MonoBehaviour
{
    private readonly List<EnemyInstance> _activeEnemies = new List<EnemyInstance>();
    public IReadOnlyList<EnemyInstance> ActiveEnemies => _activeEnemies;

    private void Awake()
    {
        DependencyInjector.Constructor(this);
    }

    private void OnDestroy()
    {
        DependencyInjector.Demolisher(this);
    }

    public void Register(EnemyInstance enemy)
    {
        _activeEnemies.Add(enemy);
    }

    public void Unregister(EnemyInstance enemy)
    {
        _activeEnemies.Remove(enemy);
    }
}