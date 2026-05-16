using UnityEngine;

public class CreditsScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private RectTransform creditsRect;
    [SerializeField] private float scrollSpeed = 50f;
    [SerializeField] private float endPositionY = 2000f;

    private Vector2 startPosition;
    private bool creditsFinished = false;

    private void Awake()
    {
        if (creditsRect != null)
            startPosition = creditsRect.anchoredPosition;
    }
    private void OnDisable()
    {
        if (creditsRect != null)
            creditsRect.anchoredPosition = startPosition;
        creditsFinished = false;
    }
    private void Update()
    {
        if (creditsFinished || creditsRect == null) return;

        creditsRect.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        if (creditsRect.anchoredPosition.y >= endPositionY)
        {
            FinishCredits();
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
        {
            FinishCredits();
        }
    }

    private void FinishCredits()
    {
        creditsFinished = true;
        //gameObject.SetActive(false);
    }
}