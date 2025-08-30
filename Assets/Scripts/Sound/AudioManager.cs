using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioMixer audioMixer;
    public Sound[] sounds;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.outputAudioMixerGroup = s.Output;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
        
        // Subscribe to audio events
        EventsManager.instance.Subscribe(GameEventType.PlaySound, OnPlaySound);
        EventsManager.instance.Subscribe(GameEventType.StopSound, OnStopSound);
    }
    
    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        EventsManager.instance.Unsubscribe(GameEventType.PlaySound, OnPlaySound);
        EventsManager.instance.Unsubscribe(GameEventType.StopSound, OnStopSound);
    }
    
    private void OnPlaySound(GameEvent gameEvent)
    {
        string soundName = gameEvent.data as string;
        if (!string.IsNullOrEmpty(soundName))
        {
            PlaySound(soundName);
        }
    }
    
    private void OnStopSound(GameEvent gameEvent)
    {
        string soundName = gameEvent.data as string;
        if (!string.IsNullOrEmpty(soundName))
        {
            StopSound(soundName);
        }
    }

    private void Start()
    {
        FindObjectOfType<AudioManager>().PlaySound("Wind");
    }

    public void PlaySound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + "cannot be found");
            return;
        }

        s.source.Play();
    }

    public void StopSound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + "cannot be found");
            return;
        }

        s.source.Stop();
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("volume", volume);
    }
}