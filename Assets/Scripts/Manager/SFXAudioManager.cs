/*using UnityEngine;

public class SFXAudioManager : MonoBehaviour
{
    public static SFXAudioManager Instance;

    [Header("Pool AudioSource")]
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private bool canGrow = true;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    // Array that doesnt let instance of AudioSource to be created in the inspector
    private AudioSource[] pool;

    private void Awake()
    {
        // Standard singleton implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Scene will not be destroyed when loading a new scene
        DontDestroyOnLoad(gameObject);

        // Initialize the pool
        CreatePool();
    }

    private void CreatePool()
    {
        pool = new AudioSource[initialPoolSize];

        for (int i = 0; i < initialPoolSize; i++)
        {
            pool[i] = CreateNewSource(i);
        }
    }

    private AudioSource CreateNewSource(int index)
    {
        // Mantein hierarchy order
        GameObject child = new GameObject($"SFXAudioSource_{index}");
        child.transform.SetParent(transform);

        // SFX usual configuration
        AudioSource src = child.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        return src;
    }

    private AudioSource GetFreeSource()
    {
        // Look for the source that is not playing
        for (int i = 0; i < pool.Length; i++)
        {
            if (!pool[i].isPlaying)
            {
                return pool[i];
            }
        }
        // If there is no free spaces create a new one
        if (canGrow)
        {
            AudioSource[] newPool = new AudioSource[pool.Length + 1];
            for (int i = 0; i < pool.Length; i++)
            {
                newPool[i] = pool[i];
            }

            AudioSource newSource = CreateNewSource(pool.Length);
            newPool[pool.Length - 1] = newSource;
            pool = newPool;
            return newSource;
        }

        // If cant create the pool
        return null;
    }

    // Play the sound effect without position spatialization (2D)
    public void PlaySFX(AudioClip clip, float volume, float pitch)
    {
        if (clip == null) return;

        src.spatialBlend = 0f;
        src.clip = clip;
        //src.volume = math.clamp(volume, 0f, 1f) * sfxVolume;
        src.volume = MathF.Clamp01(volume) * sfxVolume;
        src.pitch = pitch;
        src.Play();
    }

    // Play the sound effect without position spatialization (3D)
    public void PlaySFX(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, float minDistance = 1f, float maxDistance = 25f)
    {
        if (clip == null) return;

        AudioSource src = GetFreeSource();

        if (src == null) return;

        src.transform.position = position;
        src.spatialBlend = 1f;
        src.minDistance = minDistance;
        src.maxDistance = maxDistance;
        src.clip = clip;
        src.volume = MathF.Clamp01(volume) * sfxVolume;
        src.pitch = pitch;
        src.Play();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = MathF.Clamp01(volume);
    }

}
*/