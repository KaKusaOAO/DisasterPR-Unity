using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance => _instance;

    public AudioMixer audioMixer;

    public GameObject oneShotAudioPrefab;
    public AudioClip buttonFX;
    public AudioClip loginSuccessFX;
    public AudioClip loginFailedFX;
    public AudioClip joinedRoomFX;
    public AudioClip leftRoomFX;
    public AudioClip wordSelectFX;
    public AudioClip chosenWordSelectFX;
    public AudioClip commitFinalFX;
    public AudioClip popFX;
    public AudioClip topicAppearFX;
    public AudioClip topicChooseFX;
    public AudioClip rewardFX;
    public AudioClip finalChooseFX;
    public AudioClip chatFX;
    public AudioClip partyFX;
    public AudioClip errorFX;
    public AudioClip playerAppearedFX;
    public AudioClip playerRemovedFX;
    public AudioClip volumeAdjustFX;

    // Start is called before the first frame update
    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        _instance = this;
    }

    public void PlayOneShot(AudioClip clip)
    {
        var obj = Instantiate(oneShotAudioPrefab);
        var source = obj.GetComponent<AudioSource>();
        source.clip = clip;
        source.Play();
    }
}
