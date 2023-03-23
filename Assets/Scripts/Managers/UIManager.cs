using System;
using UnityEngine;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance => _instance;

    public Canvas canvas;
    public RectTransform chatFlyoutContainer;
    public GameObject chatBox;
    
    public GameObject transitionStingerPrefab;
    public GameObject chatFlyoutPrefab;

    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddTransitionStinger(Action transition)
    {
        var obj = Instantiate(transitionStingerPrefab, canvas.transform);
        var t = obj.GetComponent<TransitionStinger>();
        t.OnTrasition = transition;
    }

    public void AddChatFlyout(string text)
    {
        var obj = Instantiate(chatFlyoutPrefab, chatFlyoutContainer);
        var f = obj.GetComponent<ChatFlyout>();
        f.content = text;
    }
}