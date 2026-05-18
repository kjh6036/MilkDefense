using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class MercenaryBase : MonoBehaviour, IObjectPoolable, IClickableEntity
{
    public bool canRecyclable { get; set; }
    public Action OnRecycleStartSignature { get; }
    public Action OnRecycleFinishSignature { get; }

    [SerializeField] private EntityInfoData _infoData;
    public EntityInfoData InfoData => _infoData;

    protected MercenaryStatData _data;
    protected IMercenaryTargetSelector _targetSelector;
    protected EnemyInstance _currentTarget;
    protected float _attackTimer;

    public MercenaryStatData Data => _data;
    public MercenarySlot Slot { get; private set; }

    protected virtual void OnEnable() => canRecyclable = false;
    protected virtual void OnDisable() => canRecyclable = true;

    public void SetSlot(MercenarySlot slot) => Slot = slot;

    public virtual void Initialize(MercenaryStatData data)
    {
        _data = data;
        _attackTimer = 0f;
        _currentTarget = null;
        _targetSelector = new NearestTargetSelector();
    }

    public void SetTargetSelector(IMercenaryTargetSelector selector)
    {
        _targetSelector = selector;
    }

    private void Update()
    {
        _attackTimer -= Time.deltaTime;

        if (!IsTargetValid())
            AcquireTarget();

        if (_currentTarget == null) return;

        if (_attackTimer <= 0f)
        {
            Attack(_currentTarget);
            _attackTimer = _data.attackCooldown;
        }
    }

    private Vector3 SlotPosition => Slot != null ? Slot.transform.position : transform.position;

    private bool IsTargetValid()
    {
        if (_currentTarget == null || !_currentTarget.gameObject.activeSelf) return false;

        float distSq = (SlotPosition - _currentTarget.transform.position).sqrMagnitude;
        return distSq <= _data.attackRange * _data.attackRange;
    }

    private void AcquireTarget()
    {
        var candidates = GetEnemiesInRange();
        _currentTarget = _targetSelector?.Select(candidates, transform);
    }

    private List<EnemyInstance> GetEnemiesInRange()
    {
        var result = new List<EnemyInstance>();
        float rangeSq = _data.attackRange * _data.attackRange;
        Vector3 slotPos = SlotPosition;

        var registry = DependencyInjector.Get<EnemyRegistry>();
        if (registry == null) return result;

        foreach (var enemy in registry.ActiveEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeSelf) continue;

            float distSq = (enemy.transform.position - slotPos).sqrMagnitude;
            if (distSq <= rangeSq)
                result.Add(enemy);
        }

        return result;
    }

    protected abstract void Attack(EnemyInstance target);

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_data == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(SlotPosition, _data.attackRange);
    }
#endif
}