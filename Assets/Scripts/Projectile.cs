#region

using UnityEngine;

#endregion

public class Projectile : MonoBehaviour
{
    [Tooltip("How long the projectile will exist before being destroyed.")]
    public float lifetime;
    [Tooltip("How fast the projectile travels, and turns if it has a target.")]
    public float speed;
    [Tooltip("How much damage the projectile does.")]
    public int damage;
    [Tooltip("The size in which the projectile will splash its damage as an AOE.")]
    public float splashSize;
    [Tooltip("The radius for hit detection.")]
    public float hitSize;
    [HideInInspector]
    public Transform target;

    private bool isActive;
    //The layer in which to detect hits.
    private string hitLayer = "Default";

    /// <summary>
    ///     Initializes the projectile with a layer in which to detect hits.
    /// </summary>
    /// <param name="hitLayer"></param>
    public void Init(string hitLayer)
    {
        this.hitLayer = hitLayer;

        isActive = true;

        Destroy(gameObject, lifetime);
    }

    /// <summary>
    ///     Initializes the projectile with a target.
    /// </summary>
    /// <param name="target"></param>
    public void Init(Transform target)
    {
        this.target = target;

        //Set the layer to match the target.
        Init(LayerMask.LayerToName(target.gameObject.layer));
    }

    private void Update()
    {
        if (!isActive)
            return;

        //Rotate towards the target if it exists.
        if (target)
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(target.position - transform.position), speed * Time.deltaTime);

        transform.Translate(Vector3.forward * (speed * Time.deltaTime));

        //Check for hits, and apply damage.

        var hits = Physics.OverlapSphere(transform.position, hitSize, LayerMask.GetMask(hitLayer));

        if (hits.Length > 0)
        {
            //If the splash radius is larger than the hit radius, do another overlap check for anything hit by the splash radius.
            if (splashSize > hitSize)
            {
                hits = Physics.OverlapSphere(transform.position, splashSize, LayerMask.GetMask(hitLayer));

                foreach (var hit in hits)
                    if (hit.TryGetComponent(out Health health))
                        health.Modify(-damage);
            }
            //Otherwise, just apply damage to the first hit.
            else if (hits[0].TryGetComponent(out Health health))
            {
                health.Modify(-damage);
            }

            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        //Draw the hit and splash radius for visualisation.

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hitSize);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, splashSize);
    }
}