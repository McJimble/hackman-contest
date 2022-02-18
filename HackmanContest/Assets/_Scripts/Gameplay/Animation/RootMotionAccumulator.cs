using UnityEngine;

/// <summary>
/// Stores root motion deltas over time, and allows extraction of the current
/// rootmotion values provided by the animator. This prevents the animator from
/// applying root motion automatically, while keeping it enabled and able to
/// be applied in any way desired.
/// </summary>
[RequireComponent(typeof(Animator))]
public class RootMotionAccumulator : MonoBehaviour
{
    [SerializeField] private bool accumulateRotation = true;

    private Vector3 accumulatedRootMotion;
    private Quaternion accumulatedRootRotation;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = true;
    }

    private void OnAnimatorMove()
    {
        accumulatedRootMotion += animator.deltaPosition;

        if (!accumulateRotation)
            return;

        accumulatedRootRotation *= animator.deltaRotation;
    }

    public Vector3 ExtractRootVelocity()
    {
        return animator.velocity;
    }

    public Vector3 ExtractRootMotion()
    {
        Vector3 extractedRootMotion = accumulatedRootMotion;
        accumulatedRootMotion = Vector3.zero;
        return extractedRootMotion;
    }

    public Quaternion ExtractRootRotation()
    {
        Quaternion extractedRootRotation = accumulatedRootRotation;
        accumulatedRootRotation = Quaternion.identity;
        return extractedRootRotation;
    }
}
