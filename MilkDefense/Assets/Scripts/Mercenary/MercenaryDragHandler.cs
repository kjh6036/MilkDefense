using UnityEngine;

public class MercenaryDragHandler : MonoBehaviour
{
    [SerializeField] private RangeIndicator _rangeIndicator;
    [SerializeField] private LayerMask _draggableLayer;

    private MercenaryBase _mercenary;
    private Camera _mainCamera;
    private bool _isDragging;
    private Vector3 _dragOffset;

    private void Awake()
    {
        _mercenary = GetComponent<MercenaryBase>();
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Slot이 없으면 드래그 불가
            if (_mercenary.Slot == null) return;

            Vector2 worldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, _draggableLayer);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                _isDragging = true;
                _dragOffset = transform.position - (Vector3)worldPos;
                _rangeIndicator?.Hide();
            }
        }

        if (Input.GetMouseButton(0) && _isDragging)
        {
            if (_mercenary.Slot == null) { CancelDrag(); return; }

            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0f;
            _mercenary.Slot.UpdateDragPosition(worldPos + _dragOffset);
        }

        if (Input.GetMouseButtonUp(0) && _isDragging)
        {
            _isDragging = false;

            if (_mercenary.Slot == null) return;

            Vector2 worldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            MercenarySlot fromSlot = _mercenary.Slot;
            MercenarySlot toSlot = DependencyInjector.Get<MercenarySlotManager>().GetClosestSlot(worldPos, fromSlot);

            if (toSlot != null)
            {
                fromSlot.SwapWith(toSlot);
                _rangeIndicator?.Show(toSlot.transform.position, _mercenary.Data.attackRange);
            }
            else
            {
                fromSlot.ResetPositions();
                _rangeIndicator?.Show(fromSlot.transform.position, _mercenary.Data.attackRange);
            }
        }
    }

    private void CancelDrag()
    {
        _isDragging = false;
        _mercenary.Slot?.ResetPositions();
    }

    private void OnDisable()
    {
        _isDragging = false;
    }
}