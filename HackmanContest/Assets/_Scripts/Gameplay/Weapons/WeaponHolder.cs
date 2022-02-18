using UnityEngine;
using Cinemachine;

/// <summary>
/// Enables an object to hold and use a weapon prefab, as well as enable
/// animation rigging, recoil, and other special usages for a weapon.
/// 
/// Current implementation is a bit redundant, but I had bigger plans for the weapon system
/// so it seemed appropriate at the time.
/// </summary>
public class WeaponHolder : MonoBehaviour
{
    [Header("IK Components")]
    [SerializeField] private ConstrainToTransform leftHandIK;
    [SerializeField] private ConstrainToTransform rightHandIK;
    [SerializeField] private ConstrainToTransform bodyRecoilIK;
    [SerializeField] private ConstrainToTransform leftArmHintIK;
    [SerializeField] private ConstrainToTransform rightArmHintIK;

    [Header("Runtime")]
    [SerializeField] private Transform aimAtTransform;
    [SerializeField] private CinemachineVirtualCamera affectCamera;
    [SerializeField] private Weapon heldWeapon;

    public Transform AimAtTransform { get => aimAtTransform; set => aimAtTransform = value; }

    public Weapon HeldWeapon
    {   
        get => heldWeapon; 
    }

    private void Awake()
    {

    }

    private void Update()
    {

    }

    /// <summary>
    /// Updates all IK constraint components connected this object to follow
    /// the proper transforms of the current weapon.
    /// </summary>
    /// <param name="enableIK">If false, disables IK constraints and their targets.</param>
    public void UpdateIKConstraints(bool enableIK)
    {
        if (!HeldWeapon) return;

        // Set IK targets; remove their target on disable.
        leftHandIK.FollowTransform = (enableIK) ? HeldWeapon.LeftHandIKTransform : null;
        rightHandIK.FollowTransform = (enableIK) ? HeldWeapon.RightHandIKTransform : null;
        bodyRecoilIK.FollowTransform = (enableIK) ? HeldWeapon.RecoilIKTransform : null;
        leftArmHintIK.FollowTransform = (enableIK) ? HeldWeapon.LeftArmHintIKTransform : null;
        rightArmHintIK.FollowTransform = (enableIK) ? HeldWeapon.RightArmHintIKTransform : null;
    }

    // These functions seem kind of redundant, but I wanted to directly connect a player character
    // to the weapon they're currently holding.

    public void InitWeaponRecoil(Camera affectedCamera, CinemachineFreeLook cinemachineController)
    {
        HeldWeapon?.EnableRecoil(affectedCamera, cinemachineController);
    }

    // Sets the recoil multiplier on the current weapon.
    public void SetRecoilFactor(float fac)
    {
        fac = Mathf.Max(0, fac);
        HeldWeapon.SetRecoilFactor(fac);
    }

    public void DisableRecoil()
    {
        HeldWeapon?.DisableRecoil();
    }

    public void SetShootAtTransform(Transform newTransform)
    {
        heldWeapon?.SetShootAtTransform(newTransform);
    }

    public void Sprint(bool enable)
    {
        heldWeapon?.Sprint(enable);
    }
}
