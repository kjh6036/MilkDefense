using UnityEngine;

// CharacterSellButton과 같은 GameObject에 부착
[RequireComponent(typeof(CharacterSellButton))]
public class CharacterSellManager : MonoBehaviour
{
    private static readonly float[] SellRatioByGrade = { 0.25f, 1f, 2f, 5f };

    private CharacterSellButton _sellButton;

    private void Awake()
    {
        DependencyInjector.Constructor(this);
        _sellButton = GetComponent<CharacterSellButton>();
    }

    private void OnDestroy()
    {
        DependencyInjector.Demolisher(this);
    }

    // ─── 닭 ───────────────────────────────────────────────

    public void ShowChickenSell(Chicken chicken, Vector3 worldPos)
    {
        int price = CalcSellPrice(DependencyInjector.Get<ChickenManager>().GachaCost, chicken.Data.grade);
        Debug.Log($"[CharacterSellManager] ShowChickenSell - {chicken.Data.infoData.entityName} / {price}G");
        _sellButton.Show(worldPos, () => ExecuteChickenSell(chicken, price));
    }

    private void ExecuteChickenSell(Chicken chicken, int price)
    {
        Debug.Log($"[CharacterSellManager] ExecuteChickenSell 실행");
        DependencyInjector.Get<ChickenManager>().Sell(chicken);
        DependencyInjector.Get<ResourceManager>().Earn(price);
        Debug.Log($"[CharacterSellManager] 닭 {chicken.Data.infoData.entityName} 판매 → {price}G");
    }

    // ─── 용병 ─────────────────────────────────────────────

    public void ShowMercenarySell(MercenaryBase mercenary, Vector3 worldPos)
    {
        int price = CalcSellPrice(mercenary.Data.purchaseCost, mercenary.Data.grade);
        Debug.Log($"[CharacterSellManager] ShowMercenarySell - {mercenary.Data.mercenaryName} / {price}G");
        _sellButton.Show(worldPos, () => ExecuteMercenarySell(mercenary, price));
    }

    private void ExecuteMercenarySell(MercenaryBase mercenary, int price)
    {
        Debug.Log($"[CharacterSellManager] ExecuteMercenarySell 실행");
        DependencyInjector.Get<MercenarySlotManager>().Sell(mercenary);
        DependencyInjector.Get<ResourceManager>().Earn(price);
        Debug.Log($"[CharacterSellManager] 용병 {mercenary.Data.mercenaryName} 판매 → {price}G");
    }

    // ─── 공통 ─────────────────────────────────────────────

    public void Hide()
    {
        _sellButton.Hide();
    }

    private int CalcSellPrice(int baseCost, Grade grade)
    {
        float ratio = SellRatioByGrade[(int)grade];
        return Mathf.Max(1, Mathf.RoundToInt(baseCost * ratio));
    }
}