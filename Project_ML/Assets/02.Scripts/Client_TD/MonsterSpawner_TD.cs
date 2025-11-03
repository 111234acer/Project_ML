using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner_TD : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    private float nextSpawnTime = 0f;

    [Header("Monster Prefabs")]
    public List<GameObject> monsterPrefabs;
    public Transform[] spawnPoints;

    [Header("Pooling")]
    [Tooltip("각 프리팹마다 미리 만들어둘 개수")]
    public int preloadCount = 5;

    // 프리팹별 풀
    Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        // 프리팹마다 큐 만들어서 미리 담아두기
        foreach (var prefab in monsterPrefabs)
        {
            var q = new Queue<GameObject>();
            _pools.Add(prefab, q);

            for (int i = 0; i < preloadCount; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);

                // 몬스터가 “어디로 돌려줘야 하는지” 알 수 있게
                var m = obj.GetComponent<Monster_TD>();
                if (m != null)
                    m.OnSpawnedFromPool(this);

                q.Enqueue(obj);
            }
        }
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnMonster();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnMonster()
    {
        if (monsterPrefabs == null || monsterPrefabs.Count == 0) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        GameObject prefab = monsterPrefabs[Random.Range(0, monsterPrefabs.Count)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject monster = GetFromPool(prefab);
        if (monster == null)
        {
            // 풀에 없으면 최후의 수단으로 하나 만든다
            monster = Instantiate(prefab);
            var m = monster.GetComponent<Monster_TD>();
            if (m != null)
                m.OnSpawnedFromPool(this);
        }

        monster.transform.position = spawnPoint.position;
        monster.transform.rotation = spawnPoint.rotation;

        // 리셋 후 켜기
        monster.SetActive(true);

        // 몬스터가 타워를 다시 찾아가도록 하고 싶으면 여기서도 한 번 리셋해줄 수 있음
        var mon = monster.GetComponent<Monster_TD>();
        if (mon != null)
            mon.ResetMonster(); // 아래 Monster_TD에 만들 거
    }

    GameObject GetFromPool(GameObject prefab)
    {
        Queue<GameObject> q;
        if (_pools.TryGetValue(prefab, out q))
        {
            if (q.Count > 0)
            {
                return q.Dequeue();
            }
        }
        return null;
    }

    // 몬스터가 죽으면서 여기로 돌아오게 할 거
    public void ReturnToPool(GameObject prefab, GameObject monsterObj)
    {
        monsterObj.SetActive(false);

        Queue<GameObject> q;
        if (_pools.TryGetValue(prefab, out q))
        {
            q.Enqueue(monsterObj);
        }
        else
        {
            // 혹시 없는 프리팹이면 새로 만들어서라도 넣어둔다
            var newQ = new Queue<GameObject>();
            newQ.Enqueue(monsterObj);
            _pools.Add(prefab, newQ);
        }
    }
}