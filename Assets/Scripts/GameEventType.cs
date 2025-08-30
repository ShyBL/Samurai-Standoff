public enum GameEventType
{
    // Timer events
    SignalTriggered,
    TimerStarted,
    TimerStopped,
    
    // Player events
    PlayerDrawn,
    PlayerFault,
    PlayerWon,
    
    // Enemy events
    EnemyDrawn,
    EnemyWon,
    
    // Game state events
    RoundStarted,
    RoundEnded,
    WinnerDeclared,
    FaultRestart,
    
    // Level events
    LevelStarted,
    LevelCompleted,
    GameCompleted,
    
    // Audio events
    PlaySound,
    StopSound,
    
    // Scene events
    SceneTransitionStarted,
    SceneTransitionCompleted
}