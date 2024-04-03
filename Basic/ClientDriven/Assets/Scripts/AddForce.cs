using Unity.Netcode.Components;
using UnityEngine;

public class AddForce : MonoBehaviour
{
    [SerializeField]
    float m_ForceToAdd = 100;

    void OnTriggerStay(Collider other)
    {
        if (!enabled) return;
        if (other.gameObject.GetComponent<NetworkRigidbody>() != null)
        {
            other.gameObject.GetComponent<Rigidbody>().AddForce((other.transform.position - transform.position) * m_ForceToAdd);
        }
    }
}
