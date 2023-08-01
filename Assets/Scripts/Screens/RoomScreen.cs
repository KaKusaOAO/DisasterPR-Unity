using System;
using System.Linq;
using System.Threading.Tasks;
using DisasterPR.Cards;
using DisasterPR.Net.Packets.Play;
using DisasterPR.Sessions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomScreen : MonoBehaviour, IScreen
{
    public TMP_Text roomIdDisplay;

    public Transform categoryGrid;
    public Transform playerListContainer;

    public GameObject playerItemPrefab;
    public GameObject categoryTogglePrefab;

    public TMP_Text winScoreText;
    public Button winScoreAddButton;
    public Button winScoreRemoveButton;
    public TMP_Text roomPlayerCountText;
    public Button startButton;
    public Image startButtonImage;
    public Sprite startButtonLockSprite;
    public Sprite startButtonUnlockSprite;
    public Button[] countdownPlusButtons;
    public Button[] countdownMinusButtons;

    public TMP_Text topicTimeText;
    public TMP_Text answerTimeText;
    public TMP_Text finalTimeText;
    public TMP_Text countText;
    public GameObject lockAbilityToggle;
    public bool CountNeedsUpdate { get; set; }

    void Start()
    {
    }

    public void AddWinScore()
    {
        var game = GameManager.Instance.Game;
        if (game == null) return;

        var session = game.Player!.Session;
        if (session == null) return;

        UpdateWinScore(session.Options.WinScore + 1);
    }

    public void RemoveWinScore()
    {
        var game = GameManager.Instance.Game;
        if (game == null) return;

        var session = game.Player!.Session;
        if (session == null) return;

        UpdateWinScore(session.Options.WinScore - 1);
    }

    private void UpdateWinScore(int value)
    {
        var game = GameManager.Instance.Game;
        if (game == null) return;

        var session = game.Player!.Session;
        if (session == null) return;

        AudioManager.Instance.PlayOneShot(AudioManager.Instance.buttonFX);
        var score = Math.Clamp(value, 2, 9);
        session.Options.WinScore = score;
        
        var manager = GameManager.Instance;
        manager.Connection!.SendPacket(new ServerboundUpdateSessionOptionsPacket(manager.Session!));
    }

    public void MoreCountdownTimeClicked() => OffsetCountdownTime(1);
    public void LessCountdownTimeClicked() => OffsetCountdownTime(-1);

    private void OffsetCountdownTime(int offset)
    {
        var game = GameManager.Instance.Game;
        if (game == null) return;

        var session = game.Player!.Session;
        if (session == null) return;

        var option = session.Options.CountdownTimeSet;
        var target = CountdownTimeSet.TimeSets.Find(c =>
            c.AnswerChooseTime == option.AnswerChooseTime && c.TopicChooseTime == option.TopicChooseTime &&
            c.FinalChooseTime == option.FinalChooseTime);
        var index = CountdownTimeSet.TimeSets.IndexOf(target);
        var newIndex = Math.Clamp(index + offset, 0, CountdownTimeSet.TimeSets.Count - 1);
        
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.buttonFX);
        var manager = GameManager.Instance;
        var options = manager.Session!.Options;
        manager.Connection!.SendPacket(new ServerboundUpdateSessionOptionsPacket(
            options.WinScore, CountdownTimeSet.TimeSets[newIndex], options.EnabledCategories));
    }

    public void UpdateLockAbility(Toggle toggle)
    {
        var game = GameManager.Instance.Game;
        if (game == null) return;

        var session = game.Player!.Session;
        if (session == null) return;
        session.Options.CanLockCards = toggle.isOn;
        
        var manager = GameManager.Instance;
        manager.Connection!.SendPacket(new ServerboundUpdateSessionOptionsPacket(manager.Session!));
    }

    void Update()
    {
        var game = GameManager.Instance.Game;
        if (game == null) return;

        var session = game.Player!.Session;
        if (session == null) return;

        var isHost = session.HostPlayer == game.Player;
        winScoreAddButton.gameObject.SetActive(isHost);
        winScoreRemoveButton.gameObject.SetActive(isHost);
        
        foreach (var btn in countdownPlusButtons)
        {
            btn.gameObject.SetActive(isHost);
        }
        
        foreach (var btn in countdownMinusButtons)
        {
            btn.gameObject.SetActive(isHost);
        }

        roomIdDisplay.text = session.RoomId.ToString();

        var winScore = session.Options.WinScore;
        winScoreText.text = winScore.ToString();
        winScoreRemoveButton.interactable = winScore > 2;
        winScoreAddButton.interactable = winScore < 9;

        var players = session.Players;
        roomPlayerCountText.text = $"{players.Count}人";

        var canStart = players.Count(p => p.State == PlayerState.Ready) >= 3;
        var canStartIsHost = canStart && isHost;
        startButton.interactable = canStartIsHost;
        startButtonImage.sprite = canStart ? startButtonUnlockSprite : startButtonLockSprite;

        var timeSet = session.Options.CountdownTimeSet;
        topicTimeText.text = timeSet.TopicChooseTime.ToString();
        answerTimeText.text = timeSet.AnswerChooseTime.ToString();
        finalTimeText.text = timeSet.FinalChooseTime.ToString();

        var topicTime = timeSet.TopicChooseTime;
        foreach (var btn in countdownPlusButtons)
        {
            btn.interactable = topicTime < 60;
        }
        
        foreach (var btn in countdownMinusButtons)
        {
            btn.interactable = topicTime > 5;
        }
        
        lockAbilityToggle.SetActive(isHost);
        lockAbilityToggle.GetComponent<Toggle>().isOn = session.Options.CanLockCards;

        if (CountNeedsUpdate)
        {
            CountNeedsUpdate = false;
            var topicCount =
                session.CardPack!.FilteredTopicsByEnabledCategories(session.Options.EnabledCategories).Count;
            var wordCount =
                session.CardPack.FilteredWordsByEnabledCategories(session.Options.EnabledCategories).Count;
            countText.text = $"題目數量：{topicCount} / 答案數量：{wordCount}";
        }
    }

    public void OnAddPlayer(int index)
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.playerAppearedFX);
        var item = Instantiate(playerItemPrefab, playerListContainer);
        var it = item.GetComponent<PlayerItemEntry>();
        it.index = index;

        LayoutRebuilder.ForceRebuildLayoutImmediate(playerListContainer.GetComponent<RectTransform>());
    }

    public void OnRemovePlayer()
    {
        var session = GameManager.Instance.Session;
        if (session == null) return;
        
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.playerRemovedFX);
        for (var i = 0; i < playerListContainer.childCount; i++)
        {
            var child = playerListContainer.GetChild(i);
            var entry = child.GetComponent<PlayerItemEntry>();
            if (entry == null) continue;
            
            if (entry.index >= session.Players.Count)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void UpdateCardPack(CardPack pack)
    {
        for (var c = 0; c < categoryGrid.childCount; c++)
        {
            var child = categoryGrid.GetChild(c);
            Destroy(child.gameObject);
        }

        var i = 0;
        foreach (var _ in pack.Categories)
        {
            var toggle = Instantiate(categoryTogglePrefab, categoryGrid);
            var comp = toggle.GetComponent<CategoryToggle>();
            comp.index = i;
            i++;
        }

        CountNeedsUpdate = true;
    }

    public void RequestStartGame()
    {
        var game = GameManager.Instance.Game;
        if (game == null) return;

        var session = game.Player!.Session;
        if (session == null) return;
        
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.buttonFX);
        game.Player.Connection.SendPacket(new ServerboundRequestRoomStartPacket());
    }

    public void OnTransitionedIn()
    {
        for (var c = 0; c < playerListContainer.childCount; c++)
        {
            var child = playerListContainer.GetChild(c);
            Destroy(child.gameObject);
        }

        var game = GameManager.Instance.Game;
        if (game == null) return;

        var session = game.Player!.Session;
        if (session == null) return;

        for (var c = 0; c < playerListContainer.childCount; c++)
        {
            var child = playerListContainer.GetChild(c);
            Destroy(child.gameObject);
        }
        
        var i = 0;
        foreach (var _ in session.Players)
        {
            var item = Instantiate(playerItemPrefab, playerListContainer);
            var it = item.GetComponent<PlayerItemEntry>();
            it.index = i;
            i++;
        }

        CountNeedsUpdate = true;
        LayoutRebuilder.ForceRebuildLayoutImmediate(playerListContainer.GetComponent<RectTransform>());

        game.Player.State = PlayerState.Ready;
        game.Player.Connection.SendPacket(new ServerboundUpdatePlayerStatePacket(game.Player));
    }

    public void OnTransitionedOut()
    {
    }
}