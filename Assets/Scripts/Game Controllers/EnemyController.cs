using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    #region Serialized Fields

    [Header("Enemy UI Elements")] 
    [SerializeField] private TextMeshProUGUI enemyNameText;

    [SerializeField] private Image enemyImage;

    [Header("Enemy State")] 
    [SerializeField] private bool hasEnemyAttacked;
    [SerializeField] private Character selectedCharacter;

    [Header("Game Data")] 
    [SerializeField] private GameData enemyStats; 

    #endregion

    #region Private Fields

    private float _attackTimer;
    private float _reactionTime;

    private Dictionary<EnemyDifficultyType, List<CharacterType>> _difficultyCharacterMap;

    #endregion

    #region Unity Methods

    private void Start()
    {
        InitializeDifficultyCharacterMap();
        AssignEnemyTraits();
        ResetAttackTimer();
    }

    private void Update()
    {
        HandleEnemyAttackLogic();
    }

    #endregion

    #region Initialization

    // Maps difficulty levels to character type sequences.
    private void InitializeDifficultyCharacterMap()
    {
        _difficultyCharacterMap = new()
        {
            {
                EnemyDifficultyType.EasyMode,
                new List<CharacterType>
                {
                    CharacterType.Ichi, CharacterType.Bluetail, CharacterType.Macaroni
                }
            },
            {
                EnemyDifficultyType.MediumMode,
                new List<CharacterType>
                {
                    CharacterType.Bluetail, CharacterType.Macaroni, CharacterType.Chaolin
                }
            },
            {
                EnemyDifficultyType.HardMode,
                new List<CharacterType>
                {
                    CharacterType.Macaroni,
                    CharacterType.Chaolin,
                    CharacterType.Ichi,
                    CharacterType.Fraug
                }
            }
        };
    }

    // Assigns character traits and reaction time based on current level and difficulty.
    private void AssignEnemyTraits()
    {
        Debug.Log("Assign Enemy Traits Called");

        int levelIndex = LevelManager.instance.currentLevel - 1;
        var difficulty = LevelManager.instance.currentDifficulty;

        // Set reaction time based on difficulty
        _reactionTime = difficulty switch
        {
            EnemyDifficultyType.EasyMode => enemyStats.easy[levelIndex],
            EnemyDifficultyType.MediumMode => enemyStats.medium[levelIndex],
            EnemyDifficultyType.HardMode => enemyStats.hard[levelIndex],
            _ => throw new ArgumentOutOfRangeException()
        };

        // Select character for this level and difficulty
        var characterOrder = _difficultyCharacterMap[difficulty];
        if (levelIndex >= characterOrder.Count)
        {
            Debug.LogWarning("Level index exceeds character list for difficulty.");
            return;
        }

        var characterType = characterOrder[levelIndex];
        selectedCharacter = enemyStats.allCharacters.FirstOrDefault(c => c.type == characterType);

        // Set UI elements
        enemyImage.sprite = selectedCharacter?.sprites.FirstOrDefault(); // Idle sprite
        enemyNameText.text = selectedCharacter?.name;
    }

    private void ResetAttackTimer()
    {
        _attackTimer = Timer.instance.signalTime + _reactionTime;
    }

    #endregion

    #region Game Logic

    private void HandleEnemyAttackLogic()
    {
        if (!GameController.instance.winnerDeclared)
        {
            if (!hasEnemyAttacked && _attackTimer > 0)
            {
                _attackTimer -= Time.deltaTime;
            }

            if (!hasEnemyAttacked && _attackTimer <= 0)
            {
                enemyImage.sprite = selectedCharacter.sprites[1]; // Attack sprite

                MoveEnemyToAttackPosition();
                Debug.Log("AI Attacked");

                hasEnemyAttacked = true;
                GameController.instance.DeclareWinner(gameObject);
            }
        }
        else if (GameController.instance.winnerDeclared && !hasEnemyAttacked)
        {
            enemyImage.sprite = selectedCharacter.sprites[2]; // Defeat sprite
            MoveEnemyToAttackPosition();
        }
    }

    private void MoveEnemyToAttackPosition()
    {
        Vector3 newPosition = transform.localPosition;
        newPosition.x = -600;
        transform.localPosition = newPosition;
    }

    #endregion
}