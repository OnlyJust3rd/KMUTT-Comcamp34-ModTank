using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public float interval;
    public Transform spawnPlatform;
    public GameObject[] itemPrefabs;

    [HideInInspector] public bool startInterval;

    private float counter = 0;

    private void Start()
    {
        startInterval = true;
    }

    private void Update()
    {
        if (!startInterval) return;

        if (counter > interval)
        {
            counter = 0;
            startInterval = false;

            int item = Random.Range(0, itemPrefabs.Length);
            Instantiate(itemPrefabs[item], spawnPlatform.position + Vector3.up * 2, Quaternion.identity); 
        }
        else counter += Time.deltaTime;
    }
}