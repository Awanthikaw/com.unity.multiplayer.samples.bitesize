using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NewworkPooler : NetworkBehaviour
{
    [SerializeField] GameObject Spawnedobject;
    [SerializeField] GameObject Spawner;
    [SerializeField] List< GameObject > pooledobject;
    [SerializeField] float SpawnRate;
    bool spawnerEnabled;
    private void Start()
    {
        pooledobject = new List<GameObject>();
    }
    private void FixedUpdate()
    {
        if (IsServer && !spawnerEnabled)
        {
            InvokeRepeating(nameof(Spawn), 0f, SpawnRate);
            spawnerEnabled = true;
        }
        if (pooledobject.Count>10) 
        {
            CancelInvoke(nameof(Spawn));
        }
    }
    void Spawn()
    {
        var instance = Instantiate(Spawnedobject, Spawner.transform.position, Spawner.transform.rotation);
        instance.GetComponent<NetworkObject>().Spawn();
        //instance.SetActive(false);
        pooledobject.Add(Spawnedobject);

    }



}
