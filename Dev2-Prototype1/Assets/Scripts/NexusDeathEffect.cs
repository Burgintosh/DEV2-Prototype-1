using UnityEngine;
using System.Collections;

public class NexusDeathEffect : MonoBehaviour
{
    [SerializeField] float secondsForCycle = 0.1f;
    [SerializeField] int maxSize = 10;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(EffectCycle());
    }
    IEnumerator EffectCycle()
    {
        for (int i = 0; i < maxSize; ++i)
        {
            transform.localScale += Vector3.one;
            yield return new WaitForSeconds(secondsForCycle);
        }
        Destroy(gameObject);
    }
}
