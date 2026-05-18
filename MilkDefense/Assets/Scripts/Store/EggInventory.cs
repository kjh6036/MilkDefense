using System;
using System.Collections.Generic;
using UnityEngine;

public class EggInventory : MonoBehaviour
{
    [SerializeField] private EggPriceManager _eggPriceManager;

    private readonly Dictionary<Grade, int> _eggs = new Dictionary<Grade, int>();
    public event Action OnInventoryChanged;

    private void Awake()
    {
        DependencyInjector.Constructor(this);

        foreach (Grade grade in Enum.GetValues(typeof(Grade)))
            _eggs[grade] = 0;
    }

    private void OnDestroy()
    {
        DependencyInjector.Demolisher(this);
    }

    public void Add(Grade grade, int count)
    {
        _eggs[grade] += count;
        OnInventoryChanged?.Invoke();
    }

    public void AddBatch(Dictionary<Grade, int> batch)
    {
        foreach (var pair in batch)
            _eggs[pair.Key] += pair.Value;
        OnInventoryChanged?.Invoke();
    }

    public int Get(Grade grade) => _eggs[grade];

    public bool TrySell(Grade grade)
    {
        if (_eggs[grade] <= 0)
        {
            Debug.Log($"[EggInventory] {grade} 달걀이 없습니다.");
            return false;
        }

        int price = _eggPriceManager.GetPrice(grade);
        int totalEarned = price * _eggs[grade];

        DependencyInjector.Get<ResourceManager>().Earn(totalEarned);
        Debug.Log($"[EggInventory] {grade} 달걀 {_eggs[grade]}개 × {price}G = {totalEarned}G 판매");

        _eggs[grade] = 0;
        OnInventoryChanged?.Invoke();
        return true;
    }
}