using System;
using System.Collections.Generic;
using UnityEngine;

public class EggPriceManager : MonoBehaviour
{
    [Serializable]
    public class EggPriceConfig
    {
        public Grade grade;
        public int minPrice;
        public int maxPrice;
    }

    [SerializeField]
    private EggPriceConfig[] _priceConfigs = new EggPriceConfig[]
    {
        new EggPriceConfig { grade = Grade.Common,   minPrice = 5,  maxPrice = 15 },
        new EggPriceConfig { grade = Grade.Uncommon, minPrice = 7,  maxPrice = 22 },
        new EggPriceConfig { grade = Grade.Rare,     minPrice = 10, maxPrice = 30 },
        new EggPriceConfig { grade = Grade.Epic,     minPrice = 50, maxPrice = 80 },
    };

    public event Action OnPriceChanged;

    private readonly Dictionary<Grade, int> _currentPrices = new Dictionary<Grade, int>();
    private readonly Dictionary<Grade, (int min, int max)> _priceRanges = new Dictionary<Grade, (int, int)>();

    private void Awake()
    {
        DependencyInjector.Constructor(this);

        foreach (var config in _priceConfigs)
        {
            _currentPrices[config.grade] = config.minPrice;
            _priceRanges[config.grade] = (config.minPrice, config.maxPrice);
        }
    }

    private void OnDestroy()
    {
        DependencyInjector.Demolisher(this);
    }

    public int GetPrice(Grade grade) => _currentPrices[grade];
    public (int min, int max) GetRange(Grade grade) => _priceRanges[grade];

    public void RollPrices()
    {
        foreach (var config in _priceConfigs)
        {
            _currentPrices[config.grade] = UnityEngine.Random.Range(config.minPrice, config.maxPrice + 1);
        }

        OnPriceChanged?.Invoke();

        Debug.Log($"[EggPriceManager] 가격 변동 — " +
                  $"Common: {_currentPrices[Grade.Common]}G / " +
                  $"Uncommon: {_currentPrices[Grade.Uncommon]}G / " +
                  $"Rare: {_currentPrices[Grade.Rare]}G / " +
                  $"Epic: {_currentPrices[Grade.Epic]}G");
    }
}