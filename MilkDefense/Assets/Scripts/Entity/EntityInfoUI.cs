using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private void Awake()
    {
        panel.SetActive(false);
    }

    public void Show(EntityInfoData data)
    {
        iconImage.sprite = data.icon;
        nameText.text = data.entityName;
        descriptionText.text = data.description;

        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}