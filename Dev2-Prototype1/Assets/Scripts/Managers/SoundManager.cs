using UnityEngine;

public enum SoundCategory
{
    Master,
    Player,
    Enemy,
    Weapon,
    UI,
    Trap
}

public class SoundManager : MonoBehaviour
{
    // This is awful, I know. Works for now with the few sound effects there are (Nice Burg! - cub)
    public static SoundManager Instance { get; private set; }

    [Header("----- Global SFX Volume -----")]
    [Range(0f, 1f)]
    [SerializeField] float masterSFXVol = 1f;
    [Range(0f, 1f)]
    [SerializeField] float playerSFXVol = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] float enemySFXVol = 0.35f;
    [Range(0f, 1f)]
    [SerializeField] float weaponSFXVol = 0.35f;
    [Range(0f, 1f)]
    [SerializeField] float uiSFXVol = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] float trapSFXVol = 0.5f;

    [Header("----- Pitch Randomization -----")]
    [SerializeField] float minPitch = 0.9f;
    [SerializeField] float maxPitch = 1.1f;

    [Header("----- Enemy Sounds -----")]
    public AudioSource enemyShootSound;
    //public AudioSource shootingSound1911;
    //public AudioSource shootingSoundBennelli;
    //public AudioSource shootingSoundM4;
    //public AudioSource reloadSound;
    //public AudioSource reloadSoundBennelli;
    //public AudioSource shootingSoundEmpty;
    [Header("----- Player Jump Sounds -----")]
    public AudioSource playerJump1;
    public AudioSource playerJump2;
    public AudioSource playerJump3;
    [Header("----- Player Hurt Sounds -----")]
    public AudioSource playerHurt1;
    public AudioSource playerHurt2;
    public AudioSource playerHurt3;
    public AudioSource playerHurt4;
    public AudioSource playerHurt5;
    public AudioSource playerHurt6;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // for old ones
    public void PlayWithRandomPitch(AudioSource _AudioSrc, AudioClip _Clip = null, bool _OneShot = true)
    {
        PlayWithRandomPitch(_AudioSrc, _Clip, 1f, SoundCategory.Master, _OneShot);
    }

    public void PlayWithRandomPitch(AudioSource _AudioSrc, float _VolumeScale, SoundCategory _SoundCategory = SoundCategory.Master, bool _OneShot = true)
    {
        PlayWithRandomPitch(_AudioSrc, null, _VolumeScale, _SoundCategory, _OneShot);
    }

    public void PlayWithRandomPitch(AudioSource audioSource, AudioClip clip, float volumeScale, SoundCategory _SoundCategory = SoundCategory.Master, bool oneShot = true)
    {
        if(audioSource == null)
        {
            Debug.LogWarning("Tried to play a sound, but AudioSource was missing.");
            return;
        }

        if(clip == null)
        {
            clip = audioSource.clip;
        }

        if(clip == null)
        {
            Debug.LogWarning("Tried to play a sound, but no AudioClip was assigned.");
            return;
        }

        audioSource.pitch = Random.Range(minPitch, maxPitch);

        float finalVol = Mathf.Clamp01(volumeScale) * GetVolContext(_SoundCategory) * masterSFXVol;

        if (oneShot)
        {
            audioSource.PlayOneShot(clip, finalVol);
        }
        else
        {
            audioSource.clip = clip;
            audioSource.volume = finalVol;
            audioSource.Play();
        }
    }

    float GetVolContext(SoundCategory _SoundContext)
    {
        switch (_SoundContext)
        {
            case SoundCategory.Player:
                return playerSFXVol;

            case SoundCategory.Enemy:
                return enemySFXVol;

            case SoundCategory.Weapon:
                return weaponSFXVol;

            case SoundCategory.UI:
                return uiSFXVol;

            case SoundCategory.Trap:
                return trapSFXVol;

            default:
                return 1f;
        }
    }
}
