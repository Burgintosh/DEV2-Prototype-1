using UnityEngine;
using System.Collections;

public enum MusicState
{
    Pregame,
    Gameplay,
    Victory,
    Defeat,
    Pause
}

[System.Serializable]
public class MusicTrack
{
    public MusicState musicState;
    public AudioClip clip;

    [Range(0, 1)]
    public float volume = 1;

    public bool loop = true;
}

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("----- Music Tracks -----")]
    [SerializeField] MusicTrack[] musicTracks;

    [Header("----- Misc -----")]
    [SerializeField] bool playOnStart = true;
    [SerializeField] MusicState initMusicState = MusicState.Pregame;

    [Header("----- Settings -----")]
    [Range(0, 1)]
    [SerializeField] float masterVol = 1;
    [Range(0, 1)]
    [SerializeField] float pauseVolMult = 0.25f;
    [SerializeField] float fadeDur = 1f;
    [SerializeField] float interruptFadeDur = 0.5f;
    // For the future maybe if we have more than one scene
    [HideInInspector][SerializeField] bool persistBetweenScenes = false;

    AudioSource currSong;
    AudioSource nextSong;

    MusicTrack currTrack;
    Coroutine fadeCoroutine;
    Coroutine volCoroutine;

    bool isPaused;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (persistBetweenScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        AudioSource[] songs = GetComponents<AudioSource>();

        if(songs.Length < 2)
        {
            currSong = gameObject.AddComponent<AudioSource>();
            nextSong = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            currSong = songs[0];
            nextSong = songs[1];
        }

        SetSongValues(currSong);
        SetSongValues(nextSong);
    }

    private void Start()
    {
        if (playOnStart)
        {
            PlayMusic(initMusicState);
        }
    }

    void SetSongValues(AudioSource _Src)
    {
        _Src.playOnAwake = false;
        _Src.loop = true;
        _Src.volume = 0f;
    }

    public void PlayMusic(MusicState _State)
    {
        PlayMusic(_State, false);
    }

    public void PlayMusic(MusicState _State, bool _IsInterrupt)
    {
        MusicTrack newTrack = GetTrack(_State);

        if(newTrack == null)
        {
            Debug.LogWarning("No music track found for: " + _State);
            return;
        }

        if(newTrack.clip == null)
        {
            Debug.LogWarning("Music track exists, but no AudioClip is assigned for: " + _State);
            return;
        }

        if(fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;   
        }

        if(volCoroutine != null)
        {
            StopCoroutine(volCoroutine);
            volCoroutine = null;
        }

        float fadeTime = _IsInterrupt ? interruptFadeDur : fadeDur;
        fadeCoroutine = StartCoroutine(FadeToNewTrack(newTrack, fadeTime));
    }

    IEnumerator FadeToNewTrack(MusicTrack _NewTrack, float _FadeDur)
    {
        nextSong.clip = _NewTrack.clip;
        nextSong.loop = _NewTrack.loop;
        nextSong.volume = 0f;
        nextSong.Play();

        float timer = 0f;
        float currStartVol = currSong.volume;
        float targetVol = GetTargetVol(_NewTrack);

        while(timer < _FadeDur)
        {
            timer += Time.unscaledDeltaTime;

            float fadeProgress = timer / _FadeDur;

            currSong.volume = Mathf.Lerp(currStartVol, 0f, fadeProgress);
            nextSong.volume = Mathf.Lerp(0f, targetVol, fadeProgress);

            yield return null;
        }

        currSong.Stop();
        currSong.clip = null;
        currSong.volume = 0f;

        nextSong.volume = targetVol;

        AudioSource oldCurr = currSong;
        currSong = nextSong;
        nextSong = oldCurr;

        currTrack = _NewTrack;
        fadeCoroutine = null;
    }

    public void SetPausedMusicVol(bool _Paused)
    {
        isPaused = _Paused;

        if(currTrack == null || currSong == null)
        {
            return;
        }

        if(volCoroutine != null)
        {
            StopCoroutine(volCoroutine);
            volCoroutine = null;
        }

        float targetVol = GetTargetVol(currTrack);

        IEnumerator fadeRoutine = FadeCurrVol(targetVol, fadeDur);

        volCoroutine = StartCoroutine(fadeRoutine);
    }

    IEnumerator FadeCurrVol(float _TargetVol, float _FadeLen)
    {
        float timer = 0f;
        float initVol = currSong.volume;

        while(timer < _FadeLen)
        {
            timer += Time.unscaledDeltaTime;
            float fadeProgress = timer / _FadeLen;
            currSong.volume = Mathf.Lerp(initVol, _TargetVol, fadeProgress);

            yield return null;
        }

        currSong.volume = _TargetVol;
        volCoroutine = null;
    }

    float GetTargetVol(MusicTrack _Track)
    {
        float vol = _Track.volume * masterVol;

        if (isPaused)
        {
            vol *= pauseVolMult;
        }

        return vol;
    }

    MusicTrack GetTrack(MusicState _State)
    {
        for(int i = 0; i < musicTracks.Length; i++)
        {
            if (musicTracks[i].musicState == _State)
            {
                return musicTracks[i];
            }
        }

        return null;
    }

}
