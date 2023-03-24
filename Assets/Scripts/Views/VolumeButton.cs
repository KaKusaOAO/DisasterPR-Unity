using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Task = System.Threading.Tasks.Task;

public class VolumeButton : MonoBehaviour
{
    public Sprite[] sprites;
    public int value = 3;
    public Image image;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        value = Math.Clamp(value, 0, sprites.Length - 1);
        image.sprite = sprites[value];
    }

    public void OnButtonClicked()
    {
        value--;
        if (value < 0) value = sprites.Length - 1;

        var volume = (float)value / (sprites.Length - 1);
        volume = Mathf.Pow(volume, 1 / 3f);
        var audios = AudioManager.Instance;
        var mixer = audios.audioMixer;
        mixer.SetFloat("Volume", Mathf.Lerp(-80, 0, volume));

        StartCoroutine(VolumePlayFXSequence());
    }

    private IEnumerator VolumePlayFXSequence()
    {
        yield return new WaitForSeconds(0.125f);
        
        var audios = AudioManager.Instance;
        audios.PlayOneShot(audios.volumeAdjustFX);
    }
}
