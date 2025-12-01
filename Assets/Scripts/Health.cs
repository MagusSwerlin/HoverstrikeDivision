#region

using UnityEngine;

#endregion

public class Health : MonoBehaviour
{
    public delegate void OnChanged();

    //A subscription to this event will be called when the health changes.
    public event OnChanged onChanged;

    public delegate void OnZero();

    //A subscription to this event will be called when the health reaches zero.
    public event OnZero onZero;

    public int value = 100;
    [HideInInspector]
    public int maxValue;

    public void Awake()
    {
        maxValue = value;
    }

    /// <summary>
    ///     Modifies the health by an amount.
    /// </summary>
    /// <param name="amount"></param>
    public void Modify(int amount)
    {
        value += amount;

        onChanged?.Invoke();

        if (value <= 0)
        {
            onZero?.Invoke();

            Destroy(gameObject);
        }
    }
}