using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyStatData[] enemyDataList;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int preloadCountPerType = 5;

    public event Action OnEnemyDied;

    private Dictionary<EnemyStatData, ObjectPool<EnemyInstance>> _pools;
    private List<EnemyInstance> _allEnemies = new List<EnemyInstance>();
    private Coroutine _spawnRoutine;

    private void Awake()
    {
        _pools = new Dictionary<EnemyStatData, ObjectPool<EnemyInstance>>();

        foreach (var data in enemyDataList)
        {
            var pool = new ObjectPool<EnemyInstance>();
            _pools[data] = pool;

            for (int i = 0; i < preloadCountPerType; i++)
                CreateAndRegister(data, pool);
        }
    }

    public void StartSpawning()
    {
        _spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        if (_spawnRoutine != null)
            StopCoroutine(_spawnRoutine);
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyDataList.Length == 0 || spawnPoints.Length == 0) return;

        var data = enemyDataList[UnityEngine.Random.Range(0, enemyDataList.Length)];
        var spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        var pool = _pools[data];

        EnemyInstance enemy = pool.canRecycle
            ? pool.GetRecycledObject()
            : CreateAndRegister(data, pool);

        enemy.transform.position = spawnPoint.position;
        enemy.gameObject.SetActive(true);
        enemy.Initialize(data);
    }

    private EnemyInstance CreateAndRegister(EnemyStatData data, ObjectPool<EnemyInstance> pool)
    {
        GameObject go = Instantiate(data.prefab);
        var enemy = go.GetComponent<EnemyInstance>();

        enemy.OnDied += HandleEnemyDied;
        go.SetActive(false);

        pool.RegisterRecyclableObject(enemy);
        _allEnemies.Add(enemy);
        return enemy;
    }

    private void HandleEnemyDied()
    {
        OnEnemyDied?.Invoke();
    }

    private void OnDestroy()
    {
        foreach (var enemy in _allEnemies)
            if (enemy != null)
                enemy.OnDied -= HandleEnemyDied;
    }
}