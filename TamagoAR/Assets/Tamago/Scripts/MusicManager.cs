/**
 * Music controller for handling in game music
 */

using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioClip GameMusic;
    [SerializeField] private AudioClip RainSequenceMusic;
    [SerializeField] private float fadeInTimeSeconds = 2.5f;
    [SerializeField] private float fadeOutTimeSeconds = 2.5f;
    private const float MAX_VOLUME = 1.0f;
    private const float MIN_VOLUME = 0f;
    private AudioSource AudioSource;

    private static MusicManager ManagerInstance;

    public static MusicManager Instance
    {
        get { return ManagerInstance; }
    }

    public void PlayGameMusic()
    {
        StartCoroutine(FadeOutVolume());
        AudioSource.clip = GameMusic;
        StartCoroutine(FadeInVolume());
    }

    public void PlayRainSequenceMusic()
    {
        StartCoroutine(FadeOutVolume());
        AudioSource.clip = RainSequenceMusic;
        StartCoroutine(FadeInVolume());
    }

    private void Awake()
    {
        if (ManagerInstance != null && ManagerInstance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            ManagerInstance = this;
        }
    }

    private void Start()
    {
        AudioSource = GetComponent<AudioSource>();
    }

    private IEnumerator FadeInVolume()
    {
        AudioSource.Play();
        float startVolume = AudioSource.volume;
        while (AudioSource.volume < MAX_VOLUME)
        {
            AudioSource.volume += startVolume * Time.deltaTime / fadeInTimeSeconds;
            yield return null;
        }
    }

    private IEnumerator FadeOutVolume()
    {
        float startVolume = AudioSource.volume;
        while (AudioSource.volume > MIN_VOLUME)
        {
            AudioSource.volume -= startVolume * Time.deltaTime / fadeOutTimeSeconds;
            yield return null;
        }

        AudioSource.Stop();
    }
}