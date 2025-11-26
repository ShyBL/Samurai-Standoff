using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SamuraiStandoff
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Samurai Standoff/Player Data")]
    public class PlayerData : ScriptableObject
    {
        public Character selectedCharacter;
        public CharacterType characterType;
        public int lastBestFrameCount = 10000;
        public int currentBestFrameCount = 10000;
        public int currentLevel = 1;

        public Dictionary<CharacterType, bool> Characters = new Dictionary<CharacterType, bool>()
        {
            { CharacterType.Monk, true },
            { CharacterType.Ichi, false },
            { CharacterType.Bluetail, false },
            { CharacterType.Macaroni, false },
            { CharacterType.Chaolin, false },
            { CharacterType.Fraug, false }
        };

        // TODO: ADD HERE EVERY VARIABLE ADDED IN PLAYER DATA

        [Header("Difficulty Progression & Analytics")]
        public bool completedEasyMode;

        public bool completedMediumMode;
        public bool completedHardMode;

        [Header("Analytics & Achievements")] public bool startedFirstDuel; // Did player enter their first battle?
        public bool wonFirstDuel; // Did they win their first duel?
        public bool reachedMediumDifficulty; // Finished all 4 Easy stages
        public bool reachedHardDifficulty; // Finished all 4 Medium stages  
        public bool defeatedFraug; // Finished all 5 Hard stages (full game)

        public int m_perfectTimingWins; // Exactly 1 frame after signal
        public int m_totalEarlyAttacks; // Attacked before signal
        public int m_currentWinStreak; // Best streak (resets on loss)
        public int m_bestWinStreak; // Best ever streak

        public int m_totalDuels;
        public int m_totalWins;
        public int m_totalLosses;
        public int m_totalDraws;
        public int m_maxWinStreak;

        [Header("Stage Completion Counts")] [Tooltip("How many stages completed in Easy (0-4)")]
        public int easyStagesCompleted;

        [Tooltip("How many stages completed in Medium (0-4)")]
        public int mediumStagesCompleted;

        [Tooltip("How many stages completed in Hard (0-5)")]
        public int hardStagesCompleted;
        
        private void OnEnable()
        {
            if (characterType != CharacterType.Monk)
            {
                characterType = CharacterType.Monk;
            }

            if (selectedCharacter == null)
            {
                var gameData = Resources.Load("GameData") as GameData;
                if (gameData != null && gameData.allCharacters != null)
                {
                    selectedCharacter = gameData.allCharacters.FirstOrDefault(c => c.type == characterType);
                }
                //selectedCharacter = gameData.allCharacters.FirstOrDefault(c => c.type == characterType);
            }
        }
    }
}