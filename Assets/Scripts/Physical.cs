#region

using UnityEngine;

#endregion

public class Physical : MonoBehaviour
{
    [HideInInspector]
    public Rigidbody body;
    [HideInInspector]
    public new Collider collider;
    [HideInInspector]
    public Vector3 lastPosition;

    /// <summary>
    ///     Returns the size of any primitive collider type, otherwise 0.
    /// </summary>
    public float size =>
        collider.TryGetComponent(out BoxCollider box)
            ? collider.TryGetComponent(out CapsuleCollider capsule)
                ? Mathf.Max(capsule.height, capsule.radius)
                : Mathf.Max(box.size.x, box.size.y, box.size.z)
            : collider.TryGetComponent(out SphereCollider sphere)
                ? sphere.radius
                : 0;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        lastPosition = transform.position;
    }

    /// <summary>
    ///     Stops all motion.
    /// </summary>
    public void Stop()
    {
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
    }
}