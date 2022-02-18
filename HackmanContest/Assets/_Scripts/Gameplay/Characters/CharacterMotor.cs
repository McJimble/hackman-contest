using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 
/// Controls character locomotion using a rigidbody
/// 
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class CharacterMotor : MonoBehaviour
{
    // Avoids grounded check passing true on character jump.
    public static readonly float JUMP_DELAY = 0.05f;
    public static readonly Vector3 GROUND_RAY_OFFSET = new Vector3(0f, -0.3f, 0);

    [Header("Grounded Checks")]
    [SerializeField] private LayerMask whatIsGroundMask;
    [Tooltip("When grounded, physic material will use these properties. Determines behavior in edge cases like" +
        "standing on the tip of a ledge where a capsule may slip without friction")]
    [SerializeField] private PhysicMaterial groundedPhysicMaterial;
    [SerializeField] private float groundCheckHeight = 0.2f;
    [SerializeField] private float pullToGroundHeight = 0.7f;
    [SerializeField] private float pullToGroundForce = 50f;
    [Range(0f, 1f)]
    [SerializeField] private float stepDisplaceLerp = 0.9f;

    [Header("Movement Stats")]
    [Tooltip("Time after character has become ungrounded in which they can still jump.")]
    [SerializeField] private float coyoteTime = 0.2f;
    [Tooltip("Buffer time where jump attempts still try to go through while jumping conditions are not currently met")]
    [SerializeField] private float jumpBufferTime = 0.2f;
    [SerializeField] private float jumpSpeed = 5f;
    [Tooltip("Normal movement is multiplied by this amount when airborne.")]
    [Min(0f)]
    [SerializeField] private float airControlFactor = 0.6f;

    [Header("Animator Properties")]
    [Tooltip("Accumulates root motion values that can be used by this controller. Root motion not" +
        "used if this reference is not set")]
    [SerializeField] private RootMotionAccumulator rootMotion;
    [SerializeField] private bool applyRootMotionGrounded = false;
    [Space]

    private Vector3 moveVelocity;
    private Vector3 displacementThisFrame;
    private Vector3 groundedBoxCastSize;
    private Ray groundedRay;
    private RaycastHit groundedHitInfo;
    private RaycastHit pullToGroundHitInfo;
    private float jumpBufferTimeRemaining = 0f;
    private float jumpDelayTimeRemaining = 0f;
    private float timeAirborne = 0f;
    private bool isGrounded = false;
    private bool isGroundedLastFrame = false;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private PhysicMaterial physMat;
    private float initDynamicFriction;

    private int rootMotionGroundedFac;

    public bool IsGroundedCoyote { get => timeAirborne < coyoteTime; }
    public bool IsJumpingBuffered { get => jumpBufferTimeRemaining > 0f; }
    public bool IsJumping { get => jumpDelayTimeRemaining > 0f || rb.velocity.y > 0f || displacementThisFrame.y > 0f; }
    public float JumpSpeed { get => jumpSpeed; set => jumpSpeed = value; }

    public event System.Action onMotorGrounded;
    public event System.Action onMotorUngrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        physMat = (groundedPhysicMaterial) ? groundedPhysicMaterial : DefaultGroundedMaterial();
        initDynamicFriction = physMat.dynamicFriction;
        capsule.material = physMat;

        rootMotionGroundedFac = (applyRootMotionGrounded && rootMotion) ? 1 : 0;

        groundedBoxCastSize = new Vector3(capsule.radius, groundCheckHeight, capsule.radius);

        onMotorGrounded += ResetRBVerticalVelocity;
    }
#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
       /* Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(PhysicsUtils.GetCapsuleBottomWorld(capsule), 0.3f);*/
        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundedRay.origin, groundedRay.origin + (groundedRay.direction * groundCheckHeight));
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(groundedRay.origin, capsule.radius);
        Gizmos.DrawWireSphere(groundedHitInfo.point, 0.3f);
    }
