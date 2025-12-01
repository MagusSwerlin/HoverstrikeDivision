#region

using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

#endregion

public class Player : MonoBehaviour
{
    [Header("Movement")]
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

    [Header("Weapons")]
    [Tooltip("The interval between each shot, in seconds.")]
    public float shootInterval;
    [Tooltip("The reload time in seconds, once all shots have been depleted.")]
    public float reloadTime;
    [Tooltip("The angular spread of the missiles when fired.")]
    public float missileSpread;

    [Header("Components")]
    public Transform aimDirection;
    public Transform[] hoverPoints;
    public Transform[] hoverSkates;
    public Weapon[] weapons;

    [Serializable]
    public class Weapon
    {
        public Transform transform;
        public Transform[] slots;
    }

    private Physical physical;
    private Vector2 inputVector;
    private Transform target;
    private bool canShoot;
    private Tween shootCall;
    private Tween reloadCall;

    private void Start()
    {
        //The physical component is retrieved in Start to let it initialize beforehand.
        physical = GetComponent<Physical>();

        //The weapons are parented to the aim direction during runtime, as reparenting inside the model in editor would require unpacking it.
        foreach (var weapon in weapons)
            weapon.transform.SetParent(aimDirection);

        //Ready the weapons.
        Reload();

        CameraRig.SetTarget(transform);

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        //Aiming and shooting is handled in Update for player input.
        Aiming();
        Shooting();
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

        //Aim the weapons at the camera's pitch.
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
            target = hit.transform;

            UIManager.manager.target.gameObject.SetActive(true);
            UIManager.manager.aim.gameObject.SetActive(false);
            UIManager.manager.target.transform.position = CameraRig.camera.WorldToScreenPoint(hit.transform.position);
        }
        else
        {
            target = null;

            UIManager.manager.target.gameObject.SetActive(false);
            UIManager.manager.aim.gameObject.SetActive(true);

            var aimDir = CameraRig.camera.WorldToScreenPoint(CameraRig.rig.angle.position + aimDirection.forward * 10f);

            UIManager.manager.aim.transform.position =
                new Vector3(aimDir.x, aimDir.y, UIManager.manager.aim.transform.position.z);
        }
    }

    private void Shooting()
    {
        //If the player presses R, manually reload if they aren't already doing so.
        if (Keyboard.current.rKey.wasPressedThisFrame && !reloadCall.IsActive())
        {
            //If the player is already at full ammo, don't reload.
            if (!Array.Exists(weapons[0].slots, x => x.childCount == 0))
                return;

            canShoot = false;

            shootCall.Kill();
            reloadCall = DOVirtual.DelayedCall(reloadTime, Reload);
        }

        //If there's no target, or the player is not pressing the mouse button, return.
        if (!target || !Mouse.current.leftButton.isPressed)
            return;

        //If the weapons are on interval cooldown, return.
        if (!canShoot)
            return;

        //Set the shoot interval cooldown.
        canShoot = false;

        //If there are any missile in the weapons, fire them.
        if (Array.Exists(weapons[0].slots, x => x.childCount > 0))
        {
            foreach (var weapon in weapons)
            foreach (var slot in weapon.slots)
                //If there's a missile present in the slot, initialize (fire) it.
                if (slot.childCount > 0)
                {
                    slot.GetChild(0).GetComponent<Projectile>().Init(target);
                    slot.GetChild(0).SetParent(null);

                    break;
                }

            //If the last pair of missiles have been fired, reload.
            if (!Array.Exists(weapons[0].slots, x => x.childCount > 0))
                reloadCall = DOVirtual.DelayedCall(reloadTime, Reload);
            else
                //Otherwise, continue the salvo.
                shootCall = DOVirtual.DelayedCall(shootInterval, () => { canShoot = true; });
        }
        //Otherwise, reload.
        else
        {
            reloadCall = DOVirtual.DelayedCall(reloadTime, Reload);
        }
    }

    private void Reload()
    {
        canShoot = true;

        //Missiles are instantiated into the slots of the weapons, with a relative scale set so that they aren't huge in size.

        foreach (var weapon in weapons)
        foreach (var slot in weapon.slots)
        {
            //If there's already a missile in the slot, skip it.
            if (slot.childCount > 0)
                continue;

            var missile = Instantiate(AssetLoader.GetPrefab("Missile"), slot.position, slot.rotation, slot);
            missile.transform.localScale *= 0.01f;

            //Add a random offset to the missile's initial launch direction for visual effect.

            if (missileSpread != 0)
                missile.transform.rotation *= Quaternion.Euler(new Vector3(
                    Random.Range(-missileSpread, missileSpread),
                    Random.Range(-missileSpread, missileSpread),
                    0));
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

    private void OnDrawGizmos()
    {
        foreach (var weapon in weapons)
        foreach (var slot in weapon.slots)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(slot.position, 0.1f);
        }
    }
}