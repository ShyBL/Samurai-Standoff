using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SamuraiStandoff;
using TMPro;
using UnityEngine.UI;

public class GameLogicTests
{
    private List<GameObject> testGameObjects = new List<GameObject>();
    private GameManager gameManager;
    private PlayerData playerData;
    private GameData gameData;
    private SceneLoader sceneLoader;

    [SetUp]
    public void Setup()
    {
        GameManager.isTestMode = true;
        
        SetupMockData();
        
        // Setup Singletons
        gameManager = CreateTestGameObject<GameManager>("GameManager");
        InjectPrivateField(gameManager, "playerData", playerData);
        InjectPrivateField(gameManager, "gameData", gameData);
        
        sceneLoader = CreateTestGameObject<SceneLoader>("SceneLoader");
        InjectPrivateField(sceneLoader, "playerData", playerData);
        InjectPrivateField(sceneLoader, "gameData", gameData);
    }

    [TearDown]
    public void Teardown()
    {
        GameManager.isTestMode = false;
        
        foreach (var obj in testGameObjects)
        {
            if (obj != null)
                Object.DestroyImmediate(obj);
        }
        testGameObjects.Clear();
            
        // Explicitly nullify singletons
        typeof(GameManager).GetField("instance", BindingFlags.Public | BindingFlags.Static)
            .SetValue(null, null);
        typeof(SceneLoader).GetField("instance", BindingFlags.Public | BindingFlags.Static)
            .SetValue(null, null);
        typeof(DuelController).GetField("instance", BindingFlags.Public | BindingFlags.Static)
            .SetValue(null, null);

        if (playerData != null)
            Object.DestroyImmediate(playerData);
        if (gameData != null)
            Object.DestroyImmediate(gameData);

        gameManager = null;
        sceneLoader = null;
    }

    #region Helper Methods

    private void SetupMockData()
    {
        gameData = ScriptableObject.CreateInstance<GameData>();
        gameData.allCharacters = new List<Character>
        {
            new Character { name = "Monk", type = CharacterType.Monk, sprites = new List<Sprite> { null, null, null } },
            new Character { name = "Ichi", type = CharacterType.Ichi, sprites = new List<Sprite> { null, null, null } },
            new Character { name = "Bluetail", type = CharacterType.Bluetail, sprites = new List<Sprite> { null, null, null } },
            new Character { name = "Macaroni", type = CharacterType.Macaroni, sprites = new List<Sprite> { null, null, null } },
            new Character { name = "Chaolin", type = CharacterType.Chaolin, sprites = new List<Sprite> { null, null, null } },
            new Character { name = "Fraug", type = CharacterType.Fraug, sprites = new List<Sprite> { null, null, null } }
        };
        
        gameData.easyReactionTimes = new() { 1f, 0.75f, 0.5f };
        gameData.mediumReactionTimes = new() { 0.75f, 0.5f, 0.3f };
        gameData.hardReactionTimes = new() { 0.5f, 0.4f, 0.3f, 0.2f, 0.1f };
        gameData.easyTotalLevels = 3;
        gameData.mediumTotalLevels = 3;
        gameData.hardTotalLevels = 5;
        
        gameData.attackKeys = new List<KeyCode>()
        {
            KeyCode.Space, KeyCode.A, KeyCode.S, KeyCode.D
        };

        playerData = ScriptableObject.CreateInstance<PlayerData>();
        playerData.characterType = CharacterType.Monk;
        playerData.selectedCharacter = gameData.allCharacters[0];
        playerData.Characters = new Dictionary<CharacterType, bool>()
        {
            { CharacterType.Monk, true },
            { CharacterType.Ichi, false },
            { CharacterType.Bluetail, false },
            { CharacterType.Macaroni, false },
            { CharacterType.Chaolin, false },
            { CharacterType.Fraug, false }
        };
    }
    
    private PlayerController CreateMockPlayerController()
    {
        var playerObject = new GameObject("MockPlayer");
        testGameObjects.Add(playerObject);
        
        // 1. Add the component. Its Awake() will be called by Unity and
        // will log an error, but we will "fix" it by calling Awake() again.
        var controller = playerObject.AddComponent<PlayerController>();

        // 2. Create mock UI objects
        var faultTextObj = new GameObject("FaultText");
        faultTextObj.transform.SetParent(playerObject.transform);
        var faultText = faultTextObj.AddComponent<TextMeshProUGUI>();

        var playerImageObj = new GameObject("PlayerImage");
        playerImageObj.transform.SetParent(playerObject.transform);
        var playerImage = playerImageObj.AddComponent<Image>();

        var keyPromptObj = new GameObject("KeyPrompt");
        keyPromptObj.transform.SetParent(playerObject.transform);
        keyPromptObj.AddComponent<TextMeshProUGUI>(); // Add text for AssignKey to find
        
        // 3. Inject ALL dependencies into the existing component
        InjectPrivateField(controller, "faultText", faultText);
        InjectPrivateField(controller, "playerImage", playerImage);
        InjectPrivateField(controller, "keyPromptObject", keyPromptObj);
        InjectPrivateField(controller, "playerData", playerData);
        InjectPrivateField(controller, "gameData", gameData);

        // 4. Manually call Awake/Start to re-initialize with the *correct* data.
        CallPrivateMethod(controller, "Awake"); 
        CallPrivateMethod(controller, "Start");
        
        return controller;
    }

