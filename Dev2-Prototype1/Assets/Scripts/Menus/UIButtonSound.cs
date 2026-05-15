using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float volumeScale = 1f;

    private static AudioSource uiAudioSource;

    private void Start()
    {
        if (uiAudioSource != null || SoundManager.Instance == null) return;

        uiAudioSource = SoundManager.Instance.gameObject.AddComponent<AudioSource>();
        uiAudioSource.playOnAwake = false;
        uiAudioSource.spatialBlend = 0f;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (clickSound == null) return;

        SoundManager.Instance.PlayWithRandomPitch(uiAudioSource, clickSound, volumeScale, SoundCategory.UI);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound == null) return;

        SoundManager.Instance.PlayWithRandomPitch(uiAudioSource, hoverSound, volumeScale, SoundCategory.UI);
    }
}
