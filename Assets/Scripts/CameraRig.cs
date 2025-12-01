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

    [HideInInspector]
    public Transform height;
    [HideInInspector]
    public Transform angle;
    [HideInInspector]
    public Transform target;

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
        //If a target has been set, track it.
        if (target)
            transform.position = Vector3.Lerp(transform.position, target.position, 1 - damping);
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