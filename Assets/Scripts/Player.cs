#region

using System;
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
    [Tooltip("How much force is applied to correct the player's tilt.")]
    public float stabilityForce;

    [Header("Components")]
    public Transform aimDirection;
    public Transform[] hoverPoints;
    public Transform[] hoverSkates;
    public Transform[] weapons;

    private Physical physical;
    private Vector2 inputVector;

    private void Start()
    {
        //The physical component is retrieved in Start to let it initialize beforehand.
        physical = GetComponent<Physical>();

        //The weapons are parented to the aim direction during runtime, as reparenting inside the model in editor would require unpacking it.
        foreach (var weapon in weapons)
            weapon.SetParent(aimDirection);

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
        //Storing an input vector based on the player's keyboard input.
        inputVector = new Vector2(
            Keyboard.current.aKey.isPressed ? -1 : Keyboard.current.dKey.isPressed ? 1 : 0,
            Keyboard.current.wKey.isPressed ? -1 : Keyboard.current.sKey.isPressed ? 1 : 0);

        //Camera aiming using the mouse's horizontal and vertical delta, and slerping the result for smoothness.

        var aimHor = Quaternion.AngleAxis(
            CameraRig.rig.transform.eulerAngles.y + Mouse.current.delta.x.ReadValue(), Vector3.up);
        var aimVer = Quaternion.AngleAxis(
            CameraRig.rig.angle.localEulerAngles.x - Mouse.current.delta.y.ReadValue(), Vector3.right);

        CameraRig.rig.transform.rotation =
            Quaternion.Slerp(CameraRig.rig.transform.rotation, aimHor, Time.deltaTime * aimSensitivity);
        CameraRig.rig.angle.localRotation =
            Quaternion.Slerp(CameraRig.rig.angle.localRotation, aimVer, Time.deltaTime * aimSensitivity);

        //Aim each weapon at the camera's pitch.
        // foreach (var weapon in weapons)
        //     weapon.rotation = Quaternion.Slerp(weapon.rotation,
        //         Quaternion.Euler(CameraRig.rig.angle.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z),
        //         Time.deltaTime * 5);

        aimDirection.rotation = Quaternion.Slerp(aimDirection.rotation,
            Quaternion.Euler(CameraRig.rig.angle.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z),
            Time.deltaTime * 5);

        //Extra juice by making the hover skates adjust with the player's movement input.
        foreach (var skate in hoverSkates)
            skate.localRotation = Quaternion.Slerp(skate.localRotation,
                Quaternion.Euler(inputVector.y * -15, 0, inputVector.x * -15), Time.deltaTime * 5);

        //Mouse and weapon aim are shown on the screen with respective cursor icons.
        //A lock-on effect is shown by sphere-casting to see if any enemies are within the aim radius.

        if (Physics.SphereCast(aimDirection.position, 5, aimDirection.forward, out var hit, 100f,
                LayerMask.GetMask("Enemy")))
        {
            UIManager.manager.target.gameObject.SetActive(true);
            UIManager.manager.aim.gameObject.SetActive(false);
            UIManager.manager.target.transform.position = CameraRig.camera.WorldToScreenPoint(hit.transform.position);
        }
        else
        {
            UIManager.manager.target.gameObject.SetActive(false);
            UIManager.manager.aim.gameObject.SetActive(true);

            var aimDir = CameraRig.camera.WorldToScreenPoint(CameraRig.rig.angle.position + aimDirection.forward * 10f);

            UIManager.manager.aim.transform.position =
                new Vector3(aimDir.x, aimDir.y, UIManager.manager.aim.transform.position.z);
        }
    }

    private void Movement()
    {
        foreach (var hoverPoint in hoverPoints)
        {
            var index = Array.IndexOf(hoverPoints, hoverPoint);

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

                //Force is modified based on the player's input to give a sense of directional adjustment.

                var forceMod = 1f;
                const float minForce = 0.8f;

                if (inputVector.y != 0)
                {
                    if (inputVector.y > 0)
                        forceMod = index < 2 ? 1 : minForce;
                    else
                        forceMod = index < 2 ? minForce : 1;
                }

                if (inputVector.x != 0)
                {
                    if (inputVector.x > 0)
                        forceMod = index % 2 == 0 ? 1 : minForce;
                    else
                        forceMod = index % 2 == 0 ? minForce : 1;
                }

                //Draw a debug ray to visualize the force.
                Debug.DrawRay(hoverPoint.position, Vector3.down * hoverDistance,
                    Color.Lerp(Color.red, Color.green, hoverDistance / dist));

                physical.body.AddForceAtPosition(Vector3.up * (hoverForce * (hoverDistance / dist) * forceMod),
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

        //Apply a counter-force to prevent the player from tilting too much.

        const float maxTilt = 45;

        if (Vector3.Angle(transform.up, Vector3.up) > maxTilt)
        {
            var axis = Vector3.Cross(transform.up, Vector3.up);
            physical.body.AddTorque(axis * stabilityForce);
        }
    }
}