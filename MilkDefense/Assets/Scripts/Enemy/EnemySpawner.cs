using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyStatData[] enemyDataList;
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private int preloadCountPerType = 5;

    public event Action OnEnemyDied;
    public event Action OnWaveSpawnComplete;

    public int ActiveEnemyCount => DependencyInjector.Get<EnemyRegistry>().ActiveEnemies.Count;

    private Dictionary<EnemyStatData, ObjectPool<EnemyInstance>> _pools;
    private readonly List<EnemyInstance> _allEnemies = new List<EnemyInstance>();
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

    public void StartWave(int totalCount, float spawnInterval, EnemyStatData[] pool)
    {
        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
        _spawnRoutine = StartCoroutine(SpawnRoutine(totalCount, spawnInterval, pool));
    }

    public void StartWave(WaveEnemyEntry[] entries)
    {
        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
        _spawnRoutine = StartCoroutine(SpawnRoutine(entries));
    }

    public void StopSpawning()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }
    }

    private IEnumerator SpawnRoutine(int totalCount, float spawnInterval, EnemyStatData[] pool)
    {
        for (int i = 0; i < totalCount; i++)
        {
            var data = pool[UnityEngine.Random.Range(0, pool.Length)];
            SpawnEnemy(data);
            yield return new WaitForSeconds(spawnInterval);
        }

        OnWaveSpawnComplete?.Invoke();
    }

    private IEnumerator SpawnRoutine(WaveEnemyEntry[] entries)
    {
        foreach (var entry in entries)
        {
            for (int i = 0; i < entry.count; i++)
            {
                SpawnEnemy(entry.enemyData);
                yield return new WaitForSeconds(entry.spawnInterval);
            }
        }

        OnWaveSpawnComplete?.Invoke();
    }

    private void SpawnEnemy(EnemyStatData data)
    {
        if (!_pools.TryGetValue(data, out var pool))
        {
            Debug.LogWarning($"[EnemySpawner] Ç® ¾øÀ½: {data.enemyName}");
            return;
        }

        EnemyInstance enemy = pool.canRecycle
            ? pool.GetRecycledObject()
            : CreateAndRegister(data, pool);

        enemy.gameObject.SetActive(true);
        enemy.Initialize(data, waypoints);
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.15f);

            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
        }
    }
#endif

    private void OnDestroy()
    {
        foreach (var enemy in _allEnemies)
            if (enemy != null)
                enemy.OnDied -= HandleEnemyDied;
    }
}