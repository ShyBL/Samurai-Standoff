using System;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class Localization : MonoBehaviour
{
    [SerializeField] private GameData _gameData;
    
    void Start()
    {
        Invoke("Delay", 0.1f);
    }

    private void Delay()
    {
        int number = _gameData.currentlanguge;
        ChangeLang(number);
    }
    
    public void ChangeLang(int lang)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[lang];
        _gameData.currentlanguge = lang;
    }
    
}