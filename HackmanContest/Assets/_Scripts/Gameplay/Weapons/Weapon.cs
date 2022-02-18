using UnityEngine;
using System.Collections.Generic;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif


[RequireComponent(typeof(Cinemachine.CinemachineImpulseSource))]
[RequireComponent(typeof(Animator))]
public class Weapon : MonoBehaviour
{
    [System.Serializable]
    public struct ParticleEffectAttributes
    {
        public ParticleSystem particleSystem;
        public int emitAmount;

        public void EmitSetAmount()
        {
            particleSystem.Emit(emitAmount);
        }

        public void EmitWithRotation(ref ParticleSystem.EmitParams par)
        {
            particleSystem.Emit(par, emitAmount);
        }
    }

    public static readonly string RECOIL_LAYER_NAME = "Recoil";
    public static readonly string BASE_LAYER_NAME = "Base";

    [SerializeField] private AudioClip onShootSound;

    [Header("Components")]
    [SerializeField] private Transform mainShotOrigin;

    [Header("Aim At Attributes")]
    [Range(0f, 1f)]
    [Tooltip("If 2D dot product between direction to shoot at and the gun's forward is less than this value," +
        "then the gun will NOT shoot at that object and will instead shoot at a blended direction between them (or not if specified below). Recommended value: 0.95")]
    [SerializeField] private float shootAtTreshhold = 0.95f;
    [SerializeField] private bool shouldBlendAim = true;
    [Tooltip("Ray will only cast this far; if nothing is hit it still shoots forward this amount")]
    [SerializeField] private float maxRaycastDistance = 1000f;

    [Header("Stats")]
    [Tooltip("Number of bullets this gun can output per second.")]
    [SerializeField] private bool automaticFire = false;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float baseDamage = 5f;
    [SerializeField] private WeaponRecoil recoil;

    [Header("Animation")]
    [SerializeField] private Animator weaponAnimator;
    [SerializeField] private AnimatorOverrideController animationOverride;
    [Space]
    [SerializeField] private Transform leftHandIKtransform;
    [SerializeField] private Transform rightHandIKtransform;
    [SerializeField] private Transform recoilIKTransform;
    [SerializeField] private Transform leftArmHintIKTransform;
    [SerializeField] private Transform rightArmHintIKTransform;

    [Header("Non-Instanced Effects")]
    [SerializeField] private ParticleEffectAttributes[] shootEffectPrefabs;
    [SerializeField] private ParticleEffectAttributes[] onHitEffectPrefabs;

    [Header("Instanced Effects")]
    [SerializeField] private TrailRenderer bulletTracerEffect;
    [Tooltip("Hit distance may not line up with tracer effect at long ranges. If so, lower this value until tracer looks good when hitting nothing")]
    [SerializeField] private float maxTracerDistance = 100f;

    public bool IsFiring { get; private set; }
    public bool CanFire
    {
        get => timeSinceLastFire > TimeUntilFire &&
               (automaticFire || hasStoppedFiring);
    }
    public float TimeUntilFire { get => 1.0f / fireRate; }

    public Transform LeftHandIKTransform { get => leftHandIKtransform; }
    public Transform RightHandIKTransform { get => rightHandIKtransform; }
    public Transform RecoilIKTransform { get => recoilIKTransform; }
    public Transform LeftArmHintIKTransform { get => leftArmHintIKTransform; }
    public Transform RightArmHintIKTransform { get => rightArmHintIKTransform; }

    public WeaponRecoil Recoil { get => recoil; }

    private Transform shootAtTransform;     // Gun will shoot at this position, even if it's not directly in front of it.

    private Ray fireRay;
    private RaycastHit lastHitInfo;
    private Vector3 tracerPoint;
    private ParticleSystem.EmitParams onHitEmitParams = new ParticleSystem.EmitParams();

    private float timeSinceLastFire = 0f;
    [System.NonSerialized] private float recoilFactor = 1.0f;
    private bool hasStoppedFiring = true;

    // Animation hashes; depends on lots of different attributes
    private int recoilLayerIndex;
    private int animRecoil1;
    private int baseLayerIndex;
    private int animSprint;

    private void Awake()
    {
        mainShotOrigin = (mainShotOrigin) ? mainShotOrigin : this.transform;
        weaponAnimator = (weaponAnimator) ? weaponAnimator : GetComponent<Animator>();

        weaponAnimator.runtimeAnimatorController = animationOverride;
    }

    private void Start()
    {
        // Unparent certain effects; otherwise their position will follow the player which looks bad.
        foreach (var effect in onHitEffectPrefabs)
        {
            effect.particleSystem.transform.parent = null;
        }

        GenerateAnimationHashes();
    }

    private void Update()
    {
        timeSinceLastFire += Time.deltaTime;

        recoil?.UpdateCamera(recoilFactor);
    }

