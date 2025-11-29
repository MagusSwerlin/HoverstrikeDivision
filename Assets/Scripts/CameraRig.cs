#region

using UnityEngine;

#endregion

public class CameraRig : MonoBehaviour
{
    public static CameraRig rig;
    public new static Camera camera;

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

    private void LateUpdate()
    {
        if (target)
            transform.position = target.position;
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