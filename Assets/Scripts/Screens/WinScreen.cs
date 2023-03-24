using System.Collections;
using System.Threading.Tasks;
using DisasterPR.Net.Packets.Play;
using DisasterPR.Sessions;
using TMPro;
using UnityEngine;

public class WinScreen : MonoBehaviour, IScreen
{
    public TMP_Text playerNameText;
    
    void Start()
    {
        
    }

    void Update()
    {
        var session = GameManager.Instance.Session;
        if (session == null) return;

        var winner = session.GameState.WinnerPlayer;
        playerNameText.text = winner?.Name ?? "???";
    }

    public void OnBackClicked()
    {
        var manager = GameManager.Instance;
        if (manager.Connection == null) return;
        manager.Connection.SendPacket(new ServerboundLeaveRoomPacket());
        
        UIManager.Instance.AddTransitionStinger(() =>
        {
            var screens = ScreenManager.Instance;
            screens.SwitchToPage(screens.lobbyScreen);
        });
    }

    public void OnReplayClicked()
    {
        UIManager.Instance.AddTransitionStinger(() =>
        {
            ScreenManager.Instance.SwitchToPage(ScreenManager.Instance.roomScreen);
        });
    }

    private IEnumerator PartySequence()
    {
        yield return new WaitForSeconds(1f);
        MakePartyParticles();
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.partyFX);
    }

    public void OnTransitionedIn()
    {
        ScreenManager.Instance.SwitchToSpinnerBackground();
        StartCoroutine(PartySequence());
    }

    public void MakePartyParticles()
    {
        var emitter = new GameObject();
        var e = emitter.AddComponent<PartyParticleEmitter>();
        e.desiredPos = playerNameText.transform.position;
        emitter.transform.SetParent(UIManager.Instance.canvas.transform);
        emitter.transform.localScale = Vector3.one;
    }

    public void OnTransitionedOut()
    {
        ScreenManager.Instance.SwitchToFlowingBackground();
    }
}