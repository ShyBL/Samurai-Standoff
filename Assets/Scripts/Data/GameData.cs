using System.Collections.Generic;
using UnityEngine;

namespace SamuraiStandoff
{
    [CreateAssetMenu(menuName = "Samurai Standoff/Game Data")]
    public class GameData : ScriptableObject
    {
        [Header("Game Definitions")] 
        public List<Character> allCharacters;
        public List<KeyCode> attackKeys = new List<KeyCode>()
        {
            KeyCode.A,
            KeyCode.S,
            KeyCode.D
        };
        public List<KeyCode> p2AttackKeys = new List<KeyCode>()
        {
            KeyCode.J,
            KeyCode.K,
            KeyCode.L
        };

        [Header("Difficulty Settings")] 
        public List<float> easyReactionTimes = new() { 1f, 0.75f, 0.5f, 0.25f };
        public List<float> mediumReactionTimes = new() { 0.75f, 0.5f, 0.3f };
        public List<float> hardReactionTimes = new() { 0.5f, 0.4f, 0.3f, 0.2f, 0.1f };
        public int easyTotalLevels;
        public int mediumTotalLevels;
        public int hardTotalLevels;

        [Header("Audio Settings")] 
        [Range(1f, 100f)]
        public float masterVolume = 80;
        [Range(1f, 100f)]
        public float backgroundVolume = 100;

        [Header("Game State")] 
        public int faultCounter;
        public EnemyDifficultyType currentDifficulty;
        public bool isMultiplayer;

        [Header("Last Duel Results")]
        public bool lastDuelPlayerWon; // For single player - did player win?
        public bool lastDuelPlayer1Won; // For multiplayer - did player 1 win?
        public CharacterType lastEnemyCharacterType; // Who was the enemy in last duel?
        public CharacterType winningCharacter; // Who won the last duel overall?
        public int lastDuelFrameCount; // How many frames after signal did winner attack?

       
        
    }
}