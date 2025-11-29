#region

using UnityEngine;

#endregion

public class CameraRig : MonoBehaviour
{
    public static CameraRig rig;
    public new static Camera camera;

    [Tooltip("How much to dampen the camera's motion by.")]
    [Range(0f, 1f)]
    public float damping;

    private Transform height;
    private Transform angle;
    private Transform target;

    private void Awake()
    {
        rig = this;

        //Grab the necessary rig components.
        height = transform.GetChild(0);
        angle = height.GetChild(0);
        camera = angle.GetChild(0).GetComponent<Camera>();
    }

    private void Update()
    {
        if (!target)
            return;

        transform.position = Vector3.Lerp(transform.position, target.position, 1 - damping);
        transform.rotation = Quaternion.Lerp(transform.rotation,
            Quaternion.Euler(transform.eulerAngles.x, target.eulerAngles.y, transform.eulerAngles.z), 1 - damping);
    }

    /// <summary>
    ///     Sets the target for the camera rig to track.
    /// </summary>
    /// <param name="target"></param>
    public static void SetTarget(Transform target)
    {
        rig.target = target;
    }
}