using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;

public class WaveManager : MonoBehaviour
{

    [SerializeField] List<WaveData> waves = new List<WaveData>();
    [SerializeField] ObjectPoolManager poolManager;
    [SerializeField] WaveUIController waveUI;

    [SerializeField] KeyCode startWaveKey = KeyCode.J;
    [SerializeField] float timeBetweenWaves = 10f;
    [SerializeField] bool allowEarlyWaveStart = true;
    [SerializeField] bool showDebugLogs = true;

    int currentWaveIndex = -1;
    int activeEnemyCount;
    int remainingToSpawnCount;
    int activeGroupCount;

    bool waitingForFirstWaveStart = true;
    bool waitingForNextWave;
    bool skipCountdownRequested;

    private void Start()
    {
        ValidateSetupAsStartup();
        UpdateUI();
        ShowInitialPrompt();
    }

    private void Update()
    {
        if(waitingForFirstWaveStart && Input.GetKeyDown(startWaveKey))
        {
            waitingForFirstWaveStart = false;
            StartNextWave();
            return;
        }

        if(waitingForNextWave && allowEarlyWaveStart && Input.GetKeyDown(startWaveKey))
        {
            skipCountdownRequested = true;
        }
    }

    void ShowInitialPrompt()
    {
        if(waveUI != null)
        {
            waveUI.ShowPrompt($"Press {startWaveKey} To Start");
            waveUI.ClearCountdown();
        }
    }

    void StartNextWave()
    {
        currentWaveIndex++;

        if(currentWaveIndex >= waves.Count)
        {
            gamemanager.instance.youWin();
            return;
        }

        StartCoroutine(StartWaveRoutine(currentWaveIndex));
    }

    IEnumerator StartWaveRoutine(int _WaveIndex)
    {
        WaveData wave = waves[_WaveIndex];

        if (!ValidateWave(wave))
        {
            LogWarning($"Wave {_WaveIndex + 1} failed to verify. Stopping wave progression");
            yield break;
        }

        waitingForNextWave = false;
        skipCountdownRequested = false;
        activeEnemyCount = 0;
        activeGroupCount = wave.groups.Count;
        remainingToSpawnCount = CountRemainingEnemies(wave);

        if(waveUI != null)
        {
            waveUI.SetWaveNumber(_WaveIndex + 1);
            waveUI.ClearPrompt();
            waveUI.ClearCountdown();
            waveUI.SetRemainingEnemies(RemainingEnemiesDisplayValue());
        }

        foreach(WaveGroupData currGroup in wave.groups)
        {
            StartCoroutine(RunGroup(currGroup));
        }

        while(activeGroupCount > 0 || activeEnemyCount > 0 || remainingToSpawnCount > 0)
        {
            UpdateUI();
            yield return null;
        }

        gamemanager.instance.currencyManager.AddCurrency(wave.clearReward);

        if(currentWaveIndex >= waves.Count - 1)
        {
            gamemanager.instance.youWin();
            yield break;
        }

        StartCoroutine(NextWaveCountDown());
    }

    IEnumerator RunGroup(WaveGroupData _Group)
    {
        if(_Group.startdelay > 0f)
        {
            yield return new WaitForSeconds(_Group.startdelay);
        }

        foreach(SpawnEntryData currEntry in _Group.spawnEntries)
        {
            for(int i = 0; i < currEntry.count; i++)
            {

                Vector3 spawnPos = GetSpawnPosition(_Group);
                PooledObject pooledObject = poolManager.GetFromPool(currEntry.enemyPrefab, spawnPos, Quaternion.identity);

                if(pooledObject == null)
                {
                    LogWarning($"Failed to spawn from group {_Group.groupName}");
                    activeGroupCount--;
                    yield break;
                }

                PooledEnemy currEnemy = pooledObject as PooledEnemy;

                if(currEnemy == null)
                {
                    LogWarning($"Spawned pooled object is not a PooledEnemy");
                    pooledObject.ReturnToPool();
                    activeGroupCount--;
                    yield break;
                }

                currEnemy.OnSpawned(this);
                activeEnemyCount++;
                remainingToSpawnCount--;
                UpdateUI();

                if(_Group.spawnInterval > 0f)
                {
                    yield return new WaitForSeconds(_Group.spawnInterval);
                }
            }
        }

        activeGroupCount--;
    }

