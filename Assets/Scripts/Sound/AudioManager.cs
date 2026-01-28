using UnityEngine.Audio;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SamuraiStandoff
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;
        public AudioMixer audioMixer;
        public Sound[] sounds;
        [SerializeField] private GameData gameData;

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
            
            LoadVolumeFromPlayerData("MasterVolume", gameData.masterVolume);
            LoadVolumeFromPlayerData("BackgroundVolume", gameData.backgroundVolume);
        }

        public void GetSound(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("getting sounds");
            
            foreach (Sound sound in sounds)
            {
                if(sound.source.isPlaying)
                {
                    StopSound(sound.name);
                }
                else
                {
                    continue;
                }
            }
            if(scene.name == "MainMenu" || scene.name == "SingleplayerResults" || scene.name == "MultiplayerResults")
            {
                PlaySound("Menu");
            }
            if(scene.name == "SingleplayerLevel" || scene.name == "MultiplayerLevel")
            {
                PlaySound("Fight");
            }
            
        }
        public void PlaySound(string soundName)
        {
            Sound s = Array.Find(sounds, sound => sound.name == soundName);

            if (s == null)
            {
                Debug.LogWarning("Sound: " + soundName + "cannot be found");
                return;
            }

            if (s.source.isPlaying) return;
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
        
        private void LoadVolumeFromPlayerData(string mixerParam, float savedValue)
        {
            float linearVolume = Mathf.Clamp(savedValue, 1f, 100f); // Clamp to avoid invalid values
            float normalized = linearVolume / 100f; // Normalize to 0–1

            // Convert to decibels:
            // At normalized = 1 → 0 dB
            // At normalized = 0.0001 → about -80 dB (we’ll clamp to -60)
            float dB = Mathf.Log10(normalized) * 20f;
            
            dB = Mathf.Clamp(dB, -60f, 0f); // Clamp to your desired range (-60 dB to 0 dB)

            audioMixer.SetFloat(mixerParam, dB);
        }

        void OnEnable()
        {
            Debug.Log("OnEnable called");
            SceneManager.sceneLoaded += GetSound;
        }        


        void OnDestroy()
        {
            Debug.Log("OnDisable");
            SceneManager.sceneLoaded -= GetSound;
        }
    }
}