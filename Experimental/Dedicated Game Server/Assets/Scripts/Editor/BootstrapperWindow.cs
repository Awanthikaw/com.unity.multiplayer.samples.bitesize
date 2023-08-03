using Unity.Networking.Transport;
using Unity.Template.Multiplayer.NGO.Runtime;
using Unity.Template.Multiplayer.NGO.Shared;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Template.Multiplayer.NGO.Editor
{
    ///<summary>
    /// Allows for fast switching between host/server only/client only modes in unity editor
    ///</summary>
    public class BootstrapperWindow : EditorWindow
    {
        enum NetworkMode
        {
            Server,
            Client,
            Host
        }

        ConfigurationManager Configuration
        {
            get
            {
                try
                {
                    if (configuration == null)
                    {
                        configuration = new ConfigurationManager(ConfigurationManager.k_DevConfigFile);
                    }
                    return configuration;
                }
                catch (System.IO.FileNotFoundException)
                {
                    configuration = new ConfigurationManager(ConfigurationManager.k_DevConfigFile, true);
                    ResetConfigurationToDefault();
                    return configuration;
                }
            }
        }
        ConfigurationManager configuration;

        bool ServerOnly { get { return Configuration.GetBool(ConfigurationManager.k_ModeServer); } set { Configuration.Set(ConfigurationManager.k_ModeServer, value); } }
        bool AutoClient { get { return Configuration.GetBool(ConfigurationManager.k_ModeClient); } set { Configuration.Set(ConfigurationManager.k_ModeClient, value); } }

        /// <summary>
        /// How many players are needed to fill a game instance?
        /// </summary>
        public int MaxPlayers { get { return Configuration.GetInt(ConfigurationManager.k_MaxPlayers); } set { Configuration.Set(ConfigurationManager.k_MaxPlayers, value); } }
        bool UseBots { get { return Configuration.GetBool(ConfigurationManager.k_EnableBots); } set { Configuration.Set(ConfigurationManager.k_EnableBots, value); } }
        string ServerIP { get { return Configuration.GetString(ConfigurationManager.k_ServerIP); } set { Configuration.Set(ConfigurationManager.k_ServerIP, value); } }
        ushort ServerPort { get { return (ushort)Configuration.GetInt(ConfigurationManager.k_Port); } set { Configuration.Set(ConfigurationManager.k_Port, value); } }
        
        /// <summary>
        /// Will the game run in a specific mode when started in the editor?
        /// </summary>
        public bool AutoConnectOnStartup { get { return Configuration.GetBool(ConfigurationManager.k_Autoconnect); } private set { Configuration.Set(ConfigurationManager.k_Autoconnect, value); } }

        NetworkMode m_NetworkMode;
        #region UI

        VisualElement m_Root;
        TextField m_ServerIPTextField;
        Toggle m_UseBotToggle;
        Toggle m_AutoConnectOnStartupToggle;
        EnumField m_NetworkModeList;
        IntegerField m_ServerPort;
        IntegerField m_MaxPlayers;

        #endregion

        /// <summary>
        /// Opens the bootstrapper window
        /// </summary>
        [MenuItem("Multiplayer/Bootstrapper")]
        public static void ShowWindow()
        {
            var window = GetWindow<BootstrapperWindow>("Bootstrapper");
            window.Show();
        }

        void SetupBackend()
        {
            if (ServerOnly)
            {
                m_NetworkMode = NetworkMode.Server;
            }
            else if (AutoClient)
            {
                m_NetworkMode = NetworkMode.Client;
            }
        }

        void SetupFrontend()
        {
            if (m_Root != null)
            {
                m_Root.Clear();
            }
            m_Root = rootVisualElement;

            VisualTreeAsset playerVisualTree = UIElementsUtils.LoadUXML("Bootstrapper");
            playerVisualTree.CloneTree(m_Root);

            m_NetworkModeList = UIElementsUtils.SetupEnumField("lstMode", "Autoconnect Mode", OnNetworkModeChanged, m_Root, m_NetworkMode);
            UIElementsUtils.SetupButton("btnReset", OnClickReset, true, m_Root, "Reset to default");
            m_UseBotToggle = UIElementsUtils.SetupToggle("tglUseBots", "Use bots", string.Empty, UseBots, OnEnableBotsChanged, m_Root);
            m_ServerPort = UIElementsUtils.SetupIntegerField("intServerPort", ServerPort, OnServerPortChanged, m_Root);
            m_ServerIPTextField = UIElementsUtils.SetupStringField("strServerIP", "Server IP", ServerIP, OnServerIPChanged, m_Root);
            m_AutoConnectOnStartupToggle = UIElementsUtils.SetupToggle("tglAutoConnectOnStartup", "Autoconnect on startup", string.Empty, AutoConnectOnStartup, OnAutoConnectChanged, m_Root);
            m_MaxPlayers = UIElementsUtils.SetupIntegerField("intMaxPlayers", MaxPlayers, OnMaxPlayersChanged, m_Root);
            UpdateUIAccordingToNetworkMode();
        }

        void OnNetworkModeChanged(ChangeEvent<System.Enum> evt)
        {
            m_NetworkMode = (NetworkMode)evt.newValue;
            ApplyChanges();
        }

        void UpdateUIAccordingToNetworkMode()
        {
            UIElementsUtils.Show(m_ServerIPTextField);
            UIElementsUtils.Show(m_UseBotToggle);
            UIElementsUtils.Show(m_MaxPlayers);
            switch (m_NetworkMode)
            {
                case NetworkMode.Client:
                    UIElementsUtils.Hide(m_UseBotToggle);
                    UIElementsUtils.Hide(m_MaxPlayers);
                    break;
                case NetworkMode.Host:
                case NetworkMode.Server:
                    UIElementsUtils.Hide(m_ServerIPTextField);
                    break;
                default:
                    break;
            }
            if (AutoConnectOnStartup)
            {
                UIElementsUtils.Show(m_NetworkModeList);
            }
            else
            {
                UIElementsUtils.Hide(m_NetworkModeList);
            }
        }

        void OnEnableBotsChanged(ChangeEvent<bool> evt)
        {
            UseBots = evt.newValue;
            ApplyChanges();
        }

        void OnAutoConnectChanged(ChangeEvent<bool> evt)
        {
            AutoConnectOnStartup = evt.newValue;
            ApplyChanges();
        }

        void OnServerIPChanged(ChangeEvent<string> evt)
        {
            const string defaultIP = "127.0.0.1";
            string newIP = evt.newValue.ToLower();
            if (string.IsNullOrEmpty(newIP))
            {
                newIP = defaultIP;
                m_ServerIPTextField.SetValueWithoutNotify(newIP);
            }

            if (newIP != defaultIP)
            {
                if (!NetworkEndpoint.TryParse(newIP, ServerPort, out var networkEndpoint))
                {
                    Debug.LogError($"{newIP} is not a valid IPv4 address!");
                    return;
                }
                Debug.Log($"{newIP} is a valid IPv4 address!");
            }
            ServerIP = newIP;
            ApplyChanges();
        }

        void OnServerPortChanged(ChangeEvent<int> evt)
        {
            ServerPort = (ushort)evt.newValue;
            ApplyChanges();
        }

        void OnMaxPlayersChanged(ChangeEvent<int> evt)
        {
            MaxPlayers = evt.newValue;
            ApplyChanges();
        }

        void OnClickReset()
        {
            ResetConfigurationToDefault();
        }

        void OnEnable()
        {
            SetupBackend();
            SetupFrontend();
        }

        void ResetConfigurationToDefault()
        {
            configuration.Overwrite(JSONUtilities.ReadJSONFromFile(ConfigurationManager.k_DevConfigFileDefault));
            OnEnable();
            ApplyChanges();
        }

        void ApplyChanges()
        {
            switch (m_NetworkMode)
            {
                case NetworkMode.Server:
                    ServerOnly = true;
                    AutoClient = false;
                    break;
                case NetworkMode.Client:
                    ServerOnly = false;
                    AutoClient = true;
                    break;
                case NetworkMode.Host:
                    ServerOnly = false;
                    AutoClient = false;
                    break;
            }

            Configuration.SaveAsJSON(false);
            UpdateUIAccordingToNetworkMode();
        }
    }
}