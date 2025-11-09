using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner_TD : MonoBehaviour
{
    public List<GameObject> monsterPrefabs;
    public Transform[] spawnPoints;
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

    //지정 생성
    public Monster_TD SpawnSpecific(GameObject prefab)
    {
        if (prefab == null) return null;
        if (spawnPoints == null || spawnPoints.Length == 0) return null;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject monsterObj = GetFromPool(prefab);
        if (monsterObj == null)
        {
            monsterObj = Instantiate(prefab);
            var mm = monsterObj.GetComponent<Monster_TD>();
            if (mm != null)
                mm.OnSpawnedFromPool(this);
        }

        monsterObj.transform.position = spawnPoint.position;
        monsterObj.transform.rotation = spawnPoint.rotation;

        var mon = monsterObj.GetComponent<Monster_TD>();
        if (mon != null)
        {
            mon.originalPrefab = prefab;
            mon.ResetMonster();
        }

        monsterObj.SetActive(true);
        return mon;
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