#region

using UnityEngine;

#endregion

public class UIManager : MonoBehaviour
{
    public static UIManager manager;

    private void Awake()
    {
        manager = this;
    }
}