using System;
using System.Collections.Generic;
using UnityEngine;

public class EggPriceManager : MonoBehaviour
{
    [Serializable]
    public class EggPriceConfig
    {
        public Grade grade;
        public int basePrice;
        [Tooltip("변동 범위 (0.3 = ±30%)")]
        public float varianceRatio;
    }

    [SerializeField]
    private EggPriceConfig[] _priceConfigs = new EggPriceConfig[]
    {
        new EggPriceConfig { grade = Grade.Common,   basePrice = 10,  varianceRatio = 0.3f },
        new EggPriceConfig { grade = Grade.Uncommon, basePrice = 25,  varianceRatio = 0.35f },
        new EggPriceConfig { grade = Grade.Rare,     basePrice = 60,  varianceRatio = 0.4f },
        new EggPriceConfig { grade = Grade.Epic,     basePrice = 150, varianceRatio = 0.45f },
    };

    public event Action OnPriceChanged;

    private readonly Dictionary<Grade, int> _currentPrices = new Dictionary<Grade, int>();

    private void Awake()
    {
        DependencyInjector.Constructor(this);

        foreach (var config in _priceConfigs)
            _currentPrices[config.grade] = config.basePrice;
    }

    private void OnDestroy()
    {
        DependencyInjector.Demolisher(this);
    }

    public int GetPrice(Grade grade) => _currentPrices[grade];

    public void RollPrices()
    {
        foreach (var config in _priceConfigs)
        {
            float multiplier = 1f + UnityEngine.Random.Range(-config.varianceRatio, config.varianceRatio);
            _currentPrices[config.grade] = Mathf.Max(1, Mathf.RoundToInt(config.basePrice * multiplier));
        }

        OnPriceChanged?.Invoke();

        Debug.Log($"[EggPriceManager] 가격 변동 — " +
                  $"Common: {_currentPrices[Grade.Common]}G / " +
                  $"Uncommon: {_currentPrices[Grade.Uncommon]}G / " +
                  $"Rare: {_currentPrices[Grade.Rare]}G / " +
                  $"Epic: {_currentPrices[Grade.Epic]}G");
    }
}