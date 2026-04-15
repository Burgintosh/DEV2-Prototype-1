using UnityEngine;
using System.Collections;

public class Nexus : MonoBehaviour, IDamage
{
   [Range(1,100)] [SerializeField] int HP;
    [SerializeField] Renderer model;
    int HPOrig;
    Color colorOrig;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = HP;
        colorOrig = model.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void takeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(flashRed());
        if(HP >= 0)
        {
            Destroy(gameObject);
        }
        Debug.Log("NexusHit");
    }
    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }
}
