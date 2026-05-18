using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MercenaryGachaWeight
{
    public Grade grade;
    [Tooltip("뽑기 가중치")]
    public float weight;
}

public class MercenarySlotManager : MonoBehaviour
{
    [SerializeField] private MercenarySlot[] _slots;
    [SerializeField] private MercenaryStatData[] _mercenaryDataList;
    [SerializeField] private int preloadCountPerType = 3;
    [SerializeField] private int _gachaCost = 100;

    [Tooltip("이 거리 이상이면 슬롯으로 인식하지 않음")]
    [SerializeField] private float _slotSnapDistance = 1f;

    [Header("등급별 뽑기 가중치")]
    [SerializeField]
    private MercenaryGachaWeight[] _gachaWeights = new MercenaryGachaWeight[]
    {
        new MercenaryGachaWeight { grade = Grade.Common,   weight = 60f },
        new MercenaryGachaWeight { grade = Grade.Uncommon, weight = 25f },
        new MercenaryGachaWeight { grade = Grade.Rare,     weight = 12f },
        new MercenaryGachaWeight { grade = Grade.Epic,     weight = 3f  },
    };

    private Dictionary<MercenaryStatData, ObjectPool<MercenaryBase>> _pools;
    private Dictionary<Grade, List<MercenaryStatData>> _dataByGrade;

    private void Awake()
    {
        DependencyInjector.Constructor(this);

        _pools = new Dictionary<MercenaryStatData, ObjectPool<MercenaryBase>>();
        _dataByGrade = new Dictionary<Grade, List<MercenaryStatData>>();

        foreach (var data in _mercenaryDataList)
        {
            var pool = new ObjectPool<MercenaryBase>();
            _pools[data] = pool;

            for (int i = 0; i < preloadCountPerType; i++)
                CreateAndRegister(data, pool);

            if (!_dataByGrade.ContainsKey(data.grade))
                _dataByGrade[data.grade] = new List<MercenaryStatData>();
            _dataByGrade[data.grade].Add(data);
        }
    }

    private void OnDestroy()
    {
        DependencyInjector.Demolisher(this);
    }

    // ─── 가챠 ─────────────────────────────────────────────

    public void TryGacha()
    {
        var resourceManager = DependencyInjector.Get<ResourceManager>();
        if (!resourceManager.TrySpend(_gachaCost))
        {
            Debug.Log("[MercenarySlotManager] 돈이 부족합니다.");
            return;
        }

        MercenaryStatData data = RollMercenary();
        if (data == null)
        {
            Debug.LogError("[MercenarySlotManager] 뽑기 결과가 없습니다.");
            resourceManager.Earn(_gachaCost);
            return;
        }

        MercenarySlot slot = FindAvailableSlot(data);
        if (slot == null)
        {
            Debug.Log("[MercenarySlotManager] 배치 가능한 슬롯이 없습니다.");
            resourceManager.Earn(_gachaCost);
            return;
        }

        var pool = _pools[data];
        MercenaryBase mercenary = pool.canRecycle
            ? pool.GetRecycledObject()
            : CreateAndRegister(data, pool);

        mercenary.Initialize(data);
        slot.Add(mercenary);

        Debug.Log($"[MercenarySlotManager] {data.grade} 등급 {data.mercenaryName} 획득");
    }

    private MercenaryStatData RollMercenary()
    {
        Grade rolledGrade = RollGrade();

        if (!_dataByGrade.TryGetValue(rolledGrade, out var candidates) || candidates.Count == 0)
        {
            Debug.LogWarning($"[MercenarySlotManager] {rolledGrade} 등급 용병이 없습니다.");
            return null;
        }

        return candidates[Random.Range(0, candidates.Count)];
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

    // ─── 머지 ─────────────────────────────────────────────

    public List<MercenaryBase> FindMergeables(MercenaryStatData data)
    {
        var result = new List<MercenaryBase>();
        foreach (var slot in _slots)
            foreach (var m in slot.GetMercenaries())
                if (m.Data == data) result.Add(m);
        return result;
    }

    public void Merge(List<MercenaryBase> targets, MercenarySlot targetSlot)
    {
        foreach (var mercenary in targets)
        {
            mercenary.Slot?.RemoveMercenary(mercenary);
            mercenary.gameObject.SetActive(false);
        }

        Grade currentGrade = targets[0].Data.grade;
        Grade nextGrade = (Grade)((int)currentGrade + 1);

        if (!_dataByGrade.TryGetValue(nextGrade, out var candidates) || candidates.Count == 0)
        {
            Debug.LogWarning($"[MercenarySlotManager] 머지 결과 {nextGrade} 등급 용병이 없습니다.");
            return;
        }

        MercenaryStatData newData = candidates[Random.Range(0, candidates.Count)];

        var pool = _pools[newData];
        MercenaryBase newMercenary = pool.canRecycle
            ? pool.GetRecycledObject()
            : CreateAndRegister(newData, pool);

        newMercenary.Initialize(newData);
        targetSlot.Add(newMercenary);

        Debug.Log($"[MercenarySlotManager] 머지 완료 -> {newData.grade} 등급 {newData.mercenaryName}");
    }

    // ─── 드래그 ───────────────────────────────────────────

    public MercenarySlot GetClosestSlot(Vector3 worldPos, MercenarySlot exclude)
    {
        MercenarySlot closest = null;
        float minDist = float.MaxValue;
        float maxDistSq = _slotSnapDistance * _slotSnapDistance;

        foreach (var slot in _slots)
        {
            if (slot == exclude) continue;

            float distSq = (slot.transform.position - worldPos).sqrMagnitude;
            if (distSq < minDist && distSq <= maxDistSq)
            {
                minDist = distSq;
                closest = slot;
            }
        }

        return closest;
    }

    // ─── 슬롯 탐색 ────────────────────────────────────────

    private MercenarySlot FindAvailableSlot(MercenaryStatData data)
    {
        foreach (var slot in _slots)
            if (!slot.IsEmpty && slot.CanStack(data))
                return slot;

        foreach (var slot in _slots)
            if (slot.IsEmpty)
                return slot;

        return null;
    }

    // ─── 풀 ───────────────────────────────────────────────

    private MercenaryBase CreateAndRegister(MercenaryStatData data, ObjectPool<MercenaryBase> pool)
    {
        GameObject go = Instantiate(data.prefab);
        MercenaryBase mercenary = go.GetComponent<MercenaryBase>();

        go.SetActive(false);
        pool.RegisterRecyclableObject(mercenary);
        return mercenary;
    }
}