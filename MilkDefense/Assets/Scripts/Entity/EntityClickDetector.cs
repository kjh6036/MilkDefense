using UnityEngine;

// RangeIndicator와 같은 GameObject에 부착
[RequireComponent(typeof(RangeIndicator))]
public class EntityClickDetector : MonoBehaviour
{
    [SerializeField] private EntityInfoUI _entityInfoUI;
    [SerializeField] private LayerMask _clickableLayer;

    private Camera _mainCamera;
    private RangeIndicator _rangeIndicator;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _rangeIndicator = GetComponent<RangeIndicator>();
    }

    private void Start()
    {
        _rangeIndicator.Hide();
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 worldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, _clickableLayer);

        if (hit.collider == null || !hit.collider.TryGetComponent<IClickableEntity>(out var entity))
        {
            _entityInfoUI.Hide();
            _rangeIndicator.Hide();
            DependencyInjector.Get<MergeManager>().CheckHide();
            return;
        }

        _entityInfoUI.Show(entity.InfoData);

        if (hit.collider.TryGetComponent<Chicken>(out var chicken))
        {
            chicken.TryLayEgg();
            DependencyInjector.Get<MergeManager>().CheckChickenMerge(chicken, chicken.transform.position);
            _rangeIndicator.Hide();
            return;
        }

        if (hit.collider.TryGetComponent<MercenaryBase>(out var mercenary) && mercenary.Slot != null)
        {
            _rangeIndicator.Show(mercenary.Slot.transform.position, mercenary.Data.attackRange);
            DependencyInjector.Get<MergeManager>().CheckMercenaryMerge(mercenary, mercenary.Slot.transform.position);
            return;
        }

        _rangeIndicator.Hide();
        DependencyInjector.Get<MergeManager>().CheckHide();
    }
}