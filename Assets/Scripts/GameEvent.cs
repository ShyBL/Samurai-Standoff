using UnityEngine;

[System.Serializable]
public class GameEvent
{
    public GameEventType eventType;
    public object data;
    public GameObject sender;
    
    public GameEvent(GameEventType type, object data = null, GameObject sender = null)
    {
        this.eventType = type;
        this.data = data;
        this.sender = sender;
    }
}