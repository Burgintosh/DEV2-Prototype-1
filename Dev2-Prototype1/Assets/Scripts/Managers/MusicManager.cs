using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum MusicState
{
    Pregame,
    Gameplay,
    Victory,
    Defeat,
    Pause,
    MainMenu
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
    public float masterVol = 1;
    [Range(0, 1)]
    [SerializeField] float pauseVolMult = 0.25f;
    [SerializeField] float fadeDur = 1f;
    [SerializeField] float interruptFadeDur = 0.5f;
    // For the future maybe if we have more than one scene
    //[HideInInspector][SerializeField] bool persistBetweenScenes = false;
    [HideInInspector][SerializeField] bool persistBetweenScenes = true;

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (playOnStart)
        {
            PlayMusic(initMusicState);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (currTrack == null && playOnStart) return;

        if (scene.name == "MainMenu")
        {
            if (currTrack == null || currTrack.musicState != MusicState.MainMenu)
                PlayMusic(MusicState.MainMenu);
        }
        else
        {
            if (currTrack == null || currTrack.musicState != MusicState.Pregame)
            {
                SetPausedMusicVol(false);
                PlayMusic(MusicState.Pregame);
            }
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
        //float targetVol = GetTargetVol(_NewTrack);

        while(timer < _FadeDur)
        {
            timer += Time.unscaledDeltaTime;

            float fadeProgress = timer / _FadeDur;

            currSong.volume = Mathf.Lerp(currStartVol, 0f, fadeProgress);
            //nextSong.volume = Mathf.Lerp(0f, targetVol, fadeProgress);
            nextSong.volume = Mathf.Lerp(0f, GetTargetVol(_NewTrack), fadeProgress); // Changed to check TargetVol each time in case it changes during the fade

            yield return null;
        }

        currSong.Stop();
        currSong.clip = null;
        currSong.volume = 0f;

        //nextSong.volume = targetVol;
        nextSong.volume = GetTargetVol(_NewTrack);

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
        //IEnumerator fadeRoutine = FadeCurrVol(GetTargetVol(currTrack), fadeDur);

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
            //currSong.volume = Mathf.Lerp(initVol, _TargetVol, fadeProgress);
            currSong.volume = Mathf.Lerp(initVol, GetTargetVol(currTrack), fadeProgress); // // Changed to check TargetVol each time in case it changes during the fade

            yield return null;
        }

        //currSong.volume = _TargetVol;
        currSong.volume = GetTargetVol(currTrack);
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

    public void SetMasterVolume(float newVol)
    {
        masterVol = newVol;

        if (currSong != null && currTrack != null && fadeCoroutine == null && volCoroutine == null)
        {
            currSong.volume = GetTargetVol(currTrack);
        }
    }

}
