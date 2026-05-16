using UnityEngine;

public class Persistence : MonoBehaviour
{
    
    private static GameObject[] persistentObjects = new GameObject[3];
    [SerializeField] int arrayPos;
    void Awake()
    {
        if (persistentObjects[arrayPos]== null)
        {
            persistentObjects[arrayPos] = gameObject;
            DontDestroyOnLoad(gameObject);
        } else if(persistentObjects[arrayPos]!= gameObject)
        {
            Destroy(gameObject);
        }
    }

}
