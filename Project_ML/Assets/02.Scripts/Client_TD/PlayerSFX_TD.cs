using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerSFX_TD : MonoBehaviour
{
    public AudioMixerGroup sfxGroup;

    public AudioClip footstepClip;
    public AudioClip jumpClip;
    public AudioClip attackClip;

    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource oneshotSource;

    void Awake()
    {
        if (!footstepSource)
        {
            footstepSource = gameObject.AddComponent<AudioSource>();
            footstepSource.playOnAwake = false;
            footstepSource.loop = false;
            footstepSource.spatialBlend = 1f;
            footstepSource.ignoreListenerPause = false;
        }

        if (!oneshotSource)
        {
            oneshotSource = gameObject.AddComponent<AudioSource>();
            oneshotSource.playOnAwake = false;
            oneshotSource.loop = false;
            oneshotSource.spatialBlend = 1f;
            footstepSource.ignoreListenerPause = false;
        }

        if (sfxGroup)
        {
            footstepSource.outputAudioMixerGroup = sfxGroup;
            oneshotSource.outputAudioMixerGroup = sfxGroup;
        }
    }

    public void SetMoving(bool moving)
    {
        if (moving)
        {
            if (!footstepClip) return;

            if (!footstepSource.isPlaying)
            {
                footstepSource.clip = footstepClip;
                footstepSource.Play();
            }
        }
        else
        {
            StopFootstep();
        }
    }

    public void StopFootstep()
    {
        if (footstepSource.isPlaying)
            footstepSource.Stop();
    }

    public void PauseAll()
    {
        if (footstepSource.isPlaying) footstepSource.Pause();
        if (oneshotSource.isPlaying) oneshotSource.Pause();
    }

    public void ResumeAll()
    {
        if (footstepSource.clip == footstepClip) footstepSource.UnPause();
        oneshotSource.UnPause();
    }

    public void PlayJump()
    {
        if (jumpClip) oneshotSource.PlayOneShot(jumpClip);
    }

    public void PlayAttack()
    {
        if (attackClip) oneshotSource.PlayOneShot(attackClip);
    }
}
