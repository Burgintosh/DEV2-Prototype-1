using UnityEngine;
using System.Collections.Generic;

public class NexusManager : MonoBehaviour
{
    static public NexusManager nexusManagerInstance;
    public List<Nexus> nexusList;
    public int nexusCount;
    public int totalNexusHealth;
    public int currNexusHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   void Awake()
    {
        nexusManagerInstance = this;
        nexusList = new List<Nexus>();
        if(nexusList.Count > 0)
        {
            for(int i = 0; i < nexusList.Count; i++)
            {
                totalNexusHealth += nexusList[i].GetMaxHP();
            }
        }
        currNexusHealth = totalNexusHealth;
    }

    // Update is called once per frame
    void Update()
    {
        checkCurrHealth();
    }
    public void OnNexusSpawn(Nexus nexus)
    {
        nexusList.Add(nexus);
        nexusCount++;
    }
    public void OnNexusDeath()
    {
        nexusCount--;
        if(nexusCount <= 0)
        {
            gamemanager.instance.youLose();
        }
    }
    void checkCurrHealth()
    {
        currNexusHealth = 0;
        if(nexusList.Count > 0)
        {
            for (int i = 0;i < nexusList.Count; i++)
            {
                currNexusHealth += nexusList[i].GetCurrHP();
            }
        }
    }
}
