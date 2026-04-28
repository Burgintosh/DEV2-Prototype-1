using UnityEngine;
using System.Collections.Generic;

public class NexusManager : MonoBehaviour
{
    static public NexusManager nexusManagerInstance;
    public List<Nexus> nexusList;
    public int nexusCount;
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
        if(nexusCount <= 0)
        {
            gamemanager.instance.youLose();
        }
    }
}
