using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DisasterPR.Net.Packets.Play;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatButton : MonoBehaviour
{
    public TMP_InputField input;
    private Button _button;
    
    // Start is called before the first frame update
    void Start()
    {
        _button = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        var isValid = !string.IsNullOrWhiteSpace(input.text.Trim());
        _button.interactable = isValid;
    }

    public void OnButtonClicked()
    {
        var conn = GameManager.Instance.Connection!;
        var content = input.text;
        input.text = "";
        
        conn.SendPacket(new ServerboundChatPacket(content));
    }
}
