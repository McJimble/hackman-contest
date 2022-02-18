using UnityEngine;
using Cinemachine;

[CreateAssetMenu(fileName = "New Recoil", menuName = "Custom/Recoil")]
public class WeaponRecoil : ScriptableObject
{
    public const float Y_RECOIL_FACTOR = 0.001f;
    public const float X_RECOIL_FACTOR = 0.1f;

    [Tooltip("This will be applied to the Impulse Source component on a weapon.")]
    [SerializeField] private NoiseSettings impulseNoise;
    [Space]
    [Min(0f)]
    [SerializeField] private float duration = 0.1f;
    [Min(0f)]
    [SerializeField] private float resetRecoilTime = 1.0f;
    [SerializeField] private Vector2[] recoilPattern;

    // Assign these at runtime to affect cameras during gameplay!
    private Camera affectedCamera;
    private CinemachineFreeLook cinemachineController;
    private CinemachineImpulseSource impulseSource;

    private float recoilTimeRemaining = 0f;
    private float timeUntilRecoilReset;
    private int recoilIndex;

    public float RecoilModifier { get; set; }

    private void Awake()
    {
        RecoilModifier = 1.0f;
        recoilTimeRemaining = 0f;
        recoilIndex = 0;
        timeUntilRecoilReset = resetRecoilTime;

        // Unity is weird with serialization in scriptable objects sometimes, i have 
        // no idea if this is necessary to prevent unwanted serialization.
        Clear();
    }

    public void Initialize(Camera affectedCamera, CinemachineFreeLook cinemachineController, CinemachineImpulseSource impulseSource)
    {
        this.affectedCamera = affectedCamera;
        this.cinemachineController = cinemachineController;
        this.impulseSource = impulseSource;

        impulseSource.m_ImpulseDefinition.m_RawSignal = impulseNoise;
    }

    public void Clear()
    {
        this.affectedCamera = null;
        this.cinemachineController = null;

        if (impulseSource)
            impulseSource.m_ImpulseDefinition.m_RawSignal = null;

        this.impulseSource = null;
        recoilIndex = 0;
    }

    /// <summary>
    /// Sends a cinemachine impulse to the stored impulse listener, then
    /// begins recoil on the affected camera.
    /// </summary>
    public void GenerateRecoil()
    {
        recoilTimeRemaining = duration;
        timeUntilRecoilReset = resetRecoilTime;

        if (impulseNoise)
            impulseSource.GenerateImpulse(affectedCamera.transform.forward);

        recoilIndex = (recoilIndex + 1) % recoilPattern.Length;
    }

    /// <summary>
    /// Updates the stored cinemachine camera's values with that of what the
    /// next recoil timestep dictates. Call this in an Update() function to 
    /// have it occur in real time.
    /// </summary>
    public void UpdateCamera(float recoilModifier = 1.0f)
    {
        if (recoilTimeRemaining > 0f)
        {
            cinemachineController.m_YAxis.Value -= (((recoilPattern[recoilIndex].y * Y_RECOIL_FACTOR) * Time.deltaTime) / duration) * recoilModifier;
            cinemachineController.m_XAxis.Value -= (((recoilPattern[recoilIndex].x * X_RECOIL_FACTOR) * Time.deltaTime) / duration) * recoilModifier;
        }

        if (timeUntilRecoilReset < 0f)
        {
            timeUntilRecoilReset = resetRecoilTime;
            recoilIndex = 0;
        }

        recoilTimeRemaining -= Time.deltaTime;
        timeUntilRecoilReset -= Time.deltaTime;
    }
}
