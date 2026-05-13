using UnityEngine;

public class EntityClickDetector : MonoBehaviour
{
    private EntityInfoUI entityInfoUI;
    [SerializeField] private LayerMask clickableLayer;

    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
        entityInfoUI = GetComponent<EntityInfoUI>();
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 worldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, clickableLayer);

        if (hit.collider != null && hit.collider.TryGetComponent<IClickableEntity>(out var entity))
        {
            entityInfoUI.Show(entity.InfoData);
            return;
        }

        entityInfoUI.Hide();
    }
}