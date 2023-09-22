using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAnimation : MonoBehaviour
{
    Vector3 startPos;
    public float animationSpeed  = 1.5f;

    public GameObject model;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        float y = Mathf.PingPong(Time.time * animationSpeed, 1);
        model.transform.position = new Vector3(startPos.x, startPos.y + y, startPos.z);
        model.transform.Rotate(Vector3.up);
    }

    private void OnDestroy()
    {
        ItemSpawner spawner = FindObjectOfType<ItemSpawner>();
        if (spawner) spawner.startInterval = true;
    }
}
