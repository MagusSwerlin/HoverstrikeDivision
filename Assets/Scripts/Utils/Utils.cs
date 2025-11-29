#region

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

#endregion

public static class Utils
{
    /// <summary>
    ///     Returns true with a 50% chance.
    /// </summary>
    /// <returns></returns>
    public static bool RollBool()
    {
        return Random.Range(0, 2) == 0;
    }

    /// <summary>
    ///     Returns true if the chance is met.
    /// </summary>
    /// <param name="chance"></param>
    /// <returns></returns>
    public static bool RollChance(int chance)
    {
        return Random.Range(1, 101) <= chance;
    }

    /// <summary>
    ///     Returns a random item from a collection.
    /// </summary>
    /// <param name="collection"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetRandomItem<T>(IEnumerable<T> collection)
    {
        var enumerable = collection.ToList();
        return enumerable.ElementAt(Random.Range(0, enumerable.Count));
    }

    /// <summary>
    ///     Splits a list into parts.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="parts"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts)
    {
        var i = 0;
        var splits = from name in list
            group name by i++ % parts
            into part
            select part.AsEnumerable();

        return splits;
    }

    /// <summary>
    ///     Recursively finds a child with a given name.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="childName"></param>
    /// <returns></returns>
    public static Transform RecursiveFindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            var found = RecursiveFindChild(child, childName);

            if (found)
                return found;
        }

        return null;
    }

    /// <summary>
    ///     Remaps a float value from one range to another.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="minScale"></param>
    /// <param name="maxScale"></param>
    /// <returns></returns>
    public static float RemapFloat(float value, float min, float max, float minScale, float maxScale)
    {
        return minScale + (value - min) / (max - min) * (maxScale - minScale);
    }

    /// <summary>
    ///     Returns a rotation that looks at a position.
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="position"></param>
    /// <param name="up"></param>
    /// <returns></returns>
    public static Quaternion LookAtPosition(Vector3 origin, Vector3 position, Vector3 up = default)
    {
        var direction = position - origin;
        return Quaternion.LookRotation(direction, up == default ? Vector3.up : up);
    }

    /// <summary>
    ///     Returns true if the direction is looking at the other within a threshold.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="other"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    public static bool GetIsLookingAt(Vector3 direction, Vector3 other, float threshold)
    {
        return Vector3.Dot(direction, other) > 1 - threshold;
    }
}