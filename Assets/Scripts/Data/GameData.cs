using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Samurai Standoff/Game Data")]
public class GameData : ScriptableObject
{
    [Header("Game Definitions")]
    public List<Character> allCharacters;
    
    [Header("Difficulty Settings")]
    public List<float> easy = new (){ 1f, 0.75f, 0.5f, 0.25f };
    public List<float> medium = new () { 0.75f, 0.5f, 0.3f };
    public List<float> hard = new (){ 0.5f, 0.4f, 0.3f, 0.2f, 0.1f };
    public int easyTotalLevels;
    public int mediumTotalLevels;
    public int hardTotalLevels;
    
    [Header("Audio Settings")]
    [Range(0.1f, 100f)] public float volume;

    [Header("Game State")]
    public int faultCounter;
    public int currentLevel = 1;
    public bool isMultiplayer;
    public EnemyDifficultyType currentDifficulty;
}