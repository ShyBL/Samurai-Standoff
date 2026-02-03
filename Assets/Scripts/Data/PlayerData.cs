using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SamuraiStandoff
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Samurai Standoff/Player Data")]
    public class PlayerData : ScriptableObject
    {
        public int playerNumber = 1;
        public int faultCounter;
        public Character selectedCharacter;
        public CharacterType characterType;
        public int lastBestFrameCount = 10000;
        public int currentBestFrameCount = 10000;
        public int currentLevel = 1;

        public Dictionary<CharacterType, bool> Characters;
        
        
        [Header("Difficulty Progression & Analytics")]
        public bool completedEasyMode;
        public bool completedMediumMode;
        public bool completedHardMode;

        [Header("Analytics & Achievements")] public bool startedFirstDuel; // Did player enter their first battle?
        public bool wonFirstDuel; // Did they win their first duel?
        public bool reachedMediumDifficulty; // Finished all 4 Easy stages
        public bool reachedHardDifficulty; // Finished all 4 Medium stages  
        public bool defeatedFraug; // Finished all 5 Hard stages (full game)

        [Header("Combat Statistics")]
        public int m_perfectTimingWins; // Exactly 1 frame after signal
        public int m_totalEarlyAttacks; // Attacked before signal
        public int m_currentWinStreak; // Best streak (resets on loss)
        public int m_bestWinStreak; // Best ever streak

        public int m_totalDuels;
        public int m_totalWins; 
        public int m_totalLosses;
        public int m_totalDraws;
        public int m_maxWinStreak;
        
        [Header("Multiplayer Statistics")]
        public int multiplayerWins; // Wins against other player
        public int multiplayerLosses; // Losses against other player
        public int multiplayerBestWinStreak;
        public int multiplayerCurrentWinStreak;
    }
}