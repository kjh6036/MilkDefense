using UnityEngine;
using UnityEngine.UI;

// SellManager와 같은 GameObject에 부착
public class CharacterSellButton : MonoBehaviour
{
    [SerializeField] private Button _button;

    private Camera _mainCamera;
    private System.Action _onSellCallback;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _button.onClick.AddListener(OnClick);
        Hide();
    }

    public void Show(Vector3 worldPos, System.Action onSellCallback)
    {
        _onSellCallback = onSellCallback;

        Vector3 screenPos = _mainCamera.WorldToScreenPoint(worldPos);
        _button.transform.position = screenPos;
        _button.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _onSellCallback = null;
        _button.gameObject.SetActive(false);
    }

    private void OnClick()
    {
        var callback = _onSellCallback;
        Hide();
        callback?.Invoke();
    }
}