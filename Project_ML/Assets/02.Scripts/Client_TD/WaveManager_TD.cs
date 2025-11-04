using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager_TD : MonoBehaviour
{    
    [System.Serializable]
    public class MonsterWaveEntry
    {
        public GameObject prefab;

        public int startWave = 1;
        public int baseCount = 3;
        public int addPerWave = 1;
        public int maxPerWave = -1;

        public int atkBonusPerTier = 3;
        public int hpBonusPerTier = 20;
        public int tierStep = 3;
    }

    [SerializeField] private MonsterSpawner_TD spawner;
    [SerializeField] private SingleGameManager_TD gameManager;

    public List<MonsterWaveEntry> monsterTable = new List<MonsterWaveEntry>();


    public float spawnInterval = 0.7f;
    private int currentWave = 0;
    private int remainingMonsters = 0;
    private bool isWaveActive = false;

    private void Awake()
    {
        spawner = FindObjectOfType<MonsterSpawner_TD>();
        gameManager = GetComponent<SingleGameManager_TD>();
    }
    private void Start()
    {
        StartCoroutine(WaveRoutine());
    }
    IEnumerator WaveRoutine()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            StartWave();

            while (isWaveActive)
                yield return null;

            if (gameManager != null)
            {
                yield return gameManager.RestPhase();
            }
        }
    }

    void StartWave()
    {
        currentWave++;
        isWaveActive = true;

        List<(MonsterWaveEntry entry, int count)> waveSpawnList = new List<(MonsterWaveEntry, int)>();

        foreach (var entry in monsterTable)
        {
            if (entry.prefab == null) continue;
            if (currentWave < entry.startWave) continue; // 아직 등장 안 하는 몬스터

            // 이 몬스터가 등장한 이후로 몇 번째 웨이브인지
            int waveSinceStart = (currentWave - entry.startWave);
            if (waveSinceStart < 0) waveSinceStart = 0;

            int spawnCount = entry.baseCount + entry.addPerWave * waveSinceStart;

            // 웨이브 최대치 제한이 있으면 걸어준다
            if (entry.maxPerWave > 0 && spawnCount > entry.maxPerWave)
                spawnCount = entry.maxPerWave;

            if (spawnCount > 0)
            {
                waveSpawnList.Add((entry, spawnCount));
            }
        }

        remainingMonsters = 0;
        foreach (var item in waveSpawnList)
            remainingMonsters += item.count;

        // 스폰 시작
        StartCoroutine(SpawnWaveRoutine(waveSpawnList));
    }

    IEnumerator SpawnWaveRoutine(List<(MonsterWaveEntry entry, int count)> waveSpawnList)
    {
        foreach (var item in waveSpawnList)
        {
            MonsterWaveEntry entry = item.entry;
            int count = item.count;

            // 이 몬스터 전용 티어 계산
            int tier = (entry.tierStep > 0) ? (currentWave - 1) / entry.tierStep : 0;
            int addHP = tier * entry.hpBonusPerTier;
            int addATK = tier * entry.atkBonusPerTier;

            for (int i = 0; i < count; i++)
            {
                var mon = spawner.SpawnSpecific(entry.prefab);
                if (mon != null)
                {
                    mon.ApplyWaveBuff(addHP, addATK);
                    StartCoroutine(WatchMonster(mon));
                }
                else
                {
                    // 스폰 실패하면 카운트만 줄여주자
                    remainingMonsters--;
                }

                // 마지막 마리는 안기다려도 됨
                if (!(i == count - 1 && item.Equals(waveSpawnList[waveSpawnList.Count - 1])))
                    yield return new WaitForSeconds(spawnInterval);
            }
        }
    }

    IEnumerator WatchMonster(Monster_TD mon)
    {
        while (mon != null && mon.gameObject.activeSelf)
        {
            yield return null;
        }

        remainingMonsters--;
        if (remainingMonsters <= 0)
        {
            EndWave();
        }
    }

    void EndWave()
    {
        isWaveActive = false;
        // 여기서 보상 주거나 UI 업데이트 하면 됨
        Debug.Log($"Wave {currentWave} 끝!");
    }
}