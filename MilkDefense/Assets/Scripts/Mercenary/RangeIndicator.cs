using UnityEngine;

// EntityClickDetector와 같은 GameObject에 부착
public class RangeIndicator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public void Show(Vector3 position, float range)
    {
        transform.position = position;
        float diameter = range * 2f;
        transform.localScale = new Vector3(diameter, diameter, 1f);
        _spriteRenderer.enabled = true;
    }

    public void Hide()
    {
        _spriteRenderer.enabled = false;
    }
}