    Vector3 GetSpawnPosition(WaveGroupData _Group)
    {
        Vector3 center = _Group.spawnCenterPos.position;

        if(_Group.spawnAreaSize <= 0f)
        {
            return center;
        }

        float offsetX = Random.Range(-_Group.spawnAreaSize, _Group.spawnAreaSize);
        float offsetZ = Random.Range(-_Group.spawnAreaSize, _Group.spawnAreaSize);

        return new Vector3(center.x + offsetX, center.y, center.z + offsetZ);
    }

    IEnumerator NextWaveCountDown()
    {
        waitingForNextWave = true;
        float timer = timeBetweenWaves;

        if(waveUI != null)
        {
            waveUI.ShowPrompt($"Press {startWaveKey} To Start Next Wave");
        }

        while(timer > 0f)
        {
            if (skipCountdownRequested)
            {
                break;
            }

            if(waveUI != null)
            {
                waveUI.SetCountDown(timer);
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        waitingForNextWave = false;
        skipCountdownRequested = false;

        if (waveUI != null)
        {
            waveUI.ClearCountdown();
            waveUI.ClearPrompt();
        }

        StartNextWave();
    }

    public void NotifyEnemyRemoved(PooledEnemy _Enemy)
    {
        activeEnemyCount = Mathf.Max(0, activeEnemyCount - 1);
        UpdateUI();
    }

    int CountRemainingEnemies(WaveData _Wave)
    {
        int total = 0;
        
        foreach(WaveGroupData currGroup in _Wave.groups)
        {
            foreach(SpawnEntryData currEntry in currGroup.spawnEntries)
            {
                total += Mathf.Max(0, currEntry.count);
            }
        }

        return total;
    }

    int RemainingEnemiesDisplayValue()
    {
        return activeEnemyCount + remainingToSpawnCount;
    }

    void UpdateUI()
    {
        if (waveUI == null)
        {
            return;
        }

        if(currentWaveIndex >= 0)
        {
            waveUI.SetWaveNumber(currentWaveIndex + 1);
        }

        waveUI.SetRemainingEnemies(RemainingEnemiesDisplayValue());
    }

    void ValidateSetupAsStartup()
    {
        if(poolManager == null)
        {
            LogWarning("Pool Manager is not assigned");
        }

        if(waves == null || waves.Count == 0)
        {
            LogWarning("No waves are configured");
        }

    }

    bool ValidateWave(WaveData _Wave)
    {
        if(waves == null)
        {
            LogWarning("Wave is null");
            return false;
        }

        if(poolManager == null)
        {
            LogWarning("Pool Manager is missing (null):");
            return false;
        }

        if(_Wave.groups == null || _Wave.groups.Count == 0)
        {
            LogWarning("Wave has no groups");
            return false;
        }

        foreach(WaveGroupData currGroup in _Wave.groups)
        {
            if(currGroup.spawnCenterPos == null)
            {
                LogWarning("Group is missing a spawn center pos");
                return false;
            }

            if(currGroup.spawnInterval < 0f)
            {
                LogWarning("Group has invalid spawn interval");
                return false;
            }

            if(currGroup.spawnEntries == null || currGroup.spawnEntries.Count == 0)
            {
                LogWarning("Group has no spawn entries");
                return false;
            }

            foreach(SpawnEntryData currEntry in currGroup.spawnEntries)
            {
                if(currEntry.enemyPrefab == null)
                {
                    LogWarning("Group has a spawn entry with no prefab");
                    return false;
                }

                if(currEntry.count <= 0)
                {
                    LogWarning("Group has a spawn entry with count <= 0");
                    return false;
                }

                if (!poolManager.HasPoolForPrefab(currEntry.enemyPrefab))
                {
                    LogWarning("No pool exists for prefab");

                }
            }
        }

        return true;
    }

    void LogWarning(string _Msg)
    {
        if (showDebugLogs)
        {
            Debug.LogWarning($"[WaveManager] {_Msg}", this);
        }
    }

}
