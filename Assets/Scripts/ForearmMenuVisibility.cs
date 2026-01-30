using UnityEngine;

public class ForearmMenuVisibility : MonoBehaviour
{
    [Header("References")]
    public Transform head;              // CenterEyeAnchor
    public GameObject menuVisual;       // the Canvas (child) to show/hide

    [Header("Tuning")]
    public float showAngle = 45f;
    public float hideAngle = 55f;       // hysteresis to avoid flicker

    private bool _isShown;

    void Start()
    {
        if (menuVisual != null) menuVisual.SetActive(false);
        _isShown = false;
    }

    void Update()
    {
        if (head == null || menuVisual == null) return;

        Vector3 toMenu = (transform.position - head.position).normalized;
        float angle = Vector3.Angle(head.forward, toMenu);

        if (!_isShown && angle < showAngle)
        {
            _isShown = true;
            menuVisual.SetActive(true);
        }
        else if (_isShown && angle > hideAngle)
        {
            _isShown = false;
            menuVisual.SetActive(false);
        }
    }
}