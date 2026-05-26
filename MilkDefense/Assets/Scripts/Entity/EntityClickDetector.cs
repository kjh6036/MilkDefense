using UnityEngine;
using UnityEngine.EventSystems;

public class EntityClickDetector : MonoBehaviour
{
    [SerializeField] private EntityInfoUI _entityInfoUI;
    [SerializeField] private LayerMask _clickableLayer;

    [Header("버튼 오프셋")]
    [SerializeField] private Vector3 _sellButtonOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private Vector3 _mergeButtonOffset = new Vector3(0f, -1f, 0f);

    private Camera _mainCamera;
    [SerializeField] private RangeIndicator _rangeIndicator;

    private void Awake()
    {
        DependencyInjector.Constructor(this);
        _mainCamera = Camera.main;
    }

    private void OnDestroy()
    {
        DependencyInjector.Demolisher(this);
    }

    public void HideAll()
    {
        _entityInfoUI.Hide();
        _rangeIndicator.Hide();
        DependencyInjector.Get<CharacterSellManager>().Hide();
        DependencyInjector.Get<MergeManager>().CheckHide();
    }

    public void RefreshMercenaryUI(MercenaryBase mercenary)
    {
        MercenarySlot slot = mercenary.Slot;
        if (slot == null) return;

        _entityInfoUI.ShowMercenary(mercenary.InfoData, mercenary.Data, mercenary.Data.skills);
        _rangeIndicator.Show(slot.transform.position, mercenary.Data.attackRange);
        DependencyInjector.Get<MergeManager>().CheckMercenaryMerge(mercenary, slot.transform.position + _mergeButtonOffset);
        DependencyInjector.Get<CharacterSellManager>().ShowMercenarySell(mercenary, slot.transform.position + _sellButtonOffset);
    }

    private void Start()
    {
        _rangeIndicator.Hide();
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Vector2 worldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, _clickableLayer);

        if (hit.collider == null || !hit.collider.TryGetComponent<IClickableEntity>(out var entity))
        {
            _entityInfoUI.Hide();
            _rangeIndicator.Hide();
            DependencyInjector.Get<MergeManager>().CheckHide();
            DependencyInjector.Get<CharacterSellManager>().Hide();
            return;
        }

        // 타입별 Show는 아래 분기에서 처리

        if (hit.collider.TryGetComponent<Chicken>(out var chicken))
        {
            _entityInfoUI.ShowChicken(chicken.InfoData, chicken.Data.grade, chicken.Level);
            chicken.TryLayEgg();
            DependencyInjector.Get<MergeManager>().CheckChickenMerge(chicken, chicken.transform.position + _mergeButtonOffset);
            DependencyInjector.Get<CharacterSellManager>().ShowChickenSell(chicken, chicken.transform.position + _sellButtonOffset);
            _rangeIndicator.Hide();
            return;
        }

        if (hit.collider.TryGetComponent<MercenaryBase>(out var mercenary) && mercenary.Slot != null)
        {
            _entityInfoUI.ShowMercenary(mercenary.InfoData, mercenary.Data, mercenary.Data.skills);
            _rangeIndicator.Show(mercenary.Slot.transform.position, mercenary.Data.attackRange);
            DependencyInjector.Get<MergeManager>().CheckMercenaryMerge(mercenary, mercenary.Slot.transform.position + _mergeButtonOffset);
            DependencyInjector.Get<CharacterSellManager>().ShowMercenarySell(mercenary, mercenary.Slot.transform.position + _sellButtonOffset);
            return;
        }

        if (hit.collider.TryGetComponent<EnemyInstance>(out var enemy))
        {
            _entityInfoUI.ShowEnemy(enemy.InfoData, enemy.StatData);
            _rangeIndicator.Hide();
            DependencyInjector.Get<MergeManager>().CheckHide();
            DependencyInjector.Get<CharacterSellManager>().Hide();
            return;
        }

        _entityInfoUI.Show(entity.InfoData);
        _rangeIndicator.Hide();
        DependencyInjector.Get<MergeManager>().CheckHide();
        DependencyInjector.Get<CharacterSellManager>().Hide();
    }
}