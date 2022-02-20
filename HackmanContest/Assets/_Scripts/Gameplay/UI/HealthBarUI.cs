using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Enables two images to act as a sliding health bar in screen space
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;

    [Header("Required Visuals")]
    [Tooltip("Set this to false if you want one to stay still in screen space rather than following the target.")]
    [SerializeField] private bool moveWithTarget;
    [SerializeField] private RectTransform parentRect;
    [SerializeField] private Image foregroundImage;
    [SerializeField] private Image backgroundImage;

    public Transform Target { get => target; set => target = value; }
    public Vector3 Offset { get => offset; set => offset = value; }

    private void Awake()
    {

    }

    private void LateUpdate()
    {
        if (target && moveWithTarget)
        {
            Vector3 direction = (target.position - GameManager.MainCamera.transform.position).normalized;
            bool isBehind = Vector3.Dot(direction, GameManager.MainCamera.transform.forward) <= 0.0f;
            foregroundImage.enabled = !isBehind;
            backgroundImage.enabled = !isBehind;
            transform.position = GameManager.MainCamera.WorldToScreenPoint(target.position + offset);
        }
    }

    public void SetHealthBarPercentage(float percent)
    {
        Debug.Log(percent);
        float width = parentRect.rect.width * percent;
        foregroundImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }
}
