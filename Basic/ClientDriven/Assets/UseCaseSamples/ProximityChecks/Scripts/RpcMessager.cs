using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Netcode.Samples.MultiplayerUseCases.Proximity
{
	public class RpcMessager : NetworkBehaviour
	{

		ProximityChecker m_ProximityChecker;

		void Awake()
		{
			m_ProximityChecker = GetComponent<ProximityChecker>();
		}

		// Update is called once per frame
		void Update()
		{
			if (m_ProximityChecker.LocalPlayerIsClose)
			{
				SendImportantMessageServerRpc();
			}
		}

		[ServerRpc]
		void SendImportantMessageServerRpc()
		{
			if (IsServer)
			{
				Debug.Log("Client just wanted me to send an important message to all clients");
				DistributeImportantMessageClientRpc();
			}		
		}

		[ClientRpc]
		void DistributeImportantMessageClientRpc()
		{
			Debug.Log("Server just send me an important message");
		}

	} 
}
