using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkSpawner1 : NetworkBehaviour
{
    [SerializeField] GameObject Spawnedobject;
    [SerializeField] GameObject Spawner;
    [SerializeField] float SpawnRate;
    bool spawnerEnabled;
    private void FixedUpdate()
    {
        if (IsServer && !spawnerEnabled) 
        {
            InvokeRepeating(nameof(Spawn), 0f, SpawnRate);
            spawnerEnabled = true;
        } 
    }
    void Spawn()
    { 
        var instance = Instantiate(Spawnedobject,Spawner.transform.position,Spawner.transform.rotation);
        instance.GetComponent<NetworkObject>().Spawn();

    }



}
