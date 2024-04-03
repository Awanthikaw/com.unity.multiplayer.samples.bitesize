using UnityEngine;

namespace Unity.Netcode.Samples.MultiplayerUseCases.Proximity
{
    /// <summary>
    /// Manages the color of a Networked object
    /// </summary>
    public class ColorManager : NetworkBehaviour
    {
        NetworkVariable<Color32> m_NetworkedColor = new NetworkVariable<Color32>();
        Material m_Material;
        ProximityChecker m_ProximityChecker;

        //modifications
        // has to be networked because each client would have its own variable otherwise
        // can only be written by the server
        [SerializeField] NetworkVariable<bool> m_OwnershipGiven = new NetworkVariable<bool>(false);

        void Awake()
        {
            m_Material = GetComponent<Renderer>().material;
            m_ProximityChecker = GetComponent<ProximityChecker>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsClient)
            {
                /* in this case, you need to manually load the initial Color to catch up with the state of the network variable.
                * This is particularly useful when re-connecting or hot-joining a session */
                OnClientColorChanged(m_Material.color, m_NetworkedColor.Value);
                m_NetworkedColor.OnValueChanged += OnClientColorChanged;
                m_ProximityChecker.AddListener(OnClientLocalPlayerProximityStatusChanged);
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (IsClient)
            {
                m_NetworkedColor.OnValueChanged -= OnClientColorChanged;
                m_ProximityChecker.RemoveListener(OnClientLocalPlayerProximityStatusChanged);
            }
        }

        void Update()
        {
			if (!IsClient || !m_ProximityChecker.LocalPlayerIsClose)
            {
                /* note: in this case there's only client-side logic and therefore the script returns early.
                 * In a real production scenario, you would have an UpdateManager running all Updates from a centralized point.
                 * An alternative to that is to disable behaviours on client/server depending to what is/is not going to be executed on that instance. */
                return;
            }

            if (IsClient && !m_OwnershipGiven.Value && m_ProximityChecker.LocalPlayerIsClose)
			{
				var currentPlayerInTheRange = NetworkManager.Singleton.LocalClient.ClientId;
				GiveOwnership(currentPlayerInTheRange);
			}

			if (Input.GetKeyUp(KeyCode.E) && m_OwnershipGiven.Value && IsOwner)
            {
                OnClientRequestColorChange();
            }
        }

        void OnClientRequestColorChange()
        {
            OnServerChangeColorServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        void OnServerChangeColorServerRpc()
        {
            m_NetworkedColor.Value = GetRandomColor();
        }

        void OnClientColorChanged(Color32 previousColor, Color32 newColor)
        {
            m_Material.color = newColor;
        }

        void OnClientLocalPlayerProximityStatusChanged(bool isClose)
        {
            Debug.Log($"Local player is now {(isClose ? "close" : "far")}");
        }

		private void GiveOwnership(ulong targetClientID)
		{
            GiveOwnershipServerRpc(targetClientID);
		}

		[ServerRpc(RequireOwnership = false)]
		void GiveOwnershipServerRpc(ulong targetClientID)
        {
			m_OwnershipGiven.Value = true;
			Debug.Log("Giving ownership");
			var networkObject = GetComponentInParent<NetworkObject>();
			if (networkObject != null)
            {
                Debug.Log("playerID: " + targetClientID);
                networkObject.ChangeOwnership(targetClientID);
            }
        }


		private Color32 GetRandomColor() => new Color32((byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256), 255);
	}
}
