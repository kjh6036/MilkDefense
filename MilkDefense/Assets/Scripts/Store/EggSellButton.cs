using TMPro;
using UnityEngine;

public class EggSellButton : MonoBehaviour
{
    [SerializeField] private Grade _grade;
    [SerializeField] private EggInventory _eggInventory;
    [SerializeField] private EggPriceManager _eggPriceManager;

    private TextMeshProUGUI _priceText;
    private TextMeshProUGUI _amountText;

    private void Awake()
    {
        _priceText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        _amountText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        _eggInventory.OnInventoryChanged += UpdateUI;
        _eggPriceManager.OnPriceChanged += UpdateUI;
        UpdateUI();
    }

    private void OnDisable()
    {
        _eggInventory.OnInventoryChanged -= UpdateUI;
        _eggPriceManager.OnPriceChanged -= UpdateUI;
    }

    private void OnDestroy()
    {
        _eggInventory.OnInventoryChanged -= UpdateUI;
        _eggPriceManager.OnPriceChanged -= UpdateUI;
    }

    private void UpdateUI()
    {
        int current = _eggPriceManager.GetPrice(_grade);
        var (min, max) = _eggPriceManager.GetRange(_grade);
        _priceText.text = $"{current}G ({min}~{max}G)";
        _amountText.text = _eggInventory.Get(_grade).ToString();
    }

    public void SellButtonOnClick()
    {
        _eggInventory.TrySell(_grade);
    }
}