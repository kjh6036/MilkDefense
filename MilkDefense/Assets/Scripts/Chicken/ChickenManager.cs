using System.Collections.Generic;
using UnityEngine;

public class ChickenManager : MonoBehaviour
{
    public static ChickenManager Instance { get; private set; }

    [SerializeField] private ChickenData[] chickenDataList;
    [SerializeField] private Transform[] chickenSlots;
    [SerializeField] private int preloadCountPerType = 3;

    private const int ChickenPurchaseCost = 50;
    private const int LimitUpgradeCost = 200;
    private const int MaxChickenLimit = 10;

    private Dictionary<ChickenData, ObjectPool<Chicken>> _pools;
    private List<Chicken> _allChickens = new List<Chicken>();
    private List<Chicken> _activeChickens = new List<Chicken>();
    private int _chickenLimit = 5;

    public int ChickenCount => _activeChickens.Count;
    public int ChickenLimit => _chickenLimit;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _pools = new Dictionary<ChickenData, ObjectPool<Chicken>>();

        foreach (var data in chickenDataList)
        {
            var pool = new ObjectPool<Chicken>();
            _pools[data] = pool;

            for (int i = 0; i < preloadCountPerType; i++)
                CreateAndRegister(data, pool);
        }
    }

    [Header("Test")]
    [SerializeField] private int testSpawnCount = 3;

    private void Start()
    {
        for (int i = 0; i < testSpawnCount; i++)
            SpawnChicken(chickenDataList[0]);
    }

    // 특정 종류의 닭 구매
    public void PurchaseChickenButton(ChickenData data)
    {
        if (_activeChickens.Count >= _chickenLimit)
        {
            Debug.Log("[ChickenManager] 닭 슬롯이 가득 찼습니다.");
            return;
        }

        int cost = _activeChickens.Count * ChickenPurchaseCost;

        if (!ResourceManager.Instance.TrySpend(cost))
        {
            Debug.Log("[ChickenManager] 돈이 부족합니다.");
            return;
        }

        SpawnChicken(data);
    }

    public void PurchaseChickenLimitButton()
    {
        if (_chickenLimit >= MaxChickenLimit)
        {
            Debug.Log("[ChickenManager] 이미 최대 한도입니다.");
            return;
        }

        int cost = _chickenLimit * LimitUpgradeCost;

        if (!ResourceManager.Instance.TrySpend(cost))
        {
            Debug.Log("[ChickenManager] 돈이 부족합니다.");
            return;
        }

        _chickenLimit++;
        Debug.Log($"[ChickenManager] 한도 업그레이드 → {_chickenLimit}칸");
    }

    private void SpawnChicken(ChickenData data)
    {
        var pool = _pools[data];

        Chicken chicken = pool.canRecycle
            ? pool.GetRecycledObject()
            : CreateAndRegister(data, pool);

        int index = _activeChickens.Count;
        Transform slot = chickenSlots[index];

        chicken.transform.position = slot.position;
        chicken.gameObject.SetActive(true);
        chicken.Initialize(data);

        _activeChickens.Add(chicken);
        Debug.Log($"[ChickenManager] {data.infoData.entityName} 생성 ({_activeChickens.Count}/{_chickenLimit})");
    }

    private Chicken CreateAndRegister(ChickenData data, ObjectPool<Chicken> pool)
    {
        GameObject go = Instantiate(data.prefab);
        Chicken chicken = go.GetComponent<Chicken>();

        go.SetActive(false);
        pool.RegisterRecyclableObject(chicken);
        _allChickens.Add(chicken);
        return chicken;
    }
}