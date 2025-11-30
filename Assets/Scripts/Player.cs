#region

using UnityEngine;
using UnityEngine.InputSystem;

#endregion

public class Player : MonoBehaviour
{
    [Header("Parameters")]
    [Tooltip("How sensitive the camera is to mouse input.")]
    public float aimSensitivity;
    [Tooltip("How far from the ground before a hover point applies force.")]
    public float hoverDistance;
    [Tooltip("How much force is applied for hovering.")]
    public float hoverForce;
    [Tooltip("How much force is applied for moving.")]
    public float moveForce;
    [Tooltip("How much force is applied for turning.")]
    public float turnForce;

    [Header("Components")]
    public Transform[] hoverPoints;
    public Transform[] weapons;

    private Physical physical;

    private void Start()
    {
        //The physical component is retrieved in Start to let it initialize beforehand.
        physical = GetComponent<Physical>();

        CameraRig.SetTarget(transform);

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        //Aiming is handled in Update to ensure the camera's motion is smooth.
        Aiming();
    }

    private void FixedUpdate()
    {
        //Movement is handled via the fixed update to ensure it is in-sync with the physics timestep.
        Movement();
    }

    private void Aiming()
    {
        //Camera aiming using the mouse's horizontal and vertical delta, and slerping the result for smoothness.

        var horizontal = Quaternion.AngleAxis(
            CameraRig.rig.transform.eulerAngles.y + Mouse.current.delta.x.ReadValue(), Vector3.up);
        var vertical = Quaternion.AngleAxis(
            CameraRig.rig.angle.localEulerAngles.x - Mouse.current.delta.y.ReadValue(), Vector3.right);

        CameraRig.rig.transform.rotation =
            Quaternion.Slerp(CameraRig.rig.transform.rotation, horizontal, Time.deltaTime * aimSensitivity);
        CameraRig.rig.angle.localRotation =
            Quaternion.Slerp(CameraRig.rig.angle.localRotation, vertical, Time.deltaTime * aimSensitivity);

        foreach (var weapon in weapons)
            weapon.localRotation = Quaternion.Slerp(weapon.localRotation,
                Quaternion.Euler(CameraRig.rig.angle.localEulerAngles.x, weapon.localEulerAngles.y,
                    weapon.localEulerAngles.z), Time.deltaTime * 5);
    }

    private void Movement()
    {
        foreach (var hoverPoint in hoverPoints)
        {
            //For each hover point, a ray is cast downwards to check if the point is within the hover distance to the floor.
            //If it is, then there is an upwards force applied at the point's position.
            //The closer the ray hits the floor, the higher the force is applied to balance out the player.

            var hits = Physics.RaycastAll(hoverPoint.position, Vector3.down, hoverDistance,
                LayerMask.GetMask("Environment"));

            foreach (var hit in hits)
            {
                //Skip the iteration if this hit result is not the floor.
                if (!hit.transform.name.Contains("Floor"))
                    continue;

                var dist = Vector3.Distance(hoverPoint.position, hit.point);

                //Draw a debug ray to visualize the force.
                Debug.DrawRay(hoverPoint.position, Vector3.down * hoverDistance,
                    Color.Lerp(Color.red, Color.green, hoverDistance / dist));

                physical.body.AddForceAtPosition(Vector3.up * (hoverForce * (hoverDistance / dist)),
                    hoverPoint.position);
            }
        }

        //Controls for handling acceleration and strafing.

        if (Keyboard.current.wKey.isPressed)
            physical.body.AddForce(transform.forward * moveForce);
        if (Keyboard.current.sKey.isPressed)
            physical.body.AddForce(-transform.forward * moveForce);
        if (Keyboard.current.aKey.isPressed)
            physical.body.AddForce(-transform.right * moveForce);
        if (Keyboard.current.dKey.isPressed)
            physical.body.AddForce(transform.right * moveForce);

        //Using a signed angle determines if the player is facing left or right of the camera,
        //and then applies a torque to the player to turn.

        var sign = Vector3.SignedAngle(CameraRig.rig.transform.forward, transform.forward, Vector3.up);
        var turn = Mathf.Clamp(sign / 180, -1, 1);

        if (Mathf.Abs(turn) > 0.01f)
            physical.body.AddTorque(transform.up * (turn * turnForce * -1));
    }
}