using System;
using UnityEngine;
using UnityEngine.UI;

public class HpBarInstance : MonoBehaviour, IObjectPoolable
{
    public bool canRecyclable { get; set; }
    public Action OnRecycleStartSignature { get; }
    public Action OnRecycleFinishSignature { get; }

    [SerializeField] private Image _fillImage;
    public EnemyType enemyType;

    protected virtual void OnEnable() => canRecyclable = false;
    protected virtual void OnDisable() => canRecyclable = true;

    public void UpdateBar(float current, float max)
    {
        _fillImage.fillAmount = current / max;
    }
}