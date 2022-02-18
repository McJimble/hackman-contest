using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Just another utility class I like to have in all my projects where I see fit.
/// 
/// Allows for more performant, physics calls, less GC allocations, as well as 
/// some handy things that should honestly be built into Unity but aren't
/// </summary>
public static class PhysicsUtils
{
    /// Defines all possible LayerMask instances that represent SINGLE layers or
    /// any commonly used masks that combine multiple physics layers.
    /// 
    /// Just for my convenience and super easy optimization; I make this in all my projects
    /// </summary>
    public struct Masks
    {
        // Built-in Unity ones that can't be changed
        public static readonly int defaultLayer = 1 >> LayerMask.NameToLayer("Default");
        public static readonly int transparentFX = 1 >> LayerMask.NameToLayer("TransparentFX");
        public static readonly int ignoreRaycast = 1 >> LayerMask.NameToLayer("Ignore Raycast");
        public static readonly int water = 1 >> LayerMask.NameToLayer("Water");
        public static readonly int ui = 1 >> LayerMask.NameToLayer("UI");

        // Custom single layers, from least to most significant bit they represent.
        public static readonly int projectile = 1 >> LayerMask.NameToLayer("Projectile");
        public static readonly int player = 1 >> LayerMask.NameToLayer("Player");
        public static readonly int pickup = 1 >> LayerMask.NameToLayer("Pickup");

    }

    public class RaycastHitDistanceComparerer : IComparer<RaycastHit>
    {
        public static RaycastHitDistanceComparerer Instance = new RaycastHitDistanceComparerer();

        public int Compare(RaycastHit x, RaycastHit y)
        {
            return (x.distance.CompareTo(y.distance));
        }
    }

    public const int MAX_NONALLOC_BUF = 512;

    /* 
     *  A static array to store all non alloc physics shape cast calls in.
     *  Only use this when order doesn't matter, otherwise casts cached from previous
     *  calls to non-alloc physics functions will clash!
     */
    public static Collider[] NonAllocCollisionCasts = new Collider[MAX_NONALLOC_BUF];

    /* 
     *  A static array to store all non alloc physics raycast calls in.
     *  Only use this when order doesn't matter, otherwise casts cached from previous
     *  calls to non-alloc physics functions will clash!
     */
    public static RaycastHit[] NonAllocRaycasts = new RaycastHit[MAX_NONALLOC_BUF];

    /// <summary>
    /// Performs Physics.RaycastAll, but returns the array in a sorted order by the distance the hit points were from the given position.
    /// </summary>
    /// <param name="origin">Ray origin</param>
    /// <param name="direction">Ray direction</param>
    /// <param name="maxDistance">Max distance of ray fired</param>
    /// <param name="layerMask">Physics layers to search through</param>
    /// <param name="q">Query trigger interation used by unity Physics.</param>
    /// <returns></returns>
    public static RaycastHit[] RaycastAllByDistance(Vector3 origin, Vector3 direction, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction q = QueryTriggerInteraction.UseGlobal)
    {
        Ray r = new Ray();
        return RaycastAllByDistance(r, maxDistance, layerMask, q);
    }

    /// <summary>
    /// Performs Physics.RaycastAll, but returns the array in a sorted order by the distance the hit points were from the given position.
    /// </summary>
    /// <param name="ray">Ray to fire</param>
    /// <param name="maxDistance">Max distance of ray fired</param>
    /// <param name="layerMask">Physics layers to search through</param>
    /// <param name="q">Query trigger interation used by unity Physics.</param>
    /// <returns></returns>
    public static RaycastHit[] RaycastAllByDistance(Ray ray, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction q = QueryTriggerInteraction.UseGlobal)
    {
        RaycastHit[] sortedArr = Physics.RaycastAll(ray, maxDistance, layerMask, q);

        System.Array.Sort(sortedArr, 0, sortedArr.Length, RaycastHitDistanceComparerer.Instance);

        return sortedArr;
    }

    /// <summary>
    /// Performs Physics.OverlapBoxNonAlloc, but places collider reference results in the global non-alloc array.
    /// </summary>
    /// <param name="center">Center of box</param>
    /// <param name="halfExtents">Extents of box</param>
    /// <param name="orientation">Rotation of box</param>
    /// <param name="layerMask">Layer to cast to</param>
    /// <param name="queryTriggerInteraction"></param>
    /// <returns></returns>
    public static int OverlapBoxNonAlloc(Vector3 center, Vector3 halfExtents, Quaternion orientation, int layerMask = Physics.AllLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        return Physics.OverlapBoxNonAlloc(center, halfExtents, NonAllocCollisionCasts, orientation, layerMask, queryTriggerInteraction);
    }

    /// <summary>
    /// Resets a transform back to zero rotation, position, and scale locally.
    /// </summary>
    /// <param name="reset">Transform to reset</param>
    public static void ResetTransformLocal(Transform reset)
    {
        reset.localRotation = Quaternion.identity;
        reset.localPosition = Vector3.zero;
        reset.localScale = Vector3.zero;
    }

    /// <summary>
    /// Resets a transform back to zero rotation, position, and scale in world space.
    /// </summary>
    /// <param name="reset">Transform to reset</param>
    public static void ResetTransformWorld(Transform reset)
    {
        reset.rotation = Quaternion.identity;
        reset.position = Vector3.zero;
        reset.localScale = Vector3.zero;
    }

    /// <summary>
    /// Converts a capsule collider's direction value to a world space axis
    /// from its transform.
    /// </summary>
    /// <param name="col">Capsule to get direction of</param>
    /// <returns></returns>
    public static Vector3 CapsuleDirectionToAxis(CapsuleCollider col)
    {
        switch (col.direction)
        {
            case 0:
                return col.transform.right;
            case 1:
                return col.transform.up;
            case 2:
                return col.transform.forward;
        }

        return col.transform.forward;
    }

    /// <summary>
    /// Gets the bottom position of a capsule collider in world space
    /// </summary>
    /// <param name="cap">Capsule collider to get the bottom position of</param>
    /// <returns></returns>
    public static Vector3 GetCapsuleBottomWorld(CapsuleCollider cap)
    {
        Vector3 capsuleBottomWorld = (cap.center + cap.transform.position);
        capsuleBottomWorld += (-CapsuleDirectionToAxis(cap) * (cap.height * 0.5f));

        return capsuleBottomWorld;
    }
}
