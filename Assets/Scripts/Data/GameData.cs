using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Samurai Standoff/Game Data")]
public class GameData : ScriptableObject
{
    [Header("Game Definitions")]
    public List<Character> allCharacters;
    public List<KeyCode> attackKeys = new List<KeyCode>() 
    { 
        KeyCode.Space, 
        KeyCode.A, 
        KeyCode.S, 
        KeyCode.D 
    };
    
    [Header("Difficulty Settings")]
    public List<float> easyReactionTimes = new (){ 1f, 0.75f, 0.5f, 0.25f };
    public List<float> mediumReactionTimes = new () { 0.75f, 0.5f, 0.3f };
    public List<float> hardReactionTimes = new (){ 0.5f, 0.4f, 0.3f, 0.2f, 0.1f };
    public int easyTotalLevels;
    public int mediumTotalLevels;
    public int hardTotalLevels;
    
    [Header("Audio Settings")]
    [Range(0.1f, 100f)] public float volume;

    [Header("Game State")]
    public int faultCounter;
    public bool isMultiplayer;
    public EnemyDifficultyType currentDifficulty;
}