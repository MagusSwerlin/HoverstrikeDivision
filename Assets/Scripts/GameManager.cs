#region

using UnityEngine;

#endregion

public class GameManager : MonoBehaviour
{
    public static GameManager manager;

    private void Awake()
    {
        manager = this;
    }
}