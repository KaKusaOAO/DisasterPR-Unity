using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class WordFinalAnim : MonoBehaviour
{
    public Vector3 desiredPos;
    public Vector3 sourcePos;
    private float _startTime;

    void Start()
    {
        _startTime = Time.time;
    }

    void Update()
    {
        var progress = Mathf.Clamp(Time.time - _startTime, 0, 1);
        progress = Mathf.Pow(progress, 1 / 3f);
        var newPos = Vector3.Lerp(sourcePos, desiredPos, progress);
        transform.position = newPos;

        var scale = Mathf.Lerp(1, 0.1f, progress);
        transform.localScale = new Vector3(scale, scale);

        if (progress < 1) return;
        
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.rewardFX);
        
        var obj = new GameObject();
        obj.transform.SetParent(UIManager.Instance.canvas.transform);
        
        var emitter = obj.AddComponent<CreditParticleEmitter>();
        emitter.desiredPos = desiredPos;
        Destroy(gameObject);
    }
}
