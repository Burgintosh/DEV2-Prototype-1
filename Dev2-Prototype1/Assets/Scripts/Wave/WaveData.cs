using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpawnEntryData
{
    public GameObject enemyPrefab;
    public int count = 1;
}

[System.Serializable]
public class WaveGroupData
{
    public string groupName = "New Group";
    public Transform spawnCenterPos;
    public float spawnAreaSize = 0f;
    public float startdelay = 0f;
    public float spawnInterval = 1f;
    public List<SpawnEntryData> spawnEntries = new List<SpawnEntryData>();
}

[System.Serializable]
public class WaveData
{

    public int clearReward = 150;
    public List<WaveGroupData> groups = new List<WaveGroupData>();
}
