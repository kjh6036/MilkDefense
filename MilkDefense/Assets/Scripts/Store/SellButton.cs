using TMPro;
using UnityEngine;

public class SellButton : MonoBehaviour
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

    private void UpdateUI()
    {
        _priceText.text = _eggPriceManager.GetPrice(_grade).ToString();
        _amountText.text = _eggInventory.Get(_grade).ToString();
    }

    public void SellButtonOnClick()
    {
        _eggInventory.TrySell(_grade);
    }
}