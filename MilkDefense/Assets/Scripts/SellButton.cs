using TMPro;
using UnityEngine;

public class SellButton : MonoBehaviour
{
    private TextMeshProUGUI priceText = null;
    private int price = 0;

    private TextMeshProUGUI amountText = null;
    private int amount = 0;

    private void Awake()
    {
        priceText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        amountText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    public void ChangePrice(int price)
    {
        this.price = price;
        priceText.text = this.price.ToString(); 
    }

    public void ChangeAmount(int amount)
    {
        this.amount += amount;
        amountText.text = this.amount.ToString();
    }

    public void SellButtonOnClick()
    {
        if (amount <= 0)
            return;

        amount = 0;
        amountText.text = amount.ToString();
    }

}
