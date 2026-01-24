using System;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace SamuraiStandoff
{
    public class Localization : MonoBehaviour
    {
        [SerializeField] private GameData _gameData;

        void Start()
        {
            ChangeLang(_gameData.currentLanguage);
        }

        public void ChangeLang(int lang)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[lang];
            _gameData.currentLanguage = lang;
        }

    }
}