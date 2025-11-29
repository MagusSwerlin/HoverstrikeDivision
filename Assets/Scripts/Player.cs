#region

using UnityEngine;
using UnityEngine.InputSystem;

#endregion

public class Player : MonoBehaviour
{
    [Header("Parameters")]
    [Tooltip("How far from the ground before a hover point applies force.")]
    public float hoverDistance;
    [Tooltip("How much force is applied for hovering.")]
    public float hoverForce;
    [Tooltip("How much force is applied for moving.")]
    public float moveForce;
    [Tooltip("How much force is applied for turning.")]
    public float turnForce;

    [Space]
    public Transform[] hoverPoints;

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
    }

    private void FixedUpdate()
    {
        //Movement is handled via the fixed update to ensure it is in-sync with the physics timestep.
        Movement();
    }

    private void Aiming()
    {
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

        //Basic, rudamentary movement handling acceleration, strafing and turning.

        if (Keyboard.current.wKey.isPressed)
            physical.body.AddForce(transform.forward * moveForce);
        if (Keyboard.current.sKey.isPressed)
            physical.body.AddForce(-transform.forward * moveForce);
        if (Keyboard.current.aKey.isPressed)
            physical.body.AddForce(-transform.right * moveForce);
        if (Keyboard.current.dKey.isPressed)
            physical.body.AddForce(transform.right * moveForce);

        physical.body.AddTorque(transform.up * (Mouse.current.delta.x.ReadValue() * turnForce));
    }
}