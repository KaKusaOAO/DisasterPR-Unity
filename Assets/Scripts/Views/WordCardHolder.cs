using System.Threading.Tasks;
using DisasterPR.Net.Packets.Play;
using UnityEngine;
using UnityEngine.UI;

public class WordCardHolder : MonoBehaviour
{
    public int index;
    public int count;
    public Image image;
    public GameObject wordCardStackPrefab;
    private GameObject _created;
    private Vector3 _desiredPos;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        image.enabled = !Item?.IsSelected ?? true;
    }

    void FixedUpdate()
    {
        if (_created == null) return;

        _created.transform.localPosition = Vector3.Lerp(_created.transform.localPosition,
            _desiredPos + new Vector3(Mathf.Sin(Time.time) * 10, 0, 0), 1 - Time.fixedDeltaTime * 40);
    }

    public void OnChosenAppear()
    {
        _created = Instantiate(wordCardStackPrefab, transform);
        
        var item = Item;
        item.Holder = this;
        item.Init();
        
        _desiredPos = _created.transform.localPosition;
        _created!.transform.localPosition += new Vector3(-500, 0, 0);
    }

    // ReSharper disable once Unity.NoNullPropagation
    public IChosenWordEntryItem Item => _created?.GetComponent<WordCardStack>();

    public void OnClickedReveal()
    {
        var player = GameManager.Instance.Player;
        var session = player?.Session;
        if (session == null) return;
        if (session.GameState.CurrentPlayer != player) return;
        
        var entry = session.GameState.CurrentChosenWords[index];
        player.Connection.SendPacket(new ServerboundRevealChosenWordEntryPacket(entry.Id));
    }
}