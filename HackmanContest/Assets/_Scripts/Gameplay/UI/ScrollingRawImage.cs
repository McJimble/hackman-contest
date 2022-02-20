using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scrolls a raw image a certain number of pixels per second. Ideal for ones that are 
/// symmetrical and desire a looping scroll effect.
/// </summary>
public class ScrollingRawImage : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private RawImage image;

    [Header("Runtime")]
    [Tooltip("Scroll Speed in pixels/sec")]
    [SerializeField] private Vector2 scrollingPerSecond;

    private Vector2 ScrollingPerSecond { get => scrollingPerSecond; set => scrollingPerSecond = value; }

    private void Awake()
    {
        image = (image) ? image : GetComponent<RawImage>();
    }

    private void Update()
    {
        image.uvRect = new Rect(image.uvRect.position + scrollingPerSecond * Time.deltaTime, image.uvRect.size);
    }
}
