using System;
using UnityEngine;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance => _instance;

    public Canvas canvas;
    public RectTransform chatFlyoutContainer;
    public RectTransform toastContainer;
    public RectTransform transitionStingerContainer;
    public GameObject chatBox;
    
    public GameObject transitionStingerPrefab;
    public GameObject chatFlyoutPrefab;
    public GameObject systemToastPrefab;

    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddTransitionStinger(Action transition)
    {
        var obj = Instantiate(transitionStingerPrefab, transitionStingerContainer);
        var t = obj.GetComponent<TransitionStinger>();
        t.OnTrasition = transition;
    }

    public void AddChatFlyout(string text)
    {
        var obj = Instantiate(chatFlyoutPrefab, chatFlyoutContainer);
        var f = obj.GetComponent<ChatFlyout>();
        f.content = text;
    }

    public SystemChatToast AddSystemToast(string text)
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.toastFX);
        var obj = Instantiate(systemToastPrefab, toastContainer);
        var f = obj.GetComponent<SystemChatToast>();
        f.content = text;
        return f;
    }
}