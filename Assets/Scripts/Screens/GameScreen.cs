using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DisasterPR;
using DisasterPR.Net.Packets.Play;
using DisasterPR.Sessions;
using Mochi.Utils.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameScreen : MonoBehaviour, IScreen
{
    private GameObject[] _frames;
    public GameObject waitChooseTopicFrame;
    public GameObject chooseTopicFrame;
    public GameObject topicFrame;

    public RectTransform playerListContainer;
    public GameObject playerItemPrefab;

    public TMP_Text timerText;
    public TMP_Text topicText;
    public TMP_Text cycleText;
    public TMP_Text currentPlayerText;

    public WordCardLayout chosenWordsLayout;
    public GameObject wordHolderPrefab;

    public WordCardLayout holdingWordsLayout;
    private RectTransform _holdingWordsTransform;
    public GameObject holdingWordPrefab;

    public GameObject chooseWordButton;
    public GameObject chooseFinalButton;
    public TMP_Text roomIdHintText;

    private Vector2 _holdingWordsPosition0;

    private List<WordCardHolder> _holders = new();
    public bool previewTopic = false;

    public List<HoldingWordCardEntry> LocalSelectedWords { get; } = new();
    public int LocalChosenFinalIndex { get; set; } = -1;

    void Awake()
    {
        _holdingWordsTransform = holdingWordsLayout.GetComponent<RectTransform>();
        _holdingWordsPosition0 = _holdingWordsTransform.anchoredPosition;
        
    }

    void FixedUpdate()
    {
        var manager = GameManager.Instance;
        if (manager == null) return;

        var player = manager.Player;
        var session = player?.Session;
        if (session == null) return;

        var chosen = session.GameState.CurrentChosenWords
            .Find(c => c.PlayerId == GameManager.Instance.Player!.Id);
        var canShowChatBox = session.GameState.CurrentState != StateOfGame.ChoosingWord ||
                             session.GameState.CurrentPlayer == player || chosen != null;
        _holdingWordsTransform.anchoredPosition = Vector2.Lerp(_holdingWordsTransform.anchoredPosition,
            new Vector2(0, canShowChatBox ? -500 : 0) + _holdingWordsPosition0, 1 - Time.fixedDeltaTime * 40);
    }

    void Update()
    {
        var manager = GameManager.Instance;
        if (manager == null) return;

        var player = manager.Player;
        var session = player?.Session;
        if (session == null) return;
        
        var chosen = session.GameState.CurrentChosenWords
            .Find(c => c.PlayerId == GameManager.Instance.Player!.Id);

        var canShowChatBox = session.GameState.CurrentState != StateOfGame.ChoosingWord ||
                             session.GameState.CurrentPlayer == player || chosen != null;
        UIManager.Instance.chatBox.SetActive(canShowChatBox);
        
        if (session.GameState.CurrentState == StateOfGame.ChoosingWord)
        {
            var topic = session.GameState.CurrentTopic;
            var sb = new StringBuilder();
            sb.Append(topic.Texts.First());
            
            for (var i = 0; i < topic.AnswerCount; i++)
            {
                if (!previewTopic || i >= LocalSelectedWords.Count || canShowChatBox)
                {
                    sb.Append("\ue999\ue999\ue999\ue999");
                }
                else
                {
                    var word = LocalSelectedWords[i].Card;
                    sb.Append($"<color=#128c85>{word.Label}</color>");
                }

                sb.Append(topic.Texts[i + 1]);
            }

            topicText.text = sb.ToString();
        }

        chooseWordButton.SetActive(LocalSelectedWords.Any());
        chooseFinalButton.SetActive(session.GameState.CurrentPlayer == player && LocalChosenFinalIndex != -1);

        cycleText.text = $"第\n<b>{session.GameState.RoundCycle}</b>\n輪";

        var currPlayer = session.GameState.CurrentPlayer;
        var currPlayerName = currPlayer == player ? "你" : currPlayer.Name;
        currentPlayerText.text = $"值日生：<b>{currPlayerName}</b>";

        if (roomIdHintText != null)
        {
            roomIdHintText.text = $"房號：{session.RoomId}";
        }
    }

    public void SwitchToFrame(GameObject frame)
    {
        if (_frames == null)
        {
            _frames = new[]
            {
                waitChooseTopicFrame, chooseTopicFrame, topicFrame
            };
        }
        
        if (!_frames.Contains(frame)) return;
        Debug.Log($"Switching to frame {frame.name}");
        
        foreach (var f in _frames)
        {
            f.SetActive(false);
        }

        frame.SetActive(true);
        holdingWordsLayout.gameObject.SetActive(frame == topicFrame);
        if (frame == topicFrame)
        {
            var player = GameManager.Instance.Player;
            var session = player!.Session;
            if (session == null) return;
            
            topicText.text = session.GameState.CurrentTopic.Texts
                .JoinStrings(" \ue999\ue999\ue999\ue999 ");

            LocalSelectedWords.Clear();
            LocalChosenFinalIndex = -1;
            chosenWordsLayout.DestroyAll();
            _holders.Clear();
            
            for (var i = 0; i < session.Players.Count - 1; i++)
            {
                var holder = Instantiate(wordHolderPrefab, UIManager.Instance.canvas.transform);
                var h = holder.GetComponent<WordCardHolder>();
                h.index = i;
                h.count = session.GameState.CurrentTopic.AnswerCount;
                chosenWordsLayout.AddItem(holder);
                holder.transform.localRotation = Quaternion.Euler(0, 0, 0);
                _holders.Add(h);
            }
            
            holdingWordsLayout.gameObject.SetActive(session.GameState.CurrentPlayer != player);
        }
    }

    public void UpdateWords()
    {
        // Clear the selected words
        LocalSelectedWords.Clear();
        
        // Prepare the UI
        holdingWordsLayout.DestroyAll();
        
        var player = GameManager.Instance.Player;
        if (player == null) return;

        var index = 0;
        foreach (var _ in player.HoldingCards)
        {
            var item = Instantiate(holdingWordPrefab, UIManager.Instance.canvas.transform);
            var card = item.GetComponent<HoldingWordCard>();
            card.index = index;
            holdingWordsLayout.AddItem(item);
            item.transform.localRotation = Quaternion.Euler(0, 0, 0);
            index++;
        }
    }

    public void OnAddChosenWord(Guid guid)
    {
        var player = GameManager.Instance.Player;
        var session = player!.Session;
        if (session == null) return;

        var words = session.GameState.CurrentChosenWords;
        var index = words.FindIndex(c => c.Id == guid);
        #if UNITY_EDITOR
        Debug.Log($"Appeared chosen word: " +
                  $"{words[index].Words.Select(w => w.Label).JoinStrings(", ")}");
        #endif
        _holders[index].OnChosenAppear();
    }

    public void UpdateTimer(int timer)
    {
        timerText.text = timer.ToString();
        timerText.color = timer > 5 ? Color.black : GameConstants.Red;
    }

    public void OnTransitionedIn()
    {
        ScreenManager.Instance.SwitchToStaticBackground();

        for (var c = 0; c < playerListContainer.childCount; c++)
        {
            var child = playerListContainer.GetChild(c);
            Destroy(child.gameObject);
        }
        
        var session = GameManager.Instance.Session!;
        var i = 0;
        foreach (var _ in session.Players)
        {
            var item = Instantiate(playerItemPrefab, playerListContainer);
            var it = item.GetComponent<InGamePlayerEntry>();
            it.index = i;
            i++;
        }
    }

    public void SubmitSelectedWords()
    {
        var manager = GameManager.Instance;
        if (manager == null) return;
        
        var player = manager.Player;
        var session = player?.Session;
        if (session == null) return;

        if (LocalSelectedWords.Any(w => w.IsLocked))
        {
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.errorFX);
            UIManager.Instance.AddSystemToast("不能提交已鎖定的卡片！").SetErrorStyle();
            return;
        }

        var count = session.GameState.CurrentTopic.AnswerCount;
        if (LocalSelectedWords.Count != count)
        {
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.errorFX);
            UIManager.Instance.AddSystemToast($"提交的卡片數量不正確，需要 {count} 張。").SetErrorStyle();
            return;
        }

        var indices = LocalSelectedWords.Select(w => player.HoldingCards.IndexOf(w)).ToHashSet();
        session.LocalGameState.ChooseWord(indices);
        LocalSelectedWords.Clear();
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.wordSelectFX);
    }

    public void SubmitSelectedFinal()
    {
        var manager = GameManager.Instance;
        if (manager == null) return;
        
        var player = manager.Player;
        var session = player?.Session;
        if (session == null) return;
        
        session.LocalGameState.ChooseFinal(LocalChosenFinalIndex);
    }

    public void OnTransitionedOut()
    {
        ScreenManager.Instance.SwitchToFlowingBackground();
        UIManager.Instance.chatBox.SetActive(true);
    }

    public void ClientRequestShuffleCards()
    {
        var manager = GameManager.Instance;
        if (manager == null) return;
        
        var player = manager.Player;
        if (player == null) return;
        
        player.Connection.SendPacket(new ServerboundRequestShuffleWordsPacket());
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.buttonFX);
    }

    public void OnRevealWord(Guid guid)
    {
        var manager = GameManager.Instance;
        if (manager == null) return;
        
        var player = manager.Player;
        var session = player?.Session;
        if (session == null) return;

        var topic = session.GameState.CurrentTopic;
        var chosen = session.GameState.CurrentChosenWords.Find(c => c.Id == guid);
        var sb = new StringBuilder();
        sb.Append(topic.Texts.First());

        for (var i = 0; i < topic.AnswerCount; i++)
        {
            sb.Append($"<color=#128c85>{chosen.Words[i].Label}</color>");
            sb.Append(topic.Texts[i + 1]);
        }
        topicText.text = sb.ToString();
        LocalChosenFinalIndex = session.GameState.CurrentChosenWords.IndexOf(chosen);
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.chosenWordSelectFX);
    }

    public void OnSetFinal(int index)
    {
        var manager = GameManager.Instance;
        if (manager == null) return;
        
        var player = manager.Player;
        var session = player?.Session;
        if (session == null) return;

        AudioManager.Instance.PlayOneShot(AudioManager.Instance.commitFinalFX);
        
        var chosen = session.GameState.CurrentChosenWords[index];
        var playerEntries = FindObjectsOfType<InGamePlayerEntry>();
        var holders = chosenWordsLayout.Items
            .Select(o => o.GetComponent<WordCardHolder>())
            .Where(o => o != null).ToList();
        
        foreach (var entry in playerEntries)
        {
            var p = session.Players[entry.index];
            if (chosen.PlayerId != p.Id) continue;

            var holder = holders.Find(h => h.index == index);
            var clone = Instantiate(holder.gameObject, UIManager.Instance.canvas.transform);
            clone.transform.rotation = holder.transform.rotation;
            
            var source = holder.GetComponent<RectTransform>().position;
            var dest = entry.scoreText.rectTransform.position;
            clone.transform.position = source;
            clone.GetComponentInChildren<WordCardStack>()?.Disable();
            
            var anim = clone.AddComponent<WordFinalAnim>();
            anim.sourcePos = source;
            anim.desiredPos = dest;
        }
    }
    
    public void OnChangeToState(StateOfGame state)
    {
        var player = GameManager.Instance.Player;
        var audios = AudioManager.Instance;
        var session = player!.Session;
        var screens = ScreenManager.Instance;
        
        if (state == StateOfGame.Started)
        {
            audios.PlayOneShot(audios.loginSuccessFX);
            UIManager.Instance.AddTransitionStinger(() =>
            {
                ScreenManager.Instance.SwitchToPage(this);
            });
        }
        else if (state == StateOfGame.ChoosingTopic)
        {
            audios.PlayOneShot(audios.topicChooseFX);
            SwitchToFrame(player == session!.GameState.CurrentPlayer
                ? chooseTopicFrame
                : waitChooseTopicFrame);
        }
        else if (state == StateOfGame.ChoosingWord)
        {
            audios.PlayOneShot(audios.topicAppearFX);
            SwitchToFrame(topicFrame);
        }
        else if (state == StateOfGame.ChoosingFinal)
        {
            var topic = session!.GameState.CurrentTopic;
            var sb = new StringBuilder();
            sb.Append(topic.Texts.First());
            
            for (var i = 0; i < topic.AnswerCount; i++)
            {
                sb.Append("\ue999\ue999\ue999\ue999");
                sb.Append(topic.Texts[i + 1]);
            }

            topicText.text = sb.ToString();
            audios.PlayOneShot(audios.finalChooseFX);
        }
        else if (state == StateOfGame.WinResult)
        {
            UIManager.Instance.AddTransitionStinger(() =>
            {
                screens.SwitchToPage(screens.winScreen);
            });
        }

        LocalSelectedWords.Clear();

        if (state == StateOfGame.ChoosingTopic)
        {
            LocalChosenFinalIndex = -1;
        }
    }
}