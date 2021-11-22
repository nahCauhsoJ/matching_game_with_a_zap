using System.Collections;
using UnityEngine;

public class Sound : MonoBehaviour
{
    public static Sound main;
    void Awake() {main = this;}

    public AudioClip charge_up;
    public AudioClip charge_up_peak;
    public AudioClip discharge;
    public AudioClip flick;
    public AudioClip flap;
    public AudioClip fry;
    public AudioClip game_over;
    public AudioClip tick;

    public static void Play(AudioSource source, AudioClip clip, float volume=1f, float pitch=1f, float delay=0f)
    {
        if (delay > 0) { main.StartPlayDelayed(source,clip,volume,pitch,delay); return; }
        source.pitch = pitch;
        source.PlayOneShot(clip,volume);
    }

    // Can't start Coroutine in static method. Hence this.
    void StartPlayDelayed(AudioSource source, AudioClip clip, float volume, float pitch, float delay)
    {
        StartCoroutine(PlayDelayed(source,clip,volume,pitch,delay));
    }
    IEnumerator PlayDelayed(AudioSource source, AudioClip clip, float volume, float pitch, float delay)
    {
        yield return new WaitForSeconds(delay);
        Play(source,clip,volume,pitch,0f);
    }
}
