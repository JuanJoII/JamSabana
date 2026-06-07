using UnityEngine;

/// <summary>
/// ScriptableObject que define la configuración de un sonido individual.
/// Crea un asset via: Audio > SoundData
/// </summary>
[CreateAssetMenu(menuName = "Audio/SoundData")]
public class SoundData : ScriptableObject
{
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(0.1f, 3f)]
    public float pitch = 1f;

    [Header("Variación Aleatoria")]
    [Range(0f, 0.5f)]
    public float randomPitchRange = 0.1f;

    [Range(0f, 0.3f)]
    public float randomVolumeRange = 0.05f;

    public bool loop = false;
}