    private void GenerateAnimationHashes()
    {
        StringBuilder sb = new StringBuilder();

        // Recoil Layer
        recoilLayerIndex = weaponAnimator.GetLayerIndex(RECOIL_LAYER_NAME);

        // Recoil1
        sb.Append(RECOIL_LAYER_NAME).Append(".").Append("Recoil_1");
        animRecoil1 = Animator.StringToHash(sb.ToString());
        sb.Clear();

        // Base Layer
        baseLayerIndex = weaponAnimator.GetLayerIndex(BASE_LAYER_NAME);

        // Sprint
        animSprint = Animator.StringToHash("isSprinting");
        sb.Clear();
    }

#if UNITY_EDITOR
    private List<Vector3> shotPositions = new List<Vector3>();
    private const float debugSphereTime = 0.75f;
    private float timeSinceRemove = debugSphereTime;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (shotPositions.Count == 0) return;

        foreach (var pos in shotPositions)
            Gizmos.DrawWireSphere(pos, 0.2f);

        if (timeSinceRemove < 0f)
        {
            shotPositions.RemoveAt(0);
            timeSinceRemove = debugSphereTime;
        }
        timeSinceRemove -= Time.deltaTime;

    }

#endif

    public void StartFiring()
    {
        if (!CanFire) return;

        timeSinceLastFire = 0f;
        hasStoppedFiring = false;

        FireWeapon();

        if (recoil)
        {
            recoil.GenerateRecoil();
        }

        weaponAnimator.SetBool(animSprint, false);
        weaponAnimator.CrossFadeInFixedTime(animRecoil1, 0.1f, recoilLayerIndex);
    }

    public void StopFiring()
    {
        hasStoppedFiring = true;
    }

    public void FireWeapon()
    {
        AudioSource.PlayClipAtPoint(onShootSound, transform.position, 0.5f);

        foreach (var effect in shootEffectPrefabs)
        {
            effect.EmitSetAmount();
        }

        Vector3 mainShotForward = mainShotOrigin.forward;   // Used a lot, so i cached it.
        fireRay.origin = mainShotOrigin.position;
        fireRay.direction = mainShotForward;

        // This avoid shooting way off to the side when a nearby object to the side of the weapon.
        if (shootAtTransform)
        {
            Vector3 potentialDir = (shootAtTransform.position - mainShotOrigin.position).normalized;

            float aimDot = Vector3.Dot(potentialDir, mainShotForward);
            float t = (aimDot / shootAtTreshhold) * ((shouldBlendAim) ? 1 : 0);
            fireRay.direction = Vector3.Lerp(mainShotForward, potentialDir, aimDot);

            //Debug.LogFormat("Dot: {0}, Lerp t: {1}", aimDot, t);
        }

        // TODO: Create new tracer effect if time allows. Instantiating and destroying an effect that happens
        // so much is nasty. Maybe using a single line renderer? Object pool tracers?
        var spawnedTracer = Instantiate(bulletTracerEffect, fireRay.origin, Quaternion.identity);
        spawnedTracer.AddPosition(fireRay.origin);

        if (Physics.Raycast(fireRay, out lastHitInfo, maxRaycastDistance, ~PhysicsUtils.Masks.player))
        {
            foreach (var effect in onHitEffectPrefabs)
            {
                onHitEmitParams.rotation3D = lastHitInfo.normal;
                effect.particleSystem.transform.position = lastHitInfo.point + (lastHitInfo.normal * 0.01f); // Move along normal to avoid z-fighting!
                effect.particleSystem.transform.forward = lastHitInfo.normal;
                effect.EmitWithRotation(ref onHitEmitParams);
            }

            tracerPoint = lastHitInfo.point;

            IDamageReceiver receiver;
            if (lastHitInfo.collider.TryGetComponent<IDamageReceiver>(out receiver))
            {
                receiver.ReceiveDamage(baseDamage, this);
            }

        }
        // If nothing was hit, still shoot gun but don't spawn on hit effects!
        else
        {
            tracerPoint = fireRay.origin + (fireRay.direction * maxTracerDistance);
        }

        spawnedTracer.transform.position = tracerPoint;

#if UNITY_EDITOR
         shotPositions.Add(tracerPoint);
#endif
    }
    public void Sprint(bool enableSprint)
    {
        weaponAnimator.SetBool(animSprint, enableSprint);
    }

    public void EnableRecoil(Camera affectedCamera, Cinemachine.CinemachineFreeLook cinemachineController)
    {
        recoil?.Initialize(affectedCamera, cinemachineController, GetComponent<Cinemachine.CinemachineImpulseSource>());
    }

    public void SetRecoilFactor(float fac)
    {
        recoilFactor = fac;
    }

    public void DisableRecoil()
    {
        recoil?.Clear();
    }

    public void SetShootAtTransform(Transform t)
    {
        shootAtTransform = t;
    }
}