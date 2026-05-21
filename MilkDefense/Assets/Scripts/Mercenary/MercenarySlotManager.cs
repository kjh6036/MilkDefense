using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Tooltip("이 거리 이상이면 슬롯으로 인식하지 않음")]
    [SerializeField] private float _slotSnapDistance = 1f;

    [Header("가챠")]
    [SerializeField] private int _gachaBaseCost = 50;
    [SerializeField] private int _gachaCostIncrement = 2;
    [SerializeField] private Button _gachaButton;

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
    private int _gachaPullCount = 0;
    private TextMeshProUGUI _gachaButtonText;

    public int GachaCost => _gachaBaseCost + _gachaCostIncrement * _gachaPullCount;

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

        if (_gachaButton != null)
        {
            _gachaButtonText = _gachaButton.GetComponentInChildren<TextMeshProUGUI>();
            UpdateGachaButtonUI();
        }
    }

    private void OnDestroy()
    {
        DependencyInjector.Demolisher(this);
    }

    // ─── 가챠 ─────────────────────────────────────────────

    public void TryGacha()
    {
        int cost = GachaCost;
        var resourceManager = DependencyInjector.Get<ResourceManager>();
        if (!resourceManager.TrySpend(cost))
        {
            Debug.Log("[MercenarySlotManager] 돈이 부족합니다.");
            return;
        }

        MercenaryStatData data = RollMercenary();
        if (data == null)
        {
            Debug.LogError("[MercenarySlotManager] 뽑기 결과가 없습니다.");
            resourceManager.Earn(cost);
            return;
        }

        MercenarySlot slot = FindAvailableSlot(data);
        if (slot == null)
        {
            Debug.Log("[MercenarySlotManager] 배치 가능한 슬롯이 없습니다.");
            resourceManager.Earn(cost);
            return;
        }

        var pool = _pools[data];
        MercenaryBase mercenary = pool.canRecycle
            ? pool.GetRecycledObject()
            : CreateAndRegister(data, pool);

        mercenary.Initialize(data);
        slot.Add(mercenary);

        _gachaPullCount++;
        UpdateGachaButtonUI();
        Debug.Log($"[MercenarySlotManager] {data.grade} 등급 {data.mercenaryName} 획득 — 다음 비용: {GachaCost}G");
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

    private void UpdateGachaButtonUI()
    {
        if (_gachaButtonText != null)
            _gachaButtonText.text = $"용병 고용\n{GachaCost}G";
    }

    // ─── 판매 ─────────────────────────────────────────────

    public void Sell(MercenaryBase mercenary)
    {
        mercenary.Slot?.RemoveMercenary(mercenary);
        mercenary.gameObject.SetActive(false);
        Rearrange();
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
        // targetSlot 인덱스 저장 (Rearrange 전에 위치 확보)
        int targetSlotIndex = System.Array.IndexOf(_slots, targetSlot);

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
            Rearrange();
            return;
        }

        MercenaryStatData newData = candidates[Random.Range(0, candidates.Count)];

        var pool = _pools[newData];
        MercenaryBase newMercenary = pool.canRecycle
            ? pool.GetRecycledObject()
            : CreateAndRegister(newData, pool);

        newMercenary.Initialize(newData);

        // 나머지 용병 재정렬 후 새 용병 배치
        Rearrange();

        // 재정렬 후 targetSlot이 비어있으면 직접 배치, 아니면 빈 슬롯 찾기
        MercenarySlot slot = targetSlotIndex >= 0 && _slots[targetSlotIndex].IsEmpty
            ? _slots[targetSlotIndex]
            : FindAvailableSlot(newData);

        if (slot == null)
        {
            Debug.LogWarning("[MercenarySlotManager] 머지 결과 배치할 슬롯이 없습니다.");
            return;
        }

        slot.Add(newMercenary);
        Debug.Log($"[MercenarySlotManager] 머지 완료 -> {newData.grade} 등급 {newData.mercenaryName}");
    }

    // ─── 재정렬 ───────────────────────────────────────────

    private void Rearrange()
    {
        var all = new List<MercenaryBase>();
        foreach (var slot in _slots)
        {
            all.AddRange(slot.GetMercenaries());
            slot.DetachAll();
        }

        foreach (var mercenary in all)
        {
            MercenarySlot target = null;

            foreach (var slot in _slots)
            {
                if (slot.CanStack(mercenary.Data) && !slot.IsEmpty)
                {
                    target = slot;
                    break;
                }
            }

            if (target == null)
            {
                foreach (var slot in _slots)
                {
                    if (slot.IsEmpty)
                    {
                        target = slot;
                        break;
                    }
                }
            }

            if (target == null)
            {
                Debug.LogWarning("[MercenarySlotManager] 재정렬 중 배치할 슬롯이 없습니다.");
                mercenary.gameObject.SetActive(false);
                continue;
            }

            target.Add(mercenary);
        }
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