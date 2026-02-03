/// <summary>
/// 🔊 GlobalSoundManager 🔊
/// 
/// ✅ Singleton-based global sound system (persists across scenes)
/// 🎯 Features:
///     - One-shot sounds via `PlaySound(SoundType type)`
///     - Looping sounds via `PlayLooping(SoundType type)`
///     - Auto-stops looping with `StopLooping(SoundType type)`
/// 🐞 Logs which script & line called a sound (Editor only)
/// 
/// 🚀 Usage:
///     1️⃣ Assign AudioClips in the Inspector using the `Sounds` list:
///        - Each element pairs a `SoundType` enum (like 🏃 Run, 🏹 ShotArrow, 🎯 ArrowHit) with its AudioClip
///        - Inspector dropdown shows emoji + name for clarity
///     2️⃣ Call sounds in code using enum, e.g.:
///        `GlobalSoundManager.PlaySound(SoundType.ArrowHit);`
/// 
/// ✅ Advantages:
///     - No more guessing array indices
///     - Inspector-friendly names
///     - Fast dictionary lookup at runtime
/// </summary>

using UnityEngine;
using System.Collections.Generic;

public enum SoundType// enum just array of number 0,1,2,... but with names
{
    [InspectorName("⚡ Laser Attack")]
    laserAttack,

    [InspectorName("❌ Click Not Enough Money")]
    clickbutNotEnoughMoney,

    [InspectorName("✅ Click Enough Money")]
    clickEnoughMoney,

    [InspectorName("🔘 Button Click")]
    buttonClick,

    [InspectorName("💰 Currency Collect")]
    currencyCollect
}


[System.Serializable]
public struct SoundEntry
{
    public SoundType soundType;
    public AudioClip clip;
}

public class GlobalSoundManager : MonoBehaviour
{
    private static GlobalSoundManager instance;

    [SerializeField] private List<SoundEntry> sounds = new(); // 👈 Visible with names
    private Dictionary<SoundType, AudioClip> soundDict = new();

    private AudioSource audioSource;
    private static Dictionary<SoundType, AudioSource> loopingSources = new();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Build dictionary at runtime
        foreach (var entry in sounds)
        {
            if (!soundDict.ContainsKey(entry.soundType))
                soundDict.Add(entry.soundType, entry.clip);
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, float volume = 1f)
    {
        if (!instance.soundDict.ContainsKey(sound))
        {
            Debug.LogWarning($"⚠️ No clip assigned for {sound}");
            return;
        }

        instance.audioSource.PlayOneShot(instance.soundDict[sound], volume * instance.audioSource.volume);
    }

    /// <summary>
    /// 🎵 Play sound with random pitch (Minecraft-style fun!)
    /// </summary>
    /// <param name="sound">Sound type to play</param>
    /// <param name="minPitch">Minimum pitch (default 0.9)</param>
    /// <param name="maxPitch">Maximum pitch (default 1.1)</param>
    /// <param name="volume">Volume multiplier (default 1)</param>
    public static void PlaySoundRandomPitch(SoundType sound, float minPitch = 0.9f, float maxPitch = 1.1f, float volume = 1f)
    {
        if (!instance.soundDict.ContainsKey(sound))
        {
            Debug.LogWarning($"⚠️ No clip assigned for {sound}");
            return;
        }

        AudioSource tempSource = instance.gameObject.AddComponent<AudioSource>();
        tempSource.clip = instance.soundDict[sound];
        tempSource.volume = volume * instance.audioSource.volume;
        tempSource.pitch = Random.Range(minPitch, maxPitch);
        tempSource.Play();
        Destroy(tempSource, tempSource.clip.length / tempSource.pitch + 0.1f);
    }

    public static void PlayLooping(SoundType sound, float volume = 1f)
    {
        if (loopingSources.ContainsKey(sound) && loopingSources[sound].isPlaying)
            return;

        if (!instance.soundDict.ContainsKey(sound))
        {
            Debug.LogWarning($"⚠️ No clip assigned for {sound}");
            return;
        }

        AudioSource newSource = instance.gameObject.AddComponent<AudioSource>();
        newSource.clip = instance.soundDict[sound];
        newSource.volume = volume * instance.audioSource.volume;
        newSource.loop = true;
        newSource.Play();

        loopingSources[sound] = newSource;
    }

    public static void StopLooping(SoundType sound)
    {
        if (loopingSources.ContainsKey(sound))
        {
            loopingSources[sound].Stop();
            Destroy(loopingSources[sound]);
            loopingSources.Remove(sound);
        }
    }
}
