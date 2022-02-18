using UnityEngine;

/// <summary>
/// Positions an object at the first raycast hit position from a given camera each frame.
/// </summary>
public class LockToCameraRay : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [Tooltip("Offset from lower left of camera view.")]
    [SerializeField] private Vector3 viewPortOffset = new Vector3(0.5f, 0.5f, 0);
    [Tooltip("Max raycast distance to check. If nothing detected, defaults to this distance in front of the camera.")]
    [SerializeField] private float maxRayDistance = 100f;
    [Tooltip("Minimum distance in front of camera that we can raycast to")]
    [SerializeField] private float initMinRayDistance = 0f;

    public float MinRayDistance { get; set; }

    // Cache these two for performance.
    private Ray ray;
    private RaycastHit hitInfo;
    private bool didRayHit = false;

    private void Awake()
    {
        cam = (cam) ? cam : Camera.main;

        MinRayDistance = initMinRayDistance;
    }

    private void LateUpdate()
    {
        if (!cam) return;

        ray = cam.ViewportPointToRay(viewPortOffset);

        // If object was hit, make sure it is farther than the min distance.
        didRayHit = false;
        if (Physics.Raycast(ray, out hitInfo, maxRayDistance))
        {
            if (hitInfo.distance < MinRayDistance)
            {
                RaycastHit[] hits = PhysicsUtils.RaycastAllByDistance(ray, maxRayDistance);

                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].distance < MinRayDistance) continue; 
                    
                    hitInfo.point = hits[i].point;
                    didRayHit = true;
                    break;
                }
            }
            else
            {
                didRayHit = true;
            }
        }

        this.transform.position = (didRayHit) ? hitInfo.point : (ray.origin + (ray.direction * maxRayDistance));
    }

    public void SetLockCamera(Camera cam)
    {
        this.cam = cam;
    }
}
