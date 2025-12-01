#region

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

public class AssetLoader : MonoBehaviour
{
    private static Dictionary<string, GameObject> prefabDict = new();
    private static Dictionary<string, AudioClip> soundDict = new();
    private static Dictionary<string, AudioClip> musicDict = new();

    private void Awake()
    {
        prefabDict = new Dictionary<string, GameObject>();
        soundDict = new Dictionary<string, AudioClip>();
        musicDict = new Dictionary<string, AudioClip>();

        LoadResources();
    }

    //Load in all prefabs and sounds from resources and store them in dictionaries.
    private static void LoadResources()
    {
        var prefabs = Resources.LoadAll("Prefabs", typeof(GameObject)).Cast<GameObject>().ToArray();

        foreach (var prefab in prefabs)
            if (!prefabDict.TryAdd(prefab.name, prefab))
                Debug.LogError($"Duplicate prefab found! {prefab.name}");

        var sounds = Resources.LoadAll("Audio/Sounds", typeof(AudioClip)).Cast<AudioClip>().ToArray();

        foreach (var sound in sounds)
            if (!soundDict.TryAdd(sound.name, sound))
                Debug.LogError($"Duplicate sound found! {sound.name}");

        var music = Resources.LoadAll("Audio/Music", typeof(AudioClip)).Cast<AudioClip>().ToArray();

        foreach (var song in music)
            if (!musicDict.TryAdd(song.name, song))
                Debug.LogError($"Duplicate song found! {song.name}");
    }

    /// <summary>
    ///     Returns if the prefab was found and the prefab by the name given.
    /// </summary>
    /// <param name="prefabName"></param>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public static bool TryGetPrefab(string prefabName, out GameObject prefab)
    {
        return prefabDict.TryGetValue(prefabName, out prefab);
    }

    /// <summary>
    ///     Returns the prefab by the name given.
    /// </summary>
    /// <param name="prefabName"></param>
    /// <returns></returns>
    public static GameObject GetPrefab(string prefabName)
    {
        prefabDict.TryGetValue(prefabName, out var prefab);
        return prefab;
    }

    /// <summary>
    ///     Returns if the sound was found and the sound by the name given.
    /// </summary>
    /// <param name="soundName"></param>
    /// <param name="clip"></param>
    /// <returns></returns>
    public static bool TryGetSound(string soundName, out AudioClip clip)
    {
        return soundDict.TryGetValue(soundName, out clip);
    }

    /// <summary>
    ///     Returns the sound by the name given.
    /// </summary>
    /// <param name="soundName"></param>
    /// <returns></returns>
    public static AudioClip GetSound(string soundName)
    {
        soundDict.TryGetValue(soundName, out var sound);
        return sound;
    }

    /// <summary>
    ///     Returns if the song was found and the song by the name given.
    /// </summary>
    /// <param name="songName"></param>
    /// <param name="clip"></param>
    /// <returns></returns>
    public static bool TryGetSong(string songName, out AudioClip clip)
    {
        return musicDict.TryGetValue(songName, out clip);
    }

    /// <summary>
    ///     Returns the song by the name given.
    /// </summary>
    /// <param name="songName"></param>
    /// <returns></returns>
    public static AudioClip GetSong(string songName)
    {
        musicDict.TryGetValue(songName, out var song);
        return song;
    }
}