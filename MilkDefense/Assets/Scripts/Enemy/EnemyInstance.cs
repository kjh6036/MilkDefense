using System;
using UnityEngine;

public class EnemyInstance : MonoBehaviour, IObjectPoolable, IClickableEntity
{
    public bool canRecyclable { get; set; }
    public Action OnRecycleStartSignature { get; }
    public Action OnRecycleFinishSignature { get; }

    public event Action OnDied;

    [SerializeField] private EntityInfoData _infoData;
    public EntityInfoData InfoData => _infoData;

    private EnemyStatData _statData;
    private HpBarInstance _hpBar;
    private float _currentHp;
    private float _moveSpeed;

    protected virtual void OnEnable() => canRecyclable = false;
    protected virtual void OnDisable()
    {
        canRecyclable = true;
        if (_hpBar == null) return;
        if (HpBarManager.Instance != null)
            HpBarManager.Instance.Release(transform);
        _hpBar = null;
    }

    public void Initialize(EnemyStatData data)
    {
        _statData = data;
        _currentHp = data.maxHp;
        _moveSpeed = data.moveSpeed;

        _hpBar = HpBarManager.Instance.Get(transform, data.enemyType);
        _hpBar.UpdateBar(_currentHp, data.maxHp);
    }

    public void TakeDamage(float dmg)
    {
        if (_currentHp <= 0f) return;
        _currentHp -= dmg;
        _hpBar?.UpdateBar(_currentHp, _statData.maxHp);
        if (_currentHp <= 0f) Die();
    }

    private void Die()
    {
        OnDied?.Invoke();
        gameObject.SetActive(false);
    }
}