    // Helper to create a GameObject, add it to cleanup list, and return component
    private T CreateTestGameObject<T>(string name) where T : Component
    {
        var obj = new GameObject(name);
        testGameObjects.Add(obj);
        return obj.AddComponent<T>();
    }
    
    // Helper to create a mock-ready DuelController
    private DuelController CreateMockDuelController()
    {
        var dcObject = CreateTestGameObject<DuelController>("DuelController");
        DuelController.instance = dcObject;

        // Mock UI elements
        var resultTextObj = new GameObject("ResultText");
        resultTextObj.transform.SetParent(dcObject.transform);
        var resultText = resultTextObj.AddComponent<TextMeshProUGUI>();
        resultText.enabled = false;
        
        var sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(dcObject.transform);
        var slider = sliderObj.AddComponent<Slider>(); // Fix for EnemyController test

        // Inject dependencies
        InjectPrivateField(dcObject, "gameData", gameData);
        InjectPrivateField(dcObject, "playerData", playerData);
        InjectPrivateField(dcObject, "resultText", resultText);
        InjectPrivateField(dcObject, "signalSlider", slider); // Fix for EnemyController test

        return dcObject;
    }

    // Helper to create a mock-ready EnemyController
    private EnemyController CreateMockEnemyController()
    {
        var enemyObject = CreateTestGameObject<EnemyController>("MockEnemy");
        
        var enemyImageObj = new GameObject("EnemyImage");
        enemyImageObj.transform.SetParent(enemyObject.transform);
        InjectPrivateField(enemyObject, "enemyImage", enemyImageObj.AddComponent<Image>());
        
        InjectPrivateField(enemyObject, "playerData", playerData);
        InjectPrivateField(enemyObject, "gameData", gameData);
        
        return enemyObject;
    }
    
