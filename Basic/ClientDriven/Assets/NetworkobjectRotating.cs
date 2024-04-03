using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkobjectRotating : NetworkBehaviour
{
    public float speed = 10f;
    public Vector3 axis = Vector3.up;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(axis, speed * Time.deltaTime);
    }
}
