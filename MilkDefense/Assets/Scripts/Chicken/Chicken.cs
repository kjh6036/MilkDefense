using System;
using System.Collections;
using UnityEngine;

public class Chicken : MonoBehaviour, IClickableEntity, IObjectPoolable
{
    [SerializeField] private ChickenData _data;

    public EntityInfoData InfoData => _data.infoData;

    public bool canRecyclable { get; set; }
    public Action OnRecycleStartSignature { get; }
    public Action OnRecycleFinishSignature { get; }

    private HpBarInstance _hpBar;
    private int _level = 1;
    private float _currentHp = 0f;
    private Coroutine _regenRoutine;

    public int Level => _level;
    public float MaxHp => GetStat().maxHp;
    public int EggCount => GetStat().eggCount;

    private const int MaxLevel = 3;
    private const float EggLayHpThreshold = 100f;
    private const float EggCountVariance = 0.2f;

    private ChickenLevelStat GetStat() => _data.levelStats[_level - 1];

    protected virtual void OnEnable() => canRecyclable = false;
    protected virtual void OnDisable()
    {
        canRecyclable = true;
        StopRegen();
        if (_hpBar == null) return;
        DependencyInjector.Get<HpBarManager>(suppressWarning: true)?.Release(transform);
        _hpBar = null;
    }

    public ChickenData Data => _data;

    public void Initialize(ChickenData data)
    {
        _data = data;
        _level = 1;
        _currentHp = 0f;
        _hpBar = DependencyInjector.Get<HpBarManager>().Get(transform, EnemyType.Normal);
        _hpBar.UpdateBar(_currentHp, MaxHp);
        StartRegen();
    }

    public void InitializeWithLevel(ChickenData data, int level)
    {
        Initialize(data);
        for (int i = 1; i < level; i++)
            LevelUp();
    }

    public void LevelUp()
    {
        if (_level >= MaxLevel)
        {
            Debug.Log("[Chicken] 이미 최대 레벨");
            return;
        }

        _level++;
        _currentHp = Mathf.Min(_currentHp, MaxHp);
        _hpBar?.UpdateBar(_currentHp, MaxHp);

        RestartRegen();
        Debug.Log($"[Chicken] 레벨업 완료 Lv{_level} (maxHp: {MaxHp}, eggCount: {EggCount})");
    }

    public void GainHp(float amount)
    {
        _currentHp = Mathf.Min(_currentHp + amount, MaxHp);
        _hpBar?.UpdateBar(_currentHp, MaxHp);
    }

    private void StartRegen()
    {
        StopRegen();
        _regenRoutine = StartCoroutine(RegenRoutine());
    }

    private void StopRegen()
    {
        if (_regenRoutine != null)
        {
            StopCoroutine(_regenRoutine);
            _regenRoutine = null;
        }
    }

    private void RestartRegen()
    {
        StopRegen();
        StartRegen();
    }

    private IEnumerator RegenRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(GetStat().hpRegenInterval);
            GainHp(GetStat().hpRegenAmount);
        }
    }

    public void TryLayEgg()
    {
        if (_currentHp < EggLayHpThreshold)
        {
            Debug.Log($"[Chicken] HP 부족 ({_currentHp:F0}/{EggLayHpThreshold})");
            return;
        }

        _currentHp -= EggLayHpThreshold;
        _hpBar?.UpdateBar(_currentHp, MaxHp);

        float multiplier = 1f + UnityEngine.Random.Range(-EggCountVariance, EggCountVariance);
        int count = Mathf.Max(1, Mathf.RoundToInt(EggCount * multiplier));

        var batch = new System.Collections.Generic.Dictionary<Grade, int>();
        for (int i = 0; i < count; i++)
        {
            Grade grade = RollEggGrade(GetStat().eggGradeWeights);
            if (!batch.ContainsKey(grade)) batch[grade] = 0;
            batch[grade]++;
        }
        DependencyInjector.Get<EggInventory>().AddBatch(batch);

        Debug.Log($"[Chicken] Lv{_level} 달걀 {count}개 획득 (남은 HP: {_currentHp:F0})");
    }

    private Grade RollEggGrade(float[] weights)
    {
        float total = 0f;
        foreach (var w in weights) total += w;

        float roll = UnityEngine.Random.Range(0f, total);
        float cumulative = 0f;

        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative)
                return (Grade)i;
        }

        return Grade.Common;
    }
}