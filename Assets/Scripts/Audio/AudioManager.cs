using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

/// <summary>
/// Global audio manager. Persists between scenes (Don't Destroy On Load).
/// 
/// ARQUITECTURE
///   - SFX  → PlaySFX / PlayRandomSFX  
///   - Music          → PlayMusic / StopMusic / FadeMusic
///   - Ambience        → PlayAmbience
///
/// </summary>
public class AudioManager : MonoBehaviour
{
    // ─── Referencias de Inspector ───────────────────────────────────────────
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambienceSource;

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup sfxGroup;

    [SerializeField] private SoundData musica;
    [SerializeField] private SoundData ambiente;
    
    [Header("Object Pool")]
    [SerializeField] private int poolSize = 20;
    private Queue<AudioSource> _pool;
    private List<AudioSource> activeBombBeeps = new List<AudioSource>();

    // ─── Singleton ───────────────────────────────────────────────────────────
    public static AudioManager Instance { get; private set; }

    // ─── Fade ────────────────────────────────────────────────────────────────
    private Coroutine _fadeCoroutine;

    // ════════════════════════════════════════════════════════════════════════
    // INICIALIZACIÓN
    // ════════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        transform.parent = null;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitPool();
    }

    private void Start()
    {
        // PlayAmbience(ambiente);
        PlayMusic(musica);
    }
    private void InitPool()
    {
        _pool = new Queue<AudioSource>(poolSize);

        for (int i = 0; i < poolSize; i++)
        {
            _pool.Enqueue(CreatePooledSource());
        }
    }

    private AudioSource CreatePooledSource()
    {
        GameObject go = new GameObject("PooledAudioSource");
        go.transform.SetParent(transform);
        DontDestroyOnLoad(go);

        AudioSource source = go.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = sfxGroup;
        source.spatialBlend = 1f;
        source.minDistance = 1f;
        source.maxDistance = 20f;

        go.SetActive(false);
        return source;
    }

    private AudioSource GetFromPool()
    {
        if (_pool.Count == 0)
        {
            Debug.LogWarning("[AudioManager] Pool vacío: creando AudioSource extra. " +
                             "Considera aumentar poolSize en el Inspector.");
            return CreatePooledSource();
        }

        AudioSource source = _pool.Dequeue();
        source.gameObject.SetActive(true);
        return source;
    }

    private void ReturnToPool(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);
        _pool.Enqueue(source);
    }

    // ════════════════════════════════════════════════════════════════════════
    // SFX 
    // ════════════════════════════════════════════════════════════════════════
    
    public void PlaySFX(SoundData data, Vector3 position)
    {
        if (data == null)
        {
            Debug.LogWarning("[AudioManager] PlaySFX: SoundData es null.");
            return;
        }
        if (data.clip == null)
        {
            Debug.LogWarning($"[AudioManager] PlaySFX: '{data.name}' no tiene AudioClip asignado.");
            return;
        }

        AudioSource source = GetFromPool();
        source.transform.position = position;
        source.clip = data.clip;

        float pitch  = data.pitch  + Random.Range(-data.randomPitchRange,  data.randomPitchRange);
        float volume = data.volume + Random.Range(-data.randomVolumeRange, data.randomVolumeRange);

        source.pitch  = Mathf.Clamp(pitch,  0.1f, 3f);
        source.volume = Mathf.Clamp01(volume);
        source.loop   = false;

        source.Play();
        
        StartCoroutine(ReturnToPoolAfterPlay(source, data.clip.length / source.pitch));
    }
    public AudioSource PlaySFXLoop(SoundData data, Vector3 position)
    {
        if (data == null || data.clip == null) return null;

        AudioSource source = GetFromPool();
        source.transform.position = position;
        source.clip = data.clip;
        source.pitch = data.pitch;
        source.volume = data.volume;
        source.loop = true;
        source.Play();

        return source; 
    }

    public void StopSFXLoop(AudioSource source)
    {
        if (source == null) return;
        ReturnToPool(source);
    }
    
    private IEnumerator ReturnToPoolAfterPlay(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay + 0.05f); // pequeño margen de seguridad
        ReturnToPool(source);
    }
    // ════════════════════════════════════════════════════════════════════════
    // Music
    // ════════════════════════════════════════════════════════════════════════

    public void PlayMusic(SoundData data)
    {
        if (musicSource == null || data == null || data.clip == null) return;

        musicSource.clip   = data.clip;
        musicSource.volume = data.volume;
        musicSource.pitch  = data.pitch;
        musicSource.loop   = data.loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
    }
    public void FadeToMusic(SoundData data, float fadeDuration = 1f)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeMusicCoroutine(data, fadeDuration));
    }

    private IEnumerator FadeMusicCoroutine(SoundData newMusic, float duration)
    {
        float startVolume = musicSource.volume;

        // Fade out
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        musicSource.Stop();
        if (newMusic != null && newMusic.clip != null)
        {
            musicSource.clip = newMusic.clip;
            musicSource.loop = newMusic.loop;
            musicSource.pitch = newMusic.pitch;
            musicSource.Play();

            // Fade in
            t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, newMusic.volume, t / duration);
                yield return null;
            }
            musicSource.volume = newMusic.volume;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // Ambience
    // ════════════════════════════════════════════════════════════════════════

    public void PlayAmbience(SoundData data)
    {
        if (ambienceSource == null || data == null || data.clip == null) return;

        ambienceSource.clip   = data.clip;
        ambienceSource.volume = data.volume;
        ambienceSource.pitch  = data.pitch;
        ambienceSource.loop   = data.loop;
        ambienceSource.Play();
    }

    public void StopAmbience()
    {
        if (ambienceSource == null) return;
        ambienceSource.Stop();
    }
}

