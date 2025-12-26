using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SamuraiStandoff
{
    public class BackgroundController : MonoBehaviour
    {
        [SerializeField] private List<Sprite> backgrounds;
        [SerializeField] private Image background;
        private Sprite _selectedBackground;
        [SerializeField] private GameObject GreenFX;
        [SerializeField] private GameObject PinkFX;

        private void Awake()
        {
            var difficulty = Resources.Load<GameData>("Game Data").currentDifficulty;
            
            _selectedBackground = difficulty switch
            {
                EnemyDifficultyType.EasyMode => backgrounds[0],
                EnemyDifficultyType.MediumMode => backgrounds[1],
                EnemyDifficultyType.HardMode => backgrounds[2],
                _ => throw new ArgumentOutOfRangeException()
            };
            
            switch (difficulty)
            {
                case EnemyDifficultyType.EasyMode:
                    GreenFX.SetActive(true);
                    PinkFX.SetActive(false);
                    break;

                case EnemyDifficultyType.MediumMode:
                case EnemyDifficultyType.HardMode:
                    GreenFX.SetActive(false);
                    PinkFX.SetActive(true);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            background.sprite = _selectedBackground;

            AudioManager.instance.PlaySound("Waterfall");
        }

        private void OnDestroy()
        {
            AudioManager.instance.StopSound("Waterfall");
        }
    }
}