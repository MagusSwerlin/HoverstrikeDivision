#region

using UnityEngine;

#endregion

public class UIManager : MonoBehaviour
{
    public static UIManager manager;

    [Header("UI Elements")]
    public Transform cursor;
    public Transform aim;
    public Transform target;

    private void Awake()
    {
        manager = this;
    }
}