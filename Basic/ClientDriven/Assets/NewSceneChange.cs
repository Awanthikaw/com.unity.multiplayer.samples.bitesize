using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NewSceneChange : NetworkBehaviour
    
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            NetworkManager.SceneManager.LoadScene("Main Menu",UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}
