using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkobjectMoving : NetworkBehaviour
{
    public float speed = 10f;
    public Vector3 direction = Vector3.forward;

    // Update is called once per frame
    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }
}


