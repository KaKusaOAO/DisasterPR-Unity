using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DisasterPR;
using DisasterPR.Net.Packets.Play;
using KaLib.Utils.Extensions;
using TMPro;
using UnityEngine;

public class TopicPaper : MonoBehaviour
{
    public TMP_Text displayText;
    public HorizontalSide side;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var manager = GameManager.Instance;
        var session = manager.Session;
        if (session == null) return;

        var topics = session.GameState.CandidateTopics;
        if (!topics.HasValue)
        {
            displayText.text = "???";
            return;
        }
        
        var topic = side == HorizontalSide.Left ? topics.Value.Left : topics.Value.Right;
        var text = topic.Texts
            .Select(s => s.Replace("<", "<<i></i>"))
            .JoinStrings("\ue999\ue999\ue999\ue999");
        displayText.text = text;
    }

    public void OnButtonClicked()
    {
        var manager = GameManager.Instance;
        manager.Connection?.SendPacket(new ServerboundChooseTopicPacket(side));
    }
}
