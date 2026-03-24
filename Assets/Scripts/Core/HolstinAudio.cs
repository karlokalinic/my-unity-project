using UnityEngine;

public static class HolstinAudio
{
    public static void PlayOneShot(AudioClip clip, Transform source, float volume = 1f)
    {
        Vector3 position = source != null ? source.position : Vector3.zero;
        PlayOneShot(clip, position, volume);
    }

    public static void PlayOneShot(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (!Application.isPlaying || clip == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(clip, position, Mathf.Clamp01(volume));
    }
}
