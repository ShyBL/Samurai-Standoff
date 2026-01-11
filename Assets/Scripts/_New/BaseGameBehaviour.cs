using UnityEngine;

namespace SamuraiStandoff
{
    /// <summary>
    /// Base class for all game MonoBehaviours that need access to shared game data.
    /// Provides cached access to GameData and PlayerData without needing serialized references.
    /// </summary>
    public class BaseGameBehaviour : MonoBehaviour
    {
        // Cached references - loaded once and reused
        private static GameData _cachedGameData;
        private static PlayerData _cachedPlayerData;
        private static PlayerData _cachedPlayer2Data;

        /// <summary>
        /// Access to the main GameData ScriptableObject
        /// </summary>
        protected GameData gameData
        {
            get
            {
                if (_cachedGameData == null)
                {
                    _cachedGameData = Resources.Load<GameData>("GameData");
                    if (_cachedGameData == null)
                    {
                        Debug.LogError("GameData not found in Resources folder! Make sure it's named 'GameData' and located in a Resources folder.");
                    }
                }
                return _cachedGameData;
            }
        }

        /// <summary>
        /// Access to Player 1's PlayerData ScriptableObject
        /// </summary>
        protected PlayerData playerData
        {
            get
            {
                if (_cachedPlayerData == null)
                {
                    _cachedPlayerData = Resources.Load<PlayerData>("PlayerData");
                    if (_cachedPlayerData == null)
                    {
                        Debug.LogError("PlayerData not found in Resources folder! Make sure it's named 'PlayerData' and located in a Resources folder.");
                    }
                }
                return _cachedPlayerData;
            }
        }

        /// <summary>
        /// Access to Player 2's PlayerData ScriptableObject (for multiplayer)
        /// </summary>
        protected PlayerData player2Data
        {
            get
            {
                if (_cachedPlayer2Data == null)
                {
                    _cachedPlayer2Data = Resources.Load<PlayerData>("Player2Data");
                    if (_cachedPlayer2Data == null)
                    {
                        Debug.LogError("Player2Data not found in Resources folder! Make sure it's named 'Player2Data' and located in a Resources folder.");
                    }
                }
                return _cachedPlayer2Data;
            }
        }

        /// <summary>
        /// Force reload all cached resources (useful after scene changes or for testing)
        /// </summary>
        protected static void ReloadResources()
        {
            _cachedGameData = null;
            _cachedPlayerData = null;
            _cachedPlayer2Data = null;
        }

        /// <summary>
        /// Optional: Override this in derived classes for custom initialization
        /// </summary>
        protected virtual void OnEnable()
        {
            // Optionally validate resources on enable
            #if UNITY_EDITOR
            ValidateResources();
            #endif
        }

        #if UNITY_EDITOR
        private void ValidateResources()
        {
            // In editor, warn if resources aren't found
            if (gameData == null)
            {
                Debug.LogWarning($"{GetType().Name}: GameData could not be loaded from Resources.");
            }
            if (playerData == null)
            {
                Debug.LogWarning($"{GetType().Name}: PlayerData could not be loaded from Resources.");
            }
        }
        #endif
    }
}