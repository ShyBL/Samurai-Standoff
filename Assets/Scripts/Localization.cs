using System;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class Localization : MonoBehaviour
{
    [SerializeField] private GameData _gameData;
    
    void Start()
    {
        ChangeLang(_gameData.currentlanguge);
    }
    
    public void ChangeLang(int lang)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[lang];
        _gameData.currentlanguge = lang;
    }
    
}