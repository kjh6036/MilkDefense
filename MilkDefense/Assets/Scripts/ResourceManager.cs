using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;

    [SerializeField] private int _startingMoney = 100;
    private int _money = 0;
    public int Money => _money;

    private void Awake()
    {
        DependencyInjector.Constructor(this);
        Earn(_startingMoney);
    }

    private void OnDestroy()
    {
        DependencyInjector.Demolisher(this);
    }

    public bool TrySpend(int amount)
    {
        if (_money < amount) return false;

        _money -= amount;
        UpdateUI();
        return true;
    }

    public void Earn(int amount)
    {
        _money += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        moneyText.text = $"{_money}G";
    }
}