    private void InjectPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, 
            BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(obj, value);
    }
    
    private void CallPrivateMethod(object obj, string methodName, params object[] parameters)
    {
        var method = obj.GetType().GetMethod(methodName, 
            BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(obj, parameters);
    }


    #endregion
    

    #region GameManager Tests

    [Test]
    public void ProgressionReset_ClearsAllFlags()
    {
        // Arrange
        playerData.completedEasyMode = true;
        playerData.wonFirstDuel = true;
        playerData.m_totalLosses = 5; 
        playerData.easyStagesCompleted = 2;
        
        // Act
        gameManager.ResetProgression();
        
        // Assert
        Assert.IsFalse(playerData.completedEasyMode, "completedEasyMode");
        Assert.IsFalse(playerData.wonFirstDuel, "wonFirstDuel");
        Assert.AreEqual(0, playerData.easyStagesCompleted, "easyStagesCompleted");
        // NOTE: This test will still fail until you add the m_totalLosses to the ResetProgression method in GameManager.cs
        Assert.AreEqual(0, playerData.m_totalLosses, "m_totalLosses"); 
    }
    
    [Test]
    public void CharacterUnlock_EasyModeCompletion_UnlocksIchi()
    {
        gameManager.OnDifficultyCompleted("easy");
        Assert.IsTrue(playerData.Characters[CharacterType.Ichi]);
        Assert.IsTrue(playerData.completedEasyMode);
        Assert.IsTrue(playerData.reachedMediumDifficulty);
    }

    [UnityTest]
    public IEnumerator DuelWin_UpdatesPlayerStats()
    {
        int initialWins = playerData.m_totalWins;
        int initialDuels = playerData.m_totalDuels;
        gameManager.OnDuelWon(5, "TestEnemy");
        yield return null;
        Assert.AreEqual(initialWins + 1, playerData.m_totalWins);
        Assert.AreEqual(initialDuels + 1, playerData.m_totalDuels);
        Assert.AreEqual(1, playerData.m_maxWinStreak);
        Assert.IsTrue(playerData.wonFirstDuel);
    }

    [Test]
    public void OnDuelLost_UpdatesStatsAndResetsWinStreak()
    {
        playerData.m_maxWinStreak = 5;
        playerData.m_totalLosses = 2;
        playerData.m_totalDuels = 7;
        gameManager.OnDuelLost();
        Assert.AreEqual(0, playerData.m_maxWinStreak);
        Assert.AreEqual(3, playerData.m_totalLosses);
        Assert.AreEqual(8, playerData.m_totalDuels);
    }

    [Test]
    public void ValidateCharacterUnlocks_UnlocksMacaroni_On10Losses()
    {
        playerData.m_totalLosses = 10;
        Assert.IsFalse(playerData.Characters[CharacterType.Macaroni]);
        CallPrivateMethod(gameManager, "ValidateCharacterUnlocks");
        Assert.IsTrue(playerData.Characters[CharacterType.Macaroni]);
    }
    
    #endregion

    #region PlayerController Tests
    
    
    [Test]
    public void PlayerController_AssignKey_EasyMode_AlwaysUsesFirstKey()
    {
        // Arrange
        var playerObject = CreateMockPlayerController(); // Use corrected helper
        
        gameData.currentDifficulty = EnemyDifficultyType.EasyMode;
        gameData.attackKeys = new List<KeyCode> { KeyCode.A, KeyCode.B, KeyCode.C };
        
        // Act
        CallPrivateMethod(playerObject, "AssignKey"); 
        
        // Assert
        var currentKey = (KeyCode)typeof(PlayerController).GetField("_currentKey", 
            BindingFlags.NonPublic | BindingFlags.Instance).GetValue(playerObject);
            
        Assert.AreEqual(KeyCode.A, currentKey);
    }

    [Test]
    public void PlayerController_AssignKey_HardMode_UsesKeyFromList()
    {
        // Arrange
        var playerObject = CreateMockPlayerController(); // Use corrected helper
        
        gameData.currentDifficulty = EnemyDifficultyType.HardMode;
        List<KeyCode> keys = new List<KeyCode> { KeyCode.X, KeyCode.Y, KeyCode.Z };
        gameData.attackKeys = keys;
        
        // Act
        CallPrivateMethod(playerObject, "AssignKey");
        
        // Assert
        var currentKey = (KeyCode)typeof(PlayerController).GetField("_currentKey", 
            BindingFlags.NonPublic | BindingFlags.Instance).GetValue(playerObject);
            
        Assert.IsTrue(keys.Contains(currentKey));
    }
    
    #endregion

    #region EnemyController Tests
    

    [Test]
    public void EnemyController_AssignEnemyTraits_HardModeLevel4_AssignsFraugAndReaction()
    {
        // Arrange
        var dcObject = CreateMockDuelController(); 
        var enemyObject = CreateMockEnemyController(); 

        gameData.currentDifficulty = EnemyDifficultyType.HardMode;
        playerData.currentLevel = 4; 
        
        // Act
        CallPrivateMethod(enemyObject, "Start"); 
        
        // Assert
        var reactionTime = (float)typeof(EnemyController).GetField("_reactionTime", 
            BindingFlags.NonPublic | BindingFlags.Instance).GetValue(enemyObject);
        
        var selectedChar = (Character)typeof(EnemyController).GetField("selectedCharacter", 
            BindingFlags.NonPublic | BindingFlags.Instance).GetValue(enemyObject);
        
        Assert.AreEqual(0.2f, reactionTime); 
        Assert.AreEqual(CharacterType.Fraug, selectedChar.type); 
    }
    
    #endregion

    #region DuelController Integration Test
    
    [UnityTest]
    public IEnumerator DuelController_DeclareWinner_PlayerWinsFinalEasyLevel_CompletesEasyMode()
    {
        // Arrange
        var dcObject = CreateMockDuelController(); 
        var pOneObject = CreateMockPlayerController(); 
        var pTwoObject = CreateMockEnemyController(); 

        InjectPrivateField(dcObject, "pOne", pOneObject.gameObject);
        InjectPrivateField(dcObject, "pTwo", pTwoObject.gameObject);

        // Set game state
        gameData.isMultiplayer = false;
        gameData.currentDifficulty = EnemyDifficultyType.EasyMode;
        gameData.easyTotalLevels = 3;
        playerData.currentLevel = 3; 
        
        Assert.IsFalse(playerData.completedEasyMode, "Pre-condition: Easy mode should not be complete");

        // Act
        dcObject.DeclareWinner(pOneObject.gameObject, false); 
        
        yield return null; 
        
        // Assert
        Assert.IsTrue(playerData.completedEasyMode, "Easy mode should be marked as complete");
        Assert.IsTrue(playerData.reachedMediumDifficulty, "Medium difficulty should be unlocked");
        Assert.AreEqual(1, playerData.m_totalWins, "Player should have 1 win");
    }

    #endregion
}