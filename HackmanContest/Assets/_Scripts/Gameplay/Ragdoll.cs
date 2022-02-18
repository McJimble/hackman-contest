using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Disables all specified animators, rigs, and excess colliders listed
/// on this object, allows it to ragdoll if it is properly set up as one.
/// 
/// Currently does not support reversing the process.
/// Also I wrote this in like an hour due to lack of time, it's not very good
/// </summary>
public class Ragdoll : MonoBehaviour
{
    [Header("Behavior")]
    [Tooltip("When this health component dies, ragdolls automatically")]
    [SerializeField] private HealthComponent ragdollOnDeath;
    [Tooltip("Enables these when ragdoll is set active, disables when inactive")]
    [SerializeField] private Collider[] ragdollColliders;
    [SerializeField] private Transform ragdollRoot;
    [SerializeField] private Cinemachine.CinemachineFreeLook followCam;

    [Header("Components to disable")]
    [SerializeField] private Animator[] disableAnimators;
    [SerializeField] private Rig[] disableRigs;
    [SerializeField] private Collider[] disableColliders;

    [Tooltip("Unparents all of these transforms. Currently cannot re-parent them.")]
    [SerializeField] private Transform[] unparentTransforms;
    [Tooltip("Extra game objects to disable if desired.")]
    [SerializeField] private GameObject[] deactivateObjects;
    [Tooltip("Other components to disable if they screw with the ragdoll behavior somehow")]
    [SerializeField] private MonoBehaviour[] extraComponents;

    private Rigidbody rootRB;

    private void Start()
    {
        ragdollOnDeath.OnDeath += SetRagdollOffOnDeath;

        rootRB = GetComponent<Rigidbody>();
        foreach (var col in ragdollColliders)
        {
            col.enabled = false;
        }
    }

    private void SetRagdollOffOnDeath(float health, float damage, object sender)
    {
        SetRagdoll(true);

        if (rootRB && sender is MonoBehaviour)
        {
            Vector3 senderPos = (sender as MonoBehaviour).transform.position;
            Vector3 fromSender = (transform.position - senderPos);

            // Arbitrary scaling damage with force.
            rootRB.AddExplosionForce(damage * 500f, senderPos, 2 * Vector3.Distance(transform.position, senderPos));
        }
    }

    public void SetRagdoll(bool enable)
    {
        rootRB.isKinematic = enable;

        if (ragdollRoot)
            rootRB.transform.position += Vector3.up * 10;   // Removes ragdoll from floor; disabling animator puts in there...

        if (followCam)
        {
            followCam.Follow = ragdollRoot;
            followCam.LookAt = ragdollRoot;
            followCam.m_YAxis.m_InputAxisName = "";
            followCam.m_XAxis.m_InputAxisName = "";
        }

        foreach (var comp in extraComponents)
        {
            comp.enabled = !enable;
        }

        foreach (var anim in disableAnimators)
        {
            anim.enabled = !enable;
        }

        foreach (var rig in disableRigs)
        {
            rig.weight = (!enable) ? 1f : 0f;
        }

        foreach (var go in deactivateObjects)
        {
            go.SetActive(!enable);
        }

        foreach(var t in unparentTransforms)
        {
            t.parent = (enabled) ? t.parent : null;
        }

        foreach (var col in ragdollColliders)
        {
            col.enabled = enable;
        }

        foreach (var col in disableColliders)
        {
            col.enabled = !enable;
        }
    }
}
