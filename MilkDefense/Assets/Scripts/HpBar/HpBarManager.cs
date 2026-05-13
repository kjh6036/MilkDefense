using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HpBarPoolEntry
{
    public EnemyType type;
    public HpBarInstance prefab;
    public int preloadCount;
}

public class HpBarManager : MonoBehaviour
{
    public static HpBarManager Instance { get; private set; }

    [SerializeField] private Canvas _canvas;
    [SerializeField] private HpBarPoolEntry[] _hpBarEntries;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 1.5f, 0f);

    private Camera _mainCamera;
    private Dictionary<EnemyType, ObjectPool<HpBarInstance>> _pools;
    private Dictionary<EnemyType, HpBarInstance> _prefabCache;              // [Fix] O(n) 선형탐색 제거
    private Dictionary<Transform, HpBarInstance> _active;
    private readonly List<Transform> _pendingRemove = new List<Transform>(); // [Fix] 루프 중 Remove 방지

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _mainCamera = Camera.main;
        _pools = new Dictionary<EnemyType, ObjectPool<HpBarInstance>>(_hpBarEntries.Length);
        _prefabCache = new Dictionary<EnemyType, HpBarInstance>(_hpBarEntries.Length);
        _active = new Dictionary<Transform, HpBarInstance>();

        foreach (var entry in _hpBarEntries)
        {
            var pool = new ObjectPool<HpBarInstance>();
            _pools[entry.type] = pool;
            _prefabCache[entry.type] = entry.prefab;

            for (int i = 0; i < entry.preloadCount; i++)
                CreateAndRegister(entry.type, entry.prefab, pool);
        }
    }

    private void LateUpdate()
    {
        foreach (var pair in _active)
        {
            Transform target = pair.Key;
            HpBarInstance hpBar = pair.Value;

            // [Fix] null target은 루프 밖에서 일괄 제거
            if (target == null)
            {
                _pendingRemove.Add(target);
                hpBar.gameObject.SetActive(false);
                continue;
            }

            // [Fix] Viewport 기준으로 먼저 culling → 밖이면 WorldToScreenPoint 자체를 스킵
            Vector3 viewportPos = _mainCamera.WorldToViewportPoint(target.position + _offset);
            bool isVisible = viewportPos.z > 0f
                          && viewportPos.x is > 0f and < 1f
                          && viewportPos.y is > 0f and < 1f;

            if (!isVisible)
            {
                hpBar.gameObject.SetActive(false);
                continue;
            }

            hpBar.gameObject.SetActive(true);
            hpBar.transform.position = _mainCamera.WorldToScreenPoint(target.position + _offset);
        }

        // [Fix] 루프 종료 후 일괄 제거
        if (_pendingRemove.Count > 0)
        {
            foreach (var t in _pendingRemove)
                _active.Remove(t);
            _pendingRemove.Clear();
        }
    }

    public HpBarInstance Get(Transform target, EnemyType type = EnemyType.Normal)
    {
        if (!_pools.TryGetValue(type, out var pool))
        {
            Debug.LogError($"[HpBarManager] 등록되지 않은 EnemyType: {type}");
            return null;
        }

        HpBarInstance hpBar = pool.canRecycle
            ? pool.GetRecycledObject()
            : CreateAndRegister(type, _prefabCache[type], pool); // [Fix] prefabCache 사용

        hpBar.gameObject.SetActive(true);
        _active[target] = hpBar;
        return hpBar;
    }

    public void Release(Transform target)
    {
        if (!_active.TryGetValue(target, out var hpBar)) return;
        hpBar.gameObject.SetActive(false);
        _active.Remove(target);
    }

    private HpBarInstance CreateAndRegister(EnemyType type, HpBarInstance prefab, ObjectPool<HpBarInstance> pool)
    {
        HpBarInstance hpBar = Instantiate(prefab, _canvas.transform);
        hpBar.enemyType = type;
        hpBar.gameObject.SetActive(false);
        pool.RegisterRecyclableObject(hpBar);
        return hpBar;
    }
}