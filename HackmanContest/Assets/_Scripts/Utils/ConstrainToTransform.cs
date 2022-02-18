using UnityEngine;

/// <summary>
/// 
/// Simply constrains an object's position and rotation to another transform as if
/// it was the parent. This is useful for IK follow points that cannot change their
/// position in hierarchy but need to follow another object.
/// 
/// </summary>
public class ConstrainToTransform : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] private Transform followTransform;

    private Transform initialParent;

    public Transform FollowTransform
    {
        get => followTransform;
        set
        {
            followTransform = value;
            PhysicsUtils.ResetTransformLocal(transform);
        }
    }

    private void Awake()
    {
        initialParent = transform.parent;
    }

    private void Update()
    {
        if (followTransform)
        {
            transform.position = followTransform.position;
            transform.rotation = followTransform.rotation;
        }
    }

    /// <summary>
    /// Disables the constraints, and moves the object back to its original parent's position
    /// </summary>
    public void ResetToParent()
    {
        if (initialParent)
        {
            PhysicsUtils.ResetTransformLocal(transform);
        }
        followTransform = null;
    }
}
