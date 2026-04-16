using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // This is awful, I know. Works for now with the few sound effects there are
    public static SoundManager Instance { get; private set; }

    public AudioSource shootingSound1911;
    public AudioSource shootingSoundBennelli;
    public AudioSource shootingSoundM4;
    public AudioSource reloadSound;
    public AudioSource shootingSoundEmpty;

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

}
