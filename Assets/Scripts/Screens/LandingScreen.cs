using TMPro;
using UnityEngine;

public class LandingScreen : MonoBehaviour, IScreen
{
    public TMP_Text statusText;
    public NameSubmitButton button;
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnTransitionedIn()
    {
        statusText.gameObject.SetActive(false);
        UIManager.Instance.chatBox.SetActive(false);
    }

    public void OnTransitionedOut()
    {
        UIManager.Instance.chatBox.SetActive(true);
    }
}