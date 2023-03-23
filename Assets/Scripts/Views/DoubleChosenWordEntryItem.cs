using System.Threading.Tasks;
using DisasterPR.Net.Packets.Play;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class DoubleChosenWordEntryItem : MonoBehaviour, IChosenWordEntryItem
{
    public Image image;
    public Sprite normalSprite;
    public Sprite selectedSprite;
    public WordCardHolder holder;
    public bool revealed;
    public bool selected;

    public GameObject hoverIndicator;
    public GameObject hiddenPart;
    public GameObject revealedPart;

    public TMP_Text upperText;
    public TMP_Text lowerText;
    
    public WordCardHolder Holder
    {
        get => holder;
        set => holder = value;
    }
    
    public bool IsRevealed
    {
        get => revealed;
        set => revealed = value;
    }

    public bool IsSelected
    {
        get => selected;
        set => selected = value;
    }

    public bool IsEnabled
    {
        get => enabled;
        set => enabled = value;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        image.sprite = selected ? selectedSprite : normalSprite;

        if (holder != null)
        {
            var session = GameManager.Instance.Session;
            if (session == null) return;

            var index = holder.index;
            var words = session.LocalGameState.CurrentChosenWords[index];
            revealed = words.IsRevealed;
            selected = ScreenManager.Instance.gameScreen.LocalChosenFinalIndex == index;

            upperText.text = words.Words[0].Label;
            lowerText.text = words.Words[1].Label;
        }

        hiddenPart.SetActive(!revealed);
        revealedPart.SetActive(revealed);
    }

    void OnMouseEnter()
    {
        hoverIndicator.SetActive(true);
    }

    void OnMouseExit()
    {
        hoverIndicator.SetActive(false);
    }
    
    public void OnButtonClicked()
    {
        if (holder == null) return;
        holder.OnClickedReveal();
    }
}