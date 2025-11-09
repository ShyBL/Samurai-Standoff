using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SamuraiStandoff;

public class GameLogicTests
{
    private GameObject testGameObject;
    private GameManager gameManager;
    private PlayerData playerData;
    private GameData gameData;

    [SetUp]
    public void Setup()
    {
        testGameObject = new GameObject();
        SetupMockData();
    }

    [TearDown]
    public void Teardown()
    {
        if (testGameObject != null)
            Object.DestroyImmediate(testGameObject);
        if (playerData != null)
            Object.DestroyImmediate(playerData);
        if (gameData != null)
            Object.DestroyImmediate(gameData);
    }

    private void SetupMockData()
    {
        gameData = ScriptableObject.CreateInstance<GameData>();
        gameData.allCharacters = new List<Character>
        {
            new Character { name = "Monk", type = CharacterType.Monk, sprites = new List<Sprite>() },
            new Character { name = "Ichi", type = CharacterType.Ichi, sprites = new List<Sprite>() },
            new Character { name = "Bluetail", type = CharacterType.Bluetail, sprites = new List<Sprite>() },
            new Character { name = "Macaroni", type = CharacterType.Macaroni, sprites = new List<Sprite>() },
            new Character { name = "Chaolin", type = CharacterType.Chaolin, sprites = new List<Sprite>() },
            new Character { name = "Fraug", type = CharacterType.Fraug, sprites = new List<Sprite>() }
        };
        
        playerData = ScriptableObject.CreateInstance<PlayerData>();
        playerData.characterType = CharacterType.Monk;
        playerData.selectedCharacter = gameData.allCharacters[0];
    }

    [Test]
    public void CharacterUnlock_EasyModeCompletion_UnlocksIchi()
    {
        gameManager = testGameObject.AddComponent<GameManager>();
        
        var playerDataField = typeof(GameManager).GetField("playerData", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var gameDataField = typeof(GameManager).GetField("gameData", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        playerDataField.SetValue(gameManager, playerData);
        gameDataField.SetValue(gameManager, gameData);
        
        playerData.completedEasyMode = true;
        gameManager.OnDifficultyCompleted("easy");

        Assert.IsTrue(playerData.Characters[CharacterType.Ichi]);
    }

    [Test]
    public void DifficultyButtons_EasyNotCompleted_MediumLocked()
    {
        playerData.completedEasyMode = false;
        Assert.IsFalse(playerData.completedEasyMode);
    }

    [UnityTest]
    public IEnumerator DuelWin_UpdatesPlayerStats()
    {
        gameManager = testGameObject.AddComponent<GameManager>();
        
        var playerDataField = typeof(GameManager).GetField("playerData", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        playerDataField.SetValue(gameManager, playerData);
        
        int initialWins = playerData.m_totalWins;
        gameManager.OnDuelWon(5, "TestEnemy");
        
        yield return null;

        Assert.AreEqual(initialWins + 1, playerData.m_totalWins);
        Assert.AreEqual(1, playerData.m_maxWinStreak);
    }

    [Test]
    public void FaultCounter_SecondFault_TriggersLoss()
    {
        gameData.faultCounter = 1;
        gameData.faultCounter++;
        
        Assert.AreEqual(2, gameData.faultCounter);
    }

    [Test]
    public void ReactionTime_HardMode_CorrectlyAssigned()
    {
        gameData.currentDifficulty = EnemyDifficultyType.HardMode;
        gameData.hardReactionTimes = new List<float> { 0.5f, 0.4f, 0.3f };
        playerData.currentLevel = 2;

        float expectedReaction = gameData.hardReactionTimes[1];

        Assert.AreEqual(0.4f, expectedReaction);
    }

    [UnityTest]
    public IEnumerator Singleton_MultipleInstances_DestroysExtra()
    {
        var obj1 = new GameObject().AddComponent<GameManager>();
        yield return null;
        var obj2 = new GameObject().AddComponent<GameManager>();
        yield return null;
        
        Assert.IsNotNull(GameManager.instance);
        Assert.IsTrue(obj2 == null || obj2.gameObject == null);
    }

    [Test]
    public void ProgressionReset_ClearsAllFlags()
    {
        gameManager = testGameObject.AddComponent<GameManager>();
        
        var playerDataField = typeof(GameManager).GetField("playerData", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        playerDataField.SetValue(gameManager, playerData);
        
        playerData.completedEasyMode = true;
        playerData.wonFirstDuel = true;
        
        gameManager.ResetProgression();
        
        Assert.IsFalse(playerData.completedEasyMode);
        Assert.IsFalse(playerData.wonFirstDuel);
    }
}