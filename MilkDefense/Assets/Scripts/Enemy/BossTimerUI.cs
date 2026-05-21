using TMPro;
using UnityEngine;

public class BossTimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 2f, 0f);

    private Camera _mainCamera;
    private Transform _target;

    private void Awake()
    {
        _mainCamera = Camera.main;
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 screenPos = _mainCamera.WorldToScreenPoint(_target.position + _offset);
        transform.position = screenPos;
    }

    public void Show(Transform bossTransform, float duration)
    {
        _target = bossTransform;
        gameObject.SetActive(true);
        UpdateText(duration);
    }

    public void UpdateText(float remaining)
    {
        _timerText.text = $"{remaining:F0}";
    }

    public void Hide()
    {
        _target = null;
        gameObject.SetActive(false);
    }
}
