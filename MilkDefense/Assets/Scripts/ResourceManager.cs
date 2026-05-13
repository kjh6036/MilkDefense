using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI moneyText;

    private int _money = 0;
    public int Money => _money;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
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