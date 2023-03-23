using TMPro;
using UnityEngine;

public class InGamePlayerEntry : MonoBehaviour
{
    public int index;
    public TMP_Text scoreText;
    public TMP_Text nameText;
    public GameObject indicator;
    
    void Start()
    {
        
    }

    void Update()
    {
        var manager = GameManager.Instance;
        var session = manager.Session;
        if (session == null) return;
        if (index < 0 || index >= session.Players.Count) return;

        var player = session.Players[index];
        scoreText.text = player.Score.ToString();
        nameText.text = player.Name;
        indicator.SetActive(session.GameState.CurrentPlayer == player);
    }
}