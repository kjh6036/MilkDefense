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

    private Transform[] _waypoints;
    private EnemyStatData _statData;
    private HpBarInstance _hpBar;
    private float _currentHp;
    private float _moveSpeed;
    private int _waypointIndex;

    protected virtual void OnEnable() => canRecyclable = false;
    protected virtual void OnDisable()
    {
        canRecyclable = true;

        DependencyInjector.Get<EnemyRegistry>()?.Unregister(this);

        if (_hpBar == null) return;
        DependencyInjector.Get<HpBarManager>()?.Release(transform);
        _hpBar = null;
    }

    public void Initialize(EnemyStatData data, Transform[] waypoints)
    {
        _waypoints = waypoints;
        _statData = data;
        _currentHp = data.maxHp;
        _moveSpeed = data.moveSpeed;
        _waypointIndex = 0;

        if (_waypoints != null && _waypoints.Length > 0)
            transform.position = _waypoints[0].position;

        _hpBar = DependencyInjector.Get<HpBarManager>().Get(transform, data.enemyType);
        _hpBar.UpdateBar(_currentHp, data.maxHp);

        DependencyInjector.Get<EnemyRegistry>().Register(this);
    }

    private void Update()
    {
        if (_waypoints == null || _waypoints.Length == 0) return;
        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        Transform target = _waypoints[_waypointIndex];
        Vector3 dir = target.position - transform.position;

        if (dir.sqrMagnitude < 0.04f)
        {
            transform.position = target.position;
            _waypointIndex = (_waypointIndex + 1) % _waypoints.Length;
            return;
        }

        transform.position += dir.normalized * _moveSpeed * Time.deltaTime;
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