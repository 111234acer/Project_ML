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

    [Header("참조")]
    [SerializeField] private MonsterSpawner_TD spawner;
    [SerializeField] private CardManager_TD cardManager;        //  카드 매니저 연결
    [SerializeField] private SingleGameManager_TD singleGameManager; //  일시정지 제어용

    [Header("웨이브 설정")]
    public List<MonsterWaveEntry> monsterTable = new List<MonsterWaveEntry>();
    public float spawnInterval = 0.7f;
    private int currentWave = 0;
    public int cardInterval = 3;                                //  n웨이브마다 카드 선택
    private int remainingMonsters = 0;
    private bool isWaveActive = false;

    private void Awake()
    {
        spawner = FindObjectOfType<MonsterSpawner_TD>();
        if (!cardManager) cardManager = FindObjectOfType<CardManager_TD>();          //  자동 참조
        if (!singleGameManager) singleGameManager = FindObjectOfType<SingleGameManager_TD>(); // 자동 참조
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

            // 웨이브가 끝날 때까지 대기
            while (isWaveActive)
                yield return null;

            // 카드 선택 대기 상태면, 다음 웨이브는 카드 선택이 끝날 때까지 대기
            while (singleGameManager != null && singleGameManager.isPaused)
                yield return null;

            // 웨이브가 끝나고 휴식 타임 진입
            if (singleGameManager != null)
            {
                yield return singleGameManager.RestPhase();
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

            int waveSinceStart = (currentWave - entry.startWave);
            if (waveSinceStart < 0) waveSinceStart = 0;

            int spawnCount = entry.baseCount + entry.addPerWave * waveSinceStart;
            if (entry.maxPerWave > 0 && spawnCount > entry.maxPerWave)
                spawnCount = entry.maxPerWave;

            if (spawnCount > 0)
                waveSpawnList.Add((entry, spawnCount));
        }

        remainingMonsters = 0;
        foreach (var item in waveSpawnList)
            remainingMonsters += item.count;

        StartCoroutine(SpawnWaveRoutine(waveSpawnList));
    }

    IEnumerator SpawnWaveRoutine(List<(MonsterWaveEntry entry, int count)> waveSpawnList)
    {
        foreach (var item in waveSpawnList)
        {
            MonsterWaveEntry entry = item.entry;
            int count = item.count;

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
                    remainingMonsters--;
                }

                if (!(i == count - 1 && item.Equals(waveSpawnList[waveSpawnList.Count - 1])))
                    yield return new WaitForSeconds(spawnInterval);
            }
        }
    }

    IEnumerator WatchMonster(Monster_TD mon)
    {
        while (mon != null && mon.gameObject.activeSelf)
            yield return null;

        remainingMonsters--;

        if (remainingMonsters <= 0)
        {
            EndWave();
        }
    }

    void EndWave()
    {
        isWaveActive = false;
        Debug.Log($"Wave {currentWave} 끝!");

        // 카드 시스템 연동 추가
        if (cardManager != null && singleGameManager != null)
        {
            if (currentWave % cardInterval == 0)
            {
                // 카드 선택 타이밍이므로 게임을 잠시 멈춘다.
                singleGameManager.SetPause(true);

                // 카드 UI 호출
                cardManager.ShowCardSelection();

                Debug.Log($"[WaveManager_TD] Wave {currentWave} 클리어 → 카드 선택 시작");
            }
        }
    }
}
