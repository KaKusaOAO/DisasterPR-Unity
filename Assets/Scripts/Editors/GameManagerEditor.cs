#if UNITY_EDITOR
using System.Linq;
using KaLib.Utils.Extensions;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    void OnEnable()
    {
        
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var manager = GameManager.Instance;
        var session = manager?.Session;

        if (session == null)
        {
            GUILayout.Label("No session");
            Repaint();
            return;
        }
        
        GUILayout.Label($"Session #{session.RoomId}");
        GUILayout.Label("Players: \n" +
                        $"{session.Players.Select(p => $"{p.Score}: {p.Name} ({p.Id})").JoinStrings("\n")}");

        GUILayout.Label($"Topic: {session.GameState.CurrentTopic?.Texts.JoinStrings("____") ?? "<null>"}");
        
        var chosenWords = session.GameState.CurrentChosenWords
            .Select(c => "- [" + c.Words.Select(w => w.Label).JoinStrings(", ") + $"] {c.IsRevealed}")
            .JoinStrings("\n");
        GUILayout.Label($"ChosenWords: \n{chosenWords}");
        Repaint();
    }
}
#endif