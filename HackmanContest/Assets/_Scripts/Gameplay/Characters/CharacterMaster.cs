using UnityEngine;

/// <summary>
/// Master controller for a playable character. Handles camera, input,
/// and other high level aspects of a character.
/// </summary>
[RequireComponent(typeof(Animator))]
public class CharacterMaster : MonoBehaviour
{
    public const float MIN_AIM_AT_DIST_FROM_PLAYER = 0.25f;

    [Header("Debug")]
    [SerializeField] private bool lockCursor = true;

    [Header("Required Components")]
    [SerializeField] private Camera viewCamera;
    [SerializeField] private Cinemachine.CinemachineFreeLook cinemachineController;
    [Space]
    [SerializeField] private WeaponHolder weaponHolder;
    [Space]
    [SerializeField] private Animator animator;

    [Header("Control Options")]
    [SerializeField] private float turnSpeed = 15f;
    [SerializeField] private float maxWalkSpeed = 4f;
    [SerializeField] private float maxSprintSpeed = 6f;
    [Range(0f, 1f)]
    [SerializeField] private float moveAccelLerp = 0.3f;
    [SerializeField] private float aimLerp = 0.3f;
    [SerializeField] private float aimZoomWidth = 1.2f;

    private LockToCameraRay playerAimAtObject;

    private Vector2 movementInput;
    private Vector3 moveDirection;
    private float maxSpeedLerp = 0f;
    private float currentInputSpeed = 0f;
    private bool isSprinting = false;

    private int animatorInputIDx;
    private int animatorInputIDy;
    private int animatorInputIDsprint;
    private int animatorInputIDshoot;
    private int animatorInputIDjump;

    private Rigidbody playerRB;
    private CharacterMotor motor;
    private Cinemachine.CinemachineFollowZoom cameraZoom;
    private float cameraZoomInit;

    private HealthComponent playerHealth;

    public HealthComponent PlayerHealth { get => playerHealth; }
    public CharacterMotor PlayerMotor { get => motor; }

    private void Awake()
    {
        viewCamera = (viewCamera) ? viewCamera : Camera.main;
        weaponHolder = (weaponHolder) ? weaponHolder : GetComponentInChildren<WeaponHolder>();
        animator = (animator) ? animator : GetComponent<Animator>();
        motor = GetComponent<CharacterMotor>();
        playerHealth = GetComponent<HealthComponent>();

        cameraZoom = cinemachineController.GetComponent<Cinemachine.CinemachineFollowZoom>();
        cameraZoomInit = cameraZoom.m_Width;

        animatorInputIDx = Animator.StringToHash("inputX");
        animatorInputIDy = Animator.StringToHash("inputY");

        animatorInputIDsprint = Animator.StringToHash("isSprinting");
        animatorInputIDshoot = Animator.StringToHash("shotFired");
        animatorInputIDjump = Animator.StringToHash("isJumping");

        motor.onMotorGrounded += PlayerGrounded;
        motor.onMotorUngrounded += PlayerUngrounded;

        if (lockCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        currentInputSpeed = maxWalkSpeed;
    }

    private void Start()
    {
        // Create object on camera 
        playerAimAtObject = new GameObject("Player Aim At").AddComponent<LockToCameraRay>();
        playerAimAtObject.SetLockCamera(viewCamera);
        playerAimAtObject.transform.parent = this.transform;

        // TODO: If multiple weapons + swapping added, call all this on weapon swap
        // and make it more dynamic
        if (weaponHolder)
        {
            weaponHolder.SetShootAtTransform(playerAimAtObject.transform);
            weaponHolder.InitWeaponRecoil(viewCamera, cinemachineController);
            weaponHolder.UpdateIKConstraints(true);
        }
    }

    private void Update()
    {
        PollInput();

        playerAimAtObject.MinRayDistance = Vector3.Distance(viewCamera.transform.position, transform.position) + MIN_AIM_AT_DIST_FROM_PLAYER;

        UpdateAnimation();
    }

    private void LateUpdate()
    {
        if (cameraZoom)
        {
            float targetWidth = cameraZoomInit;
            if (Input.GetMouseButton(1))
            {
                targetWidth = aimZoomWidth;
                SetSprint(false);
            }

            cameraZoom.m_Width = Mathf.Lerp(cameraZoom.m_Width, targetWidth, aimLerp);
            weaponHolder?.SetRecoilFactor(targetWidth / cameraZoomInit);
        }

        // Make game object rotate in same direction as camera.
        float cameraYaw = viewCamera.transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, cameraYaw, 0), turnSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        // Barebones movement code that will likely change later.
        moveDirection = Vector3.zero;
        moveDirection += viewCamera.transform.forward * movementInput.y;
        moveDirection += viewCamera.transform.right * movementInput.x;
        moveDirection.y = 0;

        if (moveDirection != Vector3.zero)
        {
            maxSpeedLerp = Mathf.Clamp01(maxSpeedLerp + moveAccelLerp);
            moveDirection = moveDirection.normalized * currentInputSpeed * maxSpeedLerp;
        }
        else
        {
            maxSpeedLerp = 0f;
        }

        motor.SetMoveVelocity(moveDirection);
    }

    private void PollInput()
    {
        // Get input (TODO update to new input system)
        movementInput.x = Input.GetAxis("Horizontal");
        movementInput.y = Input.GetAxis("Vertical");

        if (Input.GetButton("Fire1"))
        {
            weaponHolder.HeldWeapon.StartFiring();
            animator.SetTrigger(animatorInputIDshoot);
            animator.SetBool(animatorInputIDsprint, false);
            isSprinting = false;
        }
        if (Input.GetButtonUp("Fire1"))
        {
            weaponHolder.HeldWeapon.StopFiring();
        }

        if (movementInput.y <= 0f)
            SetSprint(false);
        else if (Input.GetKeyDown(KeyCode.LeftShift) && !Input.GetMouseButton(1))
        {
            ToggleSprint();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            motor.StartJump();
        }
    }

    private void ToggleSprint()
    {
        isSprinting = !isSprinting;
        SetSprint(isSprinting);
    }

    private void SetSprint(bool enable)
    {
        isSprinting = enable;
        currentInputSpeed = (isSprinting) ? maxSprintSpeed : maxWalkSpeed;
        animator.SetBool(animatorInputIDsprint, enable);
        weaponHolder.Sprint(enable);
    }

    private void PlayerGrounded()
    {
        animator.SetBool(animatorInputIDjump, false);
    }

    private void PlayerUngrounded()
    {
        animator.SetBool(animatorInputIDjump, true);
    }

    private void UpdateAnimation()
    {
        // Set respective animator floats
        animator.SetFloat(animatorInputIDx, movementInput.x);
        animator.SetFloat(animatorInputIDy, movementInput.y);
    }
}
