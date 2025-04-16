using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Tracks")]
    [SerializeField] private List<AudioClip> musicTracks;

    [Header("Sound Effects")]
    [SerializeField] private List<SoundAudioClip> soundAudioClips;

    private Dictionary<SoundType, AudioClip> soundDict;
    private int currentMusicIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        soundDict = new Dictionary<SoundType, AudioClip>();
        foreach (var entry in soundAudioClips)
        {
            if (!soundDict.ContainsKey(entry.type))
                soundDict.Add(entry.type, entry.clip);
        }
    }

    private void Start()
    {
        PlayMusic();
    }

    private void Update()
    {
        if (!musicSource.isPlaying && musicTracks.Count > 0)
        {
            currentMusicIndex = (currentMusicIndex + 1) % musicTracks.Count;
            musicSource.clip = musicTracks[currentMusicIndex];
            musicSource.Play();
        }
    }

    public void PlayMusic()
    {
        if (musicTracks.Count == 0) return;

        musicSource.clip = musicTracks[currentMusicIndex];
        musicSource.loop = false;
        musicSource.Play();
    }

    public void PlaySound(SoundType type, float volume = 1f)
    {
        if (soundDict.TryGetValue(type, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.LogWarning("Sound not found: " + type);
        }
    }
}

[System.Serializable]
public class SoundAudioClip
{
    public SoundType type;
    public AudioClip clip;
}
