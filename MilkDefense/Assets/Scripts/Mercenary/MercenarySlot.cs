using System.Collections.Generic;
using UnityEngine;

public class MercenarySlot : MonoBehaviour
{
    public const int MaxStack = 3;

    private static readonly Vector3[] StackOffsets = new Vector3[]
    {
        new Vector3( 0f,    0f,   0f),
        new Vector3(-0.25f, 0.2f, 0f),
        new Vector3( 0.25f,-0.2f, 0f),
    };

    private readonly List<MercenaryBase> _mercenaries = new List<MercenaryBase>();

    public int Count => _mercenaries.Count;
    public bool IsFull => _mercenaries.Count >= MaxStack;
    public bool IsEmpty => _mercenaries.Count == 0;

    public bool CanStack(MercenaryStatData data)
    {
        if (IsEmpty) return true;
        if (IsFull) return false;
        return _mercenaries[0].Data == data;
    }

    public void Add(MercenaryBase mercenary)
    {
        int index = _mercenaries.Count;
        _mercenaries.Add(mercenary);
        mercenary.transform.position = transform.position + StackOffsets[index];
        mercenary.SetSlot(this);
        mercenary.gameObject.SetActive(true);
        ApplySortingOrder(mercenary, index);
    }

    private void ApplySortingOrder(MercenaryBase mercenary, int index)
    {
        foreach (var sr in mercenary.GetComponentsInChildren<SpriteRenderer>())
            sr.sortingOrder = index + 1;
    }

    // 비활성화 포함 제거 (풀 반환용)
    public List<MercenaryBase> RemoveAll()
    {
        var removed = new List<MercenaryBase>(_mercenaries);
        foreach (var m in _mercenaries)
            m.gameObject.SetActive(false);
        _mercenaries.Clear();
        return removed;
    }

    // 비활성화 없이 참조만 제거 (재정렬용)
    public List<MercenaryBase> DetachAll()
    {
        var detached = new List<MercenaryBase>(_mercenaries);
        foreach (var m in _mercenaries)
            m.SetSlot(null);
        _mercenaries.Clear();
        return detached;
    }

    public IReadOnlyList<MercenaryBase> GetMercenaries() => _mercenaries;

    public void RemoveMercenary(MercenaryBase mercenary)
    {
        _mercenaries.Remove(mercenary);
        ResetPositions();
    }

    public void SwapWith(MercenarySlot other)
    {
        var myMercs = DetachAll();
        var otherMercs = other.DetachAll();

        foreach (var m in otherMercs) Add(m);
        foreach (var m in myMercs) other.Add(m);
    }

    public void UpdateDragPosition(Vector3 worldPos)
    {
        foreach (var m in _mercenaries)
            m.transform.position = worldPos;
    }

    public void ResetPositions()
    {
        for (int i = 0; i < _mercenaries.Count; i++)
        {
            _mercenaries[i].transform.position = transform.position + StackOffsets[i];
            ApplySortingOrder(_mercenaries[i], i);
        }
    }
}