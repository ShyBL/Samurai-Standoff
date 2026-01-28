using TMPro;
using UnityEngine;
using Steamworks;

namespace SamuraiStandoff
{
    /// <summary>
    /// Optional component to handle Steam Remote Play session events.
    /// Attach this to your GameManager or a dedicated Steam object.
    /// </summary>
    public class RemotePlayCallbacks : MonoBehaviour
    {
        private Callback<SteamRemotePlaySessionConnected_t> m_SessionConnected;
        private Callback<SteamRemotePlaySessionDisconnected_t> m_SessionDisconnected;

        [Header("UI References (Optional)")]
        [SerializeField] private GameObject playerJoinedNotification;
        [SerializeField] private GameObject playerLeftNotification;
        
        private void OnEnable()
        {
            if (!SteamManager.Initialized) return;
            
            // Register callbacks for remote play session events
            m_SessionConnected = Callback<SteamRemotePlaySessionConnected_t>.Create(OnSessionConnected);
            m_SessionDisconnected = Callback<SteamRemotePlaySessionDisconnected_t>.Create(OnSessionDisconnected);
            
            Debug.Log("[Remote Play Callbacks] Registered for session events");
        }

        /// <summary>
        /// Called when a remote player connects via Steam Remote Play
        /// </summary>
        private void OnSessionConnected(SteamRemotePlaySessionConnected_t callback)
        {
            Debug.Log($"[Remote Play] Player connected - Session ID: {callback.m_unSessionID}");
            
            // Get info about the connected player
            CSteamID playerSteamID = SteamRemotePlay.GetSessionSteamID(callback.m_unSessionID);
            string playerName = SteamFriends.GetFriendPersonaName(playerSteamID);
            
            Debug.Log($"[Remote Play] Player name: {playerName}");
            
            // Optional: Show notification UI
            ShowPlayerJoinedNotification(playerName);
        }

        /// <summary>
        /// Called when a remote player disconnects
        /// </summary>
        private void OnSessionDisconnected(SteamRemotePlaySessionDisconnected_t callback)
        {
            Debug.Log($"[Remote Play] Player disconnected - Session ID: {callback.m_unSessionID}");
            
            // Check if any remote players are still connected
            uint remainingPlayers = SteamRemotePlay.GetSessionCount();
            
            // Optional: Show notification UI
            ShowPlayerLeftNotification();
        }

        /// <summary>
        /// Shows a UI notification when a player joins (optional)
        /// </summary>
        private void ShowPlayerJoinedNotification(string playerName)
        {
            if (playerJoinedNotification != null)
            {
                playerJoinedNotification.SetActive(true);
                playerJoinedNotification.GetComponent<TextMeshProUGUI>().text = $"{playerName} joined!";
            }
            
            Debug.Log($"[UI] Show notification: {playerName} joined!");
        }

        /// <summary>
        /// Shows a UI notification when a player leaves (optional)
        /// </summary>
        private void ShowPlayerLeftNotification()
        {
            if (playerLeftNotification != null)
            {
                playerLeftNotification.SetActive(true);
                playerLeftNotification.GetComponent<TextMeshProUGUI>().text = $"Player 2 left!";

            }
            
            Debug.Log("[UI] Show notification: Player left!");
        }

        /// <summary>
        /// Gets information about all currently connected remote players
        /// </summary>
        public void LogAllRemotePlayers()
        {
            if (!SteamManager.Initialized) return;
            
            uint sessionCount = SteamRemotePlay.GetSessionCount();
            Debug.Log($"[Remote Play] Total sessions: {sessionCount}");
            
            for (int i = 0; i < sessionCount; i++)
            {
                RemotePlaySessionID_t sessionID = SteamRemotePlay.GetSessionID(i);
                CSteamID steamID = SteamRemotePlay.GetSessionSteamID(sessionID);
                string name = SteamFriends.GetFriendPersonaName(steamID);
                ESteamDeviceFormFactor device = SteamRemotePlay.GetSessionClientFormFactor(sessionID);
                
                Debug.Log($"[Remote Play] Session {i}: {name} on {device}");
            }
        }

        public static RemotePlayCallbacks instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
    }
}