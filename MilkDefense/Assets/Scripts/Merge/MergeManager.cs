using System.Collections.Generic;
using UnityEngine;

// MergeButton과 같은 GameObject에 부착
[RequireComponent(typeof(MergeButton))]
public class MergeManager : MonoBehaviour
{
    private MergeButton _mergeButton;

    private void Awake()
    {
        DependencyInjector.Constructor(this);
        _mergeButton = GetComponent<MergeButton>();
    }

    private void OnDestroy()
    {
        DependencyInjector.Demolisher(this);
    }

    // ─── 닭 ───────────────────────────────────────────────

    public void CheckChickenMerge(Chicken clicked, Vector3 worldPos)
    {
        List<Chicken> matches = DependencyInjector.Get<ChickenManager>().FindMergeables(clicked.Data, clicked.Level);

        if (matches.Count >= 3)
            _mergeButton.Show(worldPos, () => ExecuteChickenMerge(clicked, matches));
        else
            _mergeButton.Hide();
    }

    private void ExecuteChickenMerge(Chicken clicked, List<Chicken> matches)
    {
        var targets = SelectThree(clicked, matches);
        DependencyInjector.Get<ChickenManager>().Merge(targets, clicked.transform.position);
        DependencyInjector.Get<EntityClickDetector>().HideAll();
    }

    // ─── 용병 ─────────────────────────────────────────────

    public void CheckMercenaryMerge(MercenaryBase clicked, Vector3 worldPos)
    {
        if (clicked.Data.grade == Grade.Epic)
        {
            _mergeButton.Hide();
            return;
        }

        List<MercenaryBase> matches = DependencyInjector.Get<MercenarySlotManager>().FindMergeables(clicked.Data);

        if (matches.Count >= 3)
            _mergeButton.Show(worldPos, () => ExecuteMercenaryMerge(clicked, matches));
        else
            _mergeButton.Hide();
    }

    private void ExecuteMercenaryMerge(MercenaryBase clicked, List<MercenaryBase> matches)
    {
        var targets = SelectThree(clicked, matches);
        DependencyInjector.Get<MercenarySlotManager>().Merge(targets, clicked.Slot);
        DependencyInjector.Get<EntityClickDetector>().HideAll();
    }

    public void CheckHide()
    {
        _mergeButton.Hide();
    }

    // ─── 공통 ─────────────────────────────────────────────

    private List<T> SelectThree<T>(T clicked, List<T> matches) where T : class
    {
        var targets = new List<T> { clicked };
        foreach (var m in matches)
        {
            if (targets.Count >= 3) break;
            if (m != clicked) targets.Add(m);
        }
        return targets;
    }
}