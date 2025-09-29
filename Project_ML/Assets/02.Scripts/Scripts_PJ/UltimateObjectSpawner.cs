using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltimateObjectSpawner : MonoBehaviour
{
    public GameObject ultimatePrefab;
    public Transform spawnPoint;
    public float delaySeconds = 300f;

    private bool spawned = false;

    void Start()
    {
        StartCoroutine(SpawnAfterDelay());
    }

    IEnumerator SpawnAfterDelay()
    {
        yield return new WaitForSeconds(delaySeconds);

        if (spawned) yield break;
        if (ultimatePrefab == null) yield break;

        Vector3 pos = spawnPoint.position;
        Quaternion rot = spawnPoint.rotation;

        Instantiate(ultimatePrefab, pos, rot);
        spawned = true;
    }
}
