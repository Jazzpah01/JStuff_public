using UnityEngine;

namespace JStuff.Utilities
{
    public class AudioPlay : MonoBehaviour {

        public static float volume = 1;

        private AudioSource tempAudioSound;
        private AudioSource tempAudioMusic;

        public static AudioSource audioSound;

        public static float musicVolume;
        public static float soundVolume;

        void Start()
        {
            tempAudioSound = this.gameObject.GetComponent<AudioSource>();
            if (tempAudioSound == null)
            {
                tempAudioSound = this.gameObject.AddComponent<AudioSource>();
                tempAudioSound.loop = false;
                tempAudioSound.volume = 1;
            }

            audioSound = tempAudioSound;

            //audioSound = tempAudioSound;
            //AudioSource audio = this.gameObject.GetComponent<AudioSource>();
        }
    
        public static void PlaySound(string soundToPlay, float volume)
        {
            if (volume < 0 || volume > 1)
                throw new System.Exception("Volume has to be between 0 and 1. (including)");

            AudioClip sound = (AudioClip)Resources.Load("Sounds/" + soundToPlay);
            Debug.Log(sound);
            audioSound.PlayOneShot(sound, volume * AudioPlay.volume);
        }

        public static GameObject CreateSoundSource(string soundToPlay, float volume, bool loop = false)
        {
            if (volume < 0 || volume > 1)
                throw new System.Exception("Volume has to be between 0 and 1! (including)");

            GameObject retval = (GameObject)Instantiate(Resources.Load("Prefabs/SoundSource"));
            AudioSource source = retval.GetComponent<AudioSource>();
            source.clip = (AudioClip)Resources.Load("Sounds/" + soundToPlay);

            if (source.clip == null)
                throw new System.Exception("The sound doesn't exist in the Sounds folder!");

            source.volume = volume * AudioPlay.volume;
            source.loop = loop;
            source.Play();
            Debug.Log("Sounds/" + soundToPlay + "  --  " + source.clip);
            return retval;
        }

        public static void ChangeSoundOnSource(GameObject soundSource, string soundToPlay, float volume, bool loop = false)
        {
            if (volume < 0 || volume > 1)
                throw new System.Exception("Volume has to be between 0 and 1! (including)");

            AudioSource source = soundSource.GetComponent<AudioSource>();
            AudioClip clip = (AudioClip)Resources.Load("Sounds/" + soundToPlay);

            Debug.Log(";"+clip.ToString() + "---" + source.clip.ToString()+";");
            source.volume = volume * AudioPlay.volume;
            source.loop = loop;

            if (clip.ToString() != source.clip.ToString())
            {
                source.clip = clip;
                source.Play();
            }
        }

        public static void RemoveSoundSource(GameObject soundSource)
        {
            if (soundSource.GetComponent<AudioSource>() == null)
                throw new System.Exception("soundSource is not a SoundSource prefab!");

            Destroy(soundSource);
        }

        public static void PlayMusic(string musicToPlay)
        {

        }
    }
}