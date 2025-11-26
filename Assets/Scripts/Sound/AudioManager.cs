using UnityEngine.Audio;
using System;
using UnityEngine;

namespace SamuraiStandoff
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;

        public AudioMixer audioMixer;
        public Sound[] sounds;

        //To add a sound effect: AudioManager.instance.PlaySound("");
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
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
                s.source.outputAudioMixerGroup = s.output;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
            }
        }

        private void Start()
        {
            PlaySound("Intro");
            // FindFirstObjectByType<AudioManager>().PlaySound("Wind");
        }

        public void PlaySound(string soundName)
        {
            Sound s = Array.Find(sounds, sound => sound.name == soundName);

            if (s == null)
            {
                Debug.LogWarning("Sound: " + soundName + "cannot be found");
                return;
            }

            s.source.Play();
        }

        public void StopSound(string soundName)
        {
            Sound s = Array.Find(sounds, sound => sound.name == soundName);

            if (s == null)
            {
                Debug.LogWarning("Sound: " + soundName + "cannot be found");
                return;
            }

            s.source.Stop();
        }

        public void SetVolume(float volume)
        {
            audioMixer.SetFloat("volume", volume);
        }
    }
}