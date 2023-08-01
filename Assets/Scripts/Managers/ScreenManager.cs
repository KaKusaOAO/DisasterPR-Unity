using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    private static ScreenManager _instance;
    public static ScreenManager Instance => _instance;

    private IScreen[] _screens;
    public LandingScreen landingScreen;
    public LobbyScreen lobbyScreen;
    public RoomScreen roomScreen;
    public GameScreen gameScreen;
    public WinScreen winScreen;

    private GameObject[] _backgrounds;
    public GameObject flowingBackground;
    public GameObject staticBackground;
    public GameObject spinnerBackground;
    
    void Awake()
    {
        _instance = this;
        _screens = new IScreen[]
        {
            landingScreen, lobbyScreen, roomScreen, gameScreen, winScreen
        };

        _backgrounds = new[]
        {
            flowingBackground, staticBackground, spinnerBackground
        };
        
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SwitchToPage(landingScreen);
    }

    public void SwitchToPage(IScreen page)
    {
        if (!_screens.Contains(page)) return;
        
        foreach (var screen in _screens)
        {
            if (((MonoBehaviour)screen).gameObject.activeInHierarchy)
            {
                screen.OnTransitionedOut();
            }

            ((MonoBehaviour)screen).gameObject.SetActive(false);
        }
        
        ((MonoBehaviour)page).gameObject.SetActive(true);
        page.OnTransitionedIn();
    }
    
    public void SwitchToBackground(GameObject background)
    {
        if (!_backgrounds.Contains(background)) return;
        
        foreach (var bg in _backgrounds)
        {
            bg.SetActive(false);
        }

        background.SetActive(true);
    }

    public void SwitchToFlowingBackground() => SwitchToBackground(flowingBackground);
    public void SwitchToStaticBackground() => SwitchToBackground(staticBackground);
    public void SwitchToSpinnerBackground() => SwitchToBackground(spinnerBackground);
}