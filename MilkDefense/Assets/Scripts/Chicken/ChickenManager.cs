using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ChickenGachaWeight
{
    public Grade grade;
    [Tooltip("뽑기 가중치")]
    public float weight;
}

public class ChickenManager : MonoBehaviour
{
    [SerializeField] private ChickenData[] chickenDataList;
    [SerializeField] private Transform[] chickenSlots;
    [SerializeField] private int preloadCountPerType = 3;

    [Header("시작 시 배치 닭 수")]
    [SerializeField] private int _startChickenCount = 3;

    [Header("가챠")]
    [SerializeField] private int _gachaCost = 300;
    [SerializeField] private Button _gachaButton;
    [SerializeField] private Button _limitUpgradeButton;

    [Header("등급별 뽑기 가중치")]
    [SerializeField]
    private ChickenGachaWeight[] _gachaWeights = new ChickenGachaWeight[]
    {
        new ChickenGachaWeight { grade = Grade.Common,   weight = 80f },
        new ChickenGachaWeight { grade = Grade.Uncommon, weight = 15f },
        new ChickenGachaWeight { grade = Grade.Rare,     weight = 5f  },
    };

    [SerializeField] private int _limitUpgradeCost = 500;
    private const int MaxChickenLimit = 10;

    private Dictionary<ChickenData, ObjectPool<Chicken>> _pools;
    private Dictionary<Grade, List<ChickenData>> _dataByGrade;
    private readonly List<Chicken> _allChickens = new List<Chicken>();
    private readonly List<Chicken> _activeChickens = new List<Chicken>();
    private int _chickenLimit = 5;
    private TextMeshProUGUI _gachaButtonText;
    private TextMeshProUGUI _limitUpgradeButtonText;

    public int ChickenCount => _activeChickens.Count;
    public int ChickenLimit => _chickenLimit;
    public int GachaCost => _gachaCost;

    private void Awake()
    {
        DependencyInjector.Constructor(this);

        _pools = new Dictionary<ChickenData, ObjectPool<Chicken>>();
        _dataByGrade = new Dictionary<Grade, List<ChickenData>>();

        foreach (var data in chickenDataList)
        {
            var pool = new ObjectPool<Chicken>();
            _pools[data] = pool;

            for (int i = 0; i < preloadCountPerType; i++)
                CreateAndRegister(data, pool);

            if (!_dataByGrade.ContainsKey(data.grade))
                _dataByGrade[data.grade] = new List<ChickenData>();
            _dataByGrade[data.grade].Add(data);
        }

        if (_gachaButton != null)
        {
            _gachaButtonText = _gachaButton.GetComponentInChildren<TextMeshProUGUI>();
            UpdateGachaButtonUI();
        }

        if (_limitUpgradeButton != null)
        {
            _limitUpgradeButtonText = _limitUpgradeButton.GetComponentInChildren<TextMeshProUGUI>();
            UpdateLimitUpgradeButtonUI();
        }
    }

    private void OnDestroy()
    {
        DependencyInjector.Demolisher(this);
    }

    private void Start()
    {
        if (!_dataByGrade.TryGetValue(Grade.Common, out var commonList) || commonList.Count == 0)
        {
            Debug.LogError("[ChickenManager] Common 등급 닭이 없습니다.");
            return;
        }

        for (int i = 0; i < _startChickenCount; i++)
            SpawnChicken(commonList[Random.Range(0, commonList.Count)]);
    }

    // ─── 가챠 ─────────────────────────────────────────────

