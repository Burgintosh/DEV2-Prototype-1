using UnityEngine;

public class checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && gamemanager.instance.playerSpawnPos.transform.position != transform.position)
        {
            gamemanager.instance.playerSpawnPos.transform.position = transform.position;
        }
    }
}
