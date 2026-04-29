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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnNexusSpawn(Nexus nexus)
    {
        nexusList.Add(nexus);
        nexusCount++;
    }
    public void OnNexusDeath()
    {
        nexusCount--;
        gamemanager.instance.UpdateNexusHPBar2();
        if(nexusCount <= 0)
        {
            gamemanager.instance.youLose();
        }
    }
    public void countTotalHealth()
    {
        if (nexusList.Count > 0)
        {
            for (int i = 0; i < nexusList.Count; i++)
            {
                if (nexusList[i] != null)
                {
                    totalNexusHealth += nexusList[i].GetMaxHP();
                }
            }
        }
        currNexusHealth = totalNexusHealth;
    }
    public void checkCurrHealth()
    {
        currNexusHealth = 0;
        if(nexusList.Count > 0)
        {
            for (int i = 0;i < nexusList.Count; i++)
            {
                if (nexusList[i] != null)
                {
                    currNexusHealth += nexusList[i].GetCurrHP();
                }
            }
        }
    }
}
