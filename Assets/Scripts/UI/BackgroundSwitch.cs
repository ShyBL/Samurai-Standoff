using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SamuraiStandoff
{
    public class BackgroundSwitch : MonoBehaviour
    {
        [SerializeField] private List<Sprite> backgrounds;
        [SerializeField] private Image background;
        private Sprite _selectedBackground;

        private void Awake()
        {
            _selectedBackground = Resources.Load<GameData>("Game Data").currentDifficulty switch
            {
                EnemyDifficultyType.EasyMode => backgrounds[0],
                EnemyDifficultyType.MediumMode => backgrounds[1],
                EnemyDifficultyType.HardMode => backgrounds[2],
                _ => throw new ArgumentOutOfRangeException()
            };

            background.sprite = _selectedBackground;

            AudioManager.instance.PlaySound("Waterfall");
        }

        private void OnDestroy()
        {
            AudioManager.instance.StopSound("Waterfall");
        }
    }
}