using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SamuraiStandoff
{
    public class EnemyController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Enemy UI Elements")] [SerializeField]
        private Image enemyImage;

        [Header("Enemy State")] [SerializeField]
        private bool hasEnemyAttacked;

        [SerializeField] public Character selectedCharacter;

        [Header("Game Data")] [SerializeField] private GameData gameData;
        [SerializeField] private PlayerData playerData;


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

            DuelController.instance.enemyReactionTime = _reactionTime;
            DuelController.instance.SetMaxFramesForSlider();
        }

        private void Update()
        {
            if (DuelController.instance.isPaused) return;
            
            if (!DuelController.instance.playerFault)
            {
                HandleEnemyAttackLogic();
            }
        }

        #endregion

        #region Initialization

        // Maps difficulty levels to character type sequences.
        private void InitializeDifficultyCharacterMap()
        {
            _difficultyCharacterMap = new()
            {
                {
                    EnemyDifficultyType.Tutorial,
                    new List<CharacterType>()
                    {
                        CharacterType.Bamboo
                    }
                },
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

            int levelIndex = playerData.currentLevel - 1;
            EnemyDifficultyType difficulty = gameData.currentDifficulty;

            // Set reaction time based on difficulty
            _reactionTime = difficulty switch
            {
                EnemyDifficultyType.EasyMode => gameData.easyReactionTimes[levelIndex],
                EnemyDifficultyType.MediumMode => gameData.mediumReactionTimes[levelIndex],
                EnemyDifficultyType.HardMode => gameData.hardReactionTimes[levelIndex],
                EnemyDifficultyType.Tutorial => gameData.tutorialReactionTimes[levelIndex],
                _ => throw new ArgumentOutOfRangeException()
            };

            // Select character for this level and difficulty
            List<CharacterType> characterOrder = _difficultyCharacterMap[difficulty];
            
            if (levelIndex >= characterOrder.Count)
            {
                Debug.LogWarning("Level index exceeds character list for difficulty.");
                return;
            }

            CharacterType characterType = characterOrder[levelIndex];
            selectedCharacter = gameData.allCharacters.FirstOrDefault(c => c.type == characterType);

            // Set UI elements
            enemyImage.sprite = selectedCharacter?.sprites.FirstOrDefault(); // Idle sprite
        }

        private void ResetAttackTimer()
        {
            _attackTimer = DuelController.instance.signalTime + _reactionTime;
        }

        #endregion

        #region Game Logic

        private void HandleEnemyAttackLogic()
        {
            if (!DuelController.instance.winnerDeclared)
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
                    DuelController.instance.DeclareWinner(gameObject);
                    
                   
                }
            }
            else if (DuelController.instance.winnerDeclared && !hasEnemyAttacked)
            {
                enemyImage.sprite = selectedCharacter.sprites[2]; // Defeat sprite
                MoveEnemyToAttackPosition();
            }
        }

        private void PlayTutorialAnimation()
        {
            var animator = GetComponentInChildren<Animator>();
            animator.SetTrigger("Tutorial");
        }

        private void MoveEnemyToAttackPosition()
        {
            if (selectedCharacter.type == CharacterType.Bamboo)
            {
                PlayTutorialAnimation();
            }
            else
            {
                Vector3 newPosition = transform.localPosition;
                newPosition.x = -600;
                transform.localPosition = newPosition;
            }
            
        }

        #endregion
    }
}