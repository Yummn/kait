using UnityEngine;

[DefaultExecutionOrder(-100)]
public sealed class GameAudio : MonoBehaviour
{
    private const string MusicPath = "Audio/BackgroundMusic";
    private const string MergePath = "Audio/Merge";
    private const string FirePath = "Audio/FireLoop";

    private static GameAudio instance;

    private AudioSource musicSource;
    private AudioSource effectSource;
    private AudioSource fireSource;
    private float targetFireVolume;
    private float targetFirePitch = 0.9f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) return;

        GameObject audioObject = new GameObject("Game Audio");
        instance = audioObject.AddComponent<GameAudio>();
        DontDestroyOnLoad(audioObject);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = CreateSource("Music", Resources.Load<AudioClip>(MusicPath), true, 0.2f);
        effectSource = CreateSource("Effects", null, false, 0.55f);
        fireSource = CreateSource("Combo Fire", Resources.Load<AudioClip>(FirePath), true, 0f);
        fireSource.pitch = targetFirePitch;

        if (musicSource.clip != null) musicSource.Play();
    }

    private void Update()
    {
        if (fireSource == null) return;

        fireSource.volume = Mathf.MoveTowards(
            fireSource.volume, targetFireVolume, Time.unscaledDeltaTime * 0.55f);
        fireSource.pitch = Mathf.MoveTowards(
            fireSource.pitch, targetFirePitch, Time.unscaledDeltaTime * 0.45f);

        if (targetFireVolume <= 0f && fireSource.volume <= 0.001f && fireSource.isPlaying) {
            fireSource.Stop();
        }
    }

    public static void PlayMerge(int tileNumber)
    {
        if (instance == null || instance.effectSource == null) return;

        AudioClip clip = Resources.Load<AudioClip>(MergePath);
        if (clip == null) return;

        int level = Mathf.Max(0, Mathf.RoundToInt(Mathf.Log(Mathf.Max(2, tileNumber), 2f)) - 2);
        instance.effectSource.pitch = Mathf.Clamp(0.94f + level * 0.035f, 0.94f, 1.25f);
        instance.effectSource.PlayOneShot(clip, 0.72f);
    }

    public static void PlayKaitKill(int chainKills)
    {
        if (instance == null || instance.effectSource == null) return;
        AudioClip clip = Resources.Load<AudioClip>(MergePath);
        if (clip == null) return;

        chainKills = Mathf.Clamp(chainKills, 1, 8);
        instance.effectSource.pitch = Mathf.Lerp(0.82f, 1.42f, (chainKills - 1) / 7f);
        instance.effectSource.PlayOneShot(clip, Mathf.Lerp(0.58f, 0.92f, chainKills / 8f));
    }

    public static void SetCombo(int comboCount)
    {
        if (instance == null || instance.fireSource == null) return;

        comboCount = Mathf.Clamp(comboCount, 0, 10);
        if (comboCount < 5)
        {
            instance.targetFireVolume = 0f;
            instance.targetFirePitch = 0.9f;
            return;
        }

        float intensity = Mathf.InverseLerp(5f, 10f, comboCount);
        instance.targetFireVolume = Mathf.Lerp(0.13f, 0.36f, intensity);
        instance.targetFirePitch = Mathf.Lerp(0.9f, 1.08f, intensity);

        if (instance.fireSource.clip != null && !instance.fireSource.isPlaying) {
            instance.fireSource.Play();
        }
    }

    private AudioSource CreateSource(string sourceName, AudioClip clip, bool loop, float volume)
    {
        GameObject sourceObject = new GameObject(sourceName);
        sourceObject.transform.SetParent(transform, false);

        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = loop;
        source.playOnAwake = false;
        source.volume = volume;
        source.spatialBlend = 0f;
        return source;
    }
}