    public void TryGacha()
    {
        if (_activeChickens.Count >= _chickenLimit)
        {
            Debug.Log("[ChickenManager] 닭 슬롯이 가득 찼습니다.");
            return;
        }

        var resourceManager = DependencyInjector.Get<ResourceManager>();
        if (!resourceManager.TrySpend(_gachaCost))
        {
            Debug.Log("[ChickenManager] 돈이 부족합니다.");
            return;
        }

        ChickenData data = RollChicken();
        if (data == null)
        {
            Debug.LogError("[ChickenManager] 뽑기 결과가 없습니다.");
            resourceManager.Earn(_gachaCost);
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

        int cost = (_chickenLimit - 4) * _limitUpgradeCost;

        if (!DependencyInjector.Get<ResourceManager>().TrySpend(cost))
        {
            Debug.Log("[ChickenManager] 돈이 부족합니다.");
            return;
        }

        _chickenLimit++;
        UpdateLimitUpgradeButtonUI();
        Debug.Log($"[ChickenManager] 한도 업그레이드 -> {_chickenLimit}칸");
    }

    private ChickenData RollChicken(int maxRetry = 10)
    {
        for (int i = 0; i < maxRetry; i++)
        {
            Grade grade = RollGrade();

            if (!_dataByGrade.TryGetValue(grade, out var candidates) || candidates.Count == 0)
            {
                Debug.Log($"[ChickenManager] {grade} 등급 닭 없음 - 재추첨 ({i + 1}/{maxRetry})");
                continue;
            }

            return candidates[Random.Range(0, candidates.Count)];
        }

        // 재추첨 실패 시 존재하는 등급 중 랜덤 반환
        Debug.LogWarning("[ChickenManager] 재추첨 한도 초과 - 보유 등급 중 랜덤 선택");
        var fallback = new System.Collections.Generic.List<ChickenData>();
        foreach (var list in _dataByGrade.Values)
            fallback.AddRange(list);
        return fallback.Count > 0 ? fallback[Random.Range(0, fallback.Count)] : null;
    }

    private Grade RollGrade()
    {
        float total = 0f;
        foreach (var w in _gachaWeights) total += w.weight;

        float roll = Random.Range(0f, total);
        float cumulative = 0f;

        foreach (var w in _gachaWeights)
        {
            cumulative += w.weight;
            if (roll < cumulative) return w.grade;
        }

        return Grade.Common;
    }

    private void UpdateGachaButtonUI()
    {
        if (_gachaButtonText != null)
            _gachaButtonText.text = $"닭 구매\n{_gachaCost}G";
    }

    private void UpdateLimitUpgradeButtonUI()
    {
        if (_limitUpgradeButtonText == null) return;
        if (_chickenLimit >= MaxChickenLimit)
            _limitUpgradeButtonText.text = "슬롯 확장\n최대";
        else
            _limitUpgradeButtonText.text = $"슬롯 확장\n{(_chickenLimit - 4) * _limitUpgradeCost}G";
    }

    // ─── 판매 ─────────────────────────────────────────────

    public void Sell(Chicken chicken)
    {
        chicken.gameObject.SetActive(false);
        _activeChickens.Remove(chicken);
    }

    // ─── 머지 ─────────────────────────────────────────────

    public List<Chicken> FindMergeables(ChickenData data, int level)
    {
        // 에픽은 같은 레벨의 모든 닭과 머지 가능 (와일드카드)
        return _activeChickens.FindAll(c =>
            c.Level == level &&
            (c.Data == data || c.Data.grade == Grade.Epic || data.grade == Grade.Epic)
        );
    }

    public void Merge(List<Chicken> targets, Vector3 spawnPos)
    {
        foreach (var chicken in targets)
        {
            chicken.gameObject.SetActive(false);
            _activeChickens.Remove(chicken);
        }

        Grade[] mergeableGrades = { Grade.Common, Grade.Uncommon, Grade.Rare };
        Grade rolledGrade = mergeableGrades[Random.Range(0, mergeableGrades.Length)];

        if (!_dataByGrade.TryGetValue(rolledGrade, out var candidates) || candidates.Count == 0)
        {
            Debug.LogWarning($"[ChickenManager] 머지 결과 {rolledGrade} 등급 닭이 없습니다.");
            return;
        }

        ChickenData newData = candidates[Random.Range(0, candidates.Count)];
        SpawnMergedChicken(newData, spawnPos);
    }

    // ─── 스폰 ─────────────────────────────────────────────

    private Transform FindEmptySlot()
    {
        foreach (var slot in chickenSlots)
        {
            bool occupied = false;
            foreach (var c in _activeChickens)
            {
                if (c.transform.position == slot.position)
                {
                    occupied = true;
                    break;
                }
            }
            if (!occupied) return slot;
        }
        return null;
    }

    private void SpawnChicken(ChickenData data)
    {
        var pool = _pools[data];

        Chicken chicken = pool.canRecycle
            ? pool.GetRecycledObject()
            : CreateAndRegister(data, pool);

        Transform slot = FindEmptySlot();
        if (slot == null)
        {
            Debug.LogError("[ChickenManager] 비어있는 슬롯이 없습니다.");
            return;
        }

        chicken.transform.position = slot.position;
        chicken.gameObject.SetActive(true);
        chicken.Initialize(data);

        _activeChickens.Add(chicken);
        Debug.Log($"[ChickenManager] {data.grade} 등급 {data.infoData.entityName} 생성 ({_activeChickens.Count}/{_chickenLimit})");
    }

    private void SpawnMergedChicken(ChickenData data, Vector3 spawnPos)
    {
        var pool = _pools[data];

        Chicken chicken = pool.canRecycle
            ? pool.GetRecycledObject()
            : CreateAndRegister(data, pool);

        chicken.transform.position = spawnPos;
        chicken.gameObject.SetActive(true);
        chicken.InitializeWithLevel(data, 2);

        _activeChickens.Add(chicken);
        Debug.Log($"[ChickenManager] 머지 완료 -> {data.grade} 등급 {data.infoData.entityName} Lv2");
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