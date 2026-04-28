using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // This is awful, I know. Works for now with the few sound effects there are
    public static SoundManager Instance { get; private set; }

    public AudioSource shootingSound1911;
    public AudioSource shootingSoundBennelli;
    public AudioSource shootingSoundM4;
    public AudioSource reloadSound;
    public AudioSource reloadSoundBennelli;
    public AudioSource shootingSoundEmpty;
    public AudioSource enemyShootSound;
    public AudioSource playerJump1;
    public AudioSource playerJump2;
    public AudioSource playerJump3;
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
        }
        else
        {
            Instance = this;
        }
    }

    public void PlayWithRandomPitch(AudioSource audioSource, bool oneShot = true)
    {
        float minPitch = 0.9f;
        float maxPitch = 1.1f;
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        if (oneShot)
            audioSource.PlayOneShot(audioSource.clip);
        else
            audioSource.Play();
    }

}
