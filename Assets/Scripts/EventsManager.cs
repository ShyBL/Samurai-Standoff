using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class EventsManager : MonoBehaviour
{
    public static EventsManager instance;
    
    private Dictionary<GameEventType, List<Action<GameEvent>>> eventListeners;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEventSystem();
            SceneManager.LoadScene(1);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeEventSystem()
    {
        eventListeners = new Dictionary<GameEventType, List<Action<GameEvent>>>();
        
        // Initialize all event types
        foreach (GameEventType eventType in System.Enum.GetValues(typeof(GameEventType)))
        {
            eventListeners[eventType] = new List<Action<GameEvent>>();
        }
    }
    
    public void Subscribe(GameEventType eventType, Action<GameEvent> listener)
    {
        if (eventListeners.ContainsKey(eventType))
        {
            eventListeners[eventType].Add(listener);
        }
    }
    
    public void Unsubscribe(GameEventType eventType, Action<GameEvent> listener)
    {
        if (eventListeners.ContainsKey(eventType))
        {
            eventListeners[eventType].Remove(listener);
        }
    }
    
    public void Publish(GameEventType eventType, object data = null, GameObject sender = null)
    {
        GameEvent gameEvent = new GameEvent(eventType, data, sender);
        
        if (eventListeners.ContainsKey(eventType))
        {
            foreach (Action<GameEvent> listener in eventListeners[eventType])
            {
                listener?.Invoke(gameEvent);
            }
        }
    }
    
    public void ClearAllListeners()
    {
        foreach (var eventType in eventListeners.Keys)
        {
            eventListeners[eventType].Clear();
        }
    }
}