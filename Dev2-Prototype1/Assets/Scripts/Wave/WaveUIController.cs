using UnityEngine;
using TMPro;

public class WaveUIController : MonoBehaviour
{
    [SerializeField] TMP_Text waveText;
    [SerializeField] TMP_Text remainingEnemiesText;
    [SerializeField] TMP_Text countdownText;
    [SerializeField] TMP_Text promptText;

    public void SetWaveNumber(int _WaveNumber)
    {
        if(waveText != null)
        {
            waveText.text = _WaveNumber.ToString();
            //waveText.text = $"Wave: {_WaveNumber}";
        }
    }

    public void SetRemainingEnemies(int _Count)
    {
        if(remainingEnemiesText != null)
        {
            remainingEnemiesText.text = _Count.ToString();
            //remainingEnemiesText.text = $"Enemies Remaining: {_Count}";
        }
    }

    public void SetCountDown(float _TimeLeft)
    {
        if(countdownText != null)
        {
            countdownText.text = $"Next Wave In: {Mathf.CeilToInt(_TimeLeft)}";
        }
    }

    public void ClearCountdown()
    {
        if(countdownText != null)
        {
            countdownText.text = "";
        }
    }

    public void ShowPrompt(string _Prompt)
    {
        if(promptText != null)
        {
            promptText.text = _Prompt;
        }
    }

    public void ClearPrompt()
    {
        if(promptText != null)
        {
            promptText.text = "";
        }
    }

}
