using UnityEngine;
using UnityEngine.UI;

// MergeManager와 같은 GameObject에 부착
public class MergeButton : MonoBehaviour
{
    [SerializeField] private Button _button;

    private Camera _mainCamera;
    private System.Action _onMergeCallback;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _button.onClick.AddListener(OnClick);
        Hide();
    }

    public void Show(Vector3 worldPos, System.Action onMergeCallback)
    {
        _onMergeCallback = onMergeCallback;

        Vector3 screenPos = _mainCamera.WorldToScreenPoint(worldPos);
        _button.transform.position = screenPos;
        _button.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _onMergeCallback = null;
        _button.gameObject.SetActive(false);
    }

    private void OnClick()
    {
        var callback = _onMergeCallback;
        Hide();
        callback?.Invoke();
    }
}