#endif
    private void Update()
    {
        timeAirborne = (isGrounded) ? 0f : timeAirborne + Time.deltaTime;

        jumpBufferTimeRemaining -= Time.deltaTime;
        jumpDelayTimeRemaining -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        displacementThisFrame = Vector3.zero;

        displacementThisFrame += moveVelocity * (isGrounded ? 1f : airControlFactor);

        GroundedChecks();

        // Final displacement of rigidbody.
        displacementThisFrame *= Time.fixedDeltaTime;
        rb.MovePosition(transform.position + displacementThisFrame);

        //Debug.LogFormat("Grounded: {0}", isGrounded);
    }

    private void GroundedChecks()
    {
        // Notify subscribers if we have just flipped grounded state.
        if (!isGroundedLastFrame && isGrounded)
            onMotorGrounded?.Invoke();
        if (isGroundedLastFrame && !isGrounded)
            onMotorUngrounded?.Invoke();

        isGroundedLastFrame = isGrounded;

        // If currently jumping, don't check if grounded since we need initial jump to work.
        if (IsJumping) return;

        groundedRay.origin = capsule.center + transform.position + GROUND_RAY_OFFSET;
        groundedRay.direction = Vector3.down;
        float rayLength = (groundCheckHeight + capsule.height / 2) + GROUND_RAY_OFFSET.y;

        // Sphere cast check for grounded; if detected point slightly above ground check distance, add displacement vertically so the body
        // can move up steps. If this passes at all, we are grounded and dynamic friction is reactivated.
        if (Physics.SphereCast(groundedRay.origin, capsule.radius, groundedRay.direction, out groundedHitInfo, rayLength, whatIsGroundMask))
        {
            float capsuleBottomY = PhysicsUtils.GetCapsuleBottomWorld(capsule).y;
            if (groundedHitInfo.point.y > capsuleBottomY)
            {
                float yDisplacement = Mathf.Lerp(0f, (groundedHitInfo.point.y - capsuleBottomY), stepDisplaceLerp);
                displacementThisFrame.y += yDisplacement / Time.fixedDeltaTime;
            }
            timeAirborne = 0f;
            isGrounded = true;
            physMat.dynamicFriction = initDynamicFriction;
        }

        // When close to the ground, apply a downward force on the motor to prevent flying off of slopes
        // when reaching the top of them. 
        if (Physics.Raycast(groundedRay, out pullToGroundHitInfo, pullToGroundHeight, whatIsGroundMask))
        {
            rb.AddForce(0f, -pullToGroundForce, 0f, ForceMode.VelocityChange);
        }
    }

    public void SetMoveVelocity(Vector3 vel)
    {
        moveVelocity = vel;
    }

    public void StartJump()
    {
        jumpBufferTimeRemaining = jumpBufferTime;

        // Jump if input was buffered and coyote time is satisfied.
        if (IsGroundedCoyote && IsJumpingBuffered)
        {
            isGrounded = false;
            jumpBufferTimeRemaining = 0f;
            jumpDelayTimeRemaining = JUMP_DELAY;
            timeAirborne = coyoteTime;
            physMat.dynamicFriction = 0f;

            //rb.AddForce(jumpSpeed * Vector3.up, ForceMode.VelocityChange);
            Vector3 newVel = rb.velocity;
            newVel.y = jumpSpeed;
            rb.velocity = newVel;
        }
    }
    
    private void ResetRBVerticalVelocity()
    {
        Vector3 newVel = rb.velocity;
        newVel.y = 0f;
        rb.velocity = newVel;
    }

    public static PhysicMaterial DefaultGroundedMaterial()
    {
        PhysicMaterial newMat = new PhysicMaterial("Default Motor Material");
        newMat.dynamicFriction = 1.3f;
        newMat.staticFriction = 0.5f;
        newMat.bounciness = 0.0f;
        newMat.frictionCombine = PhysicMaterialCombine.Minimum;
        newMat.bounceCombine = PhysicMaterialCombine.Average;

        return newMat;
    }
}
