using System.Collections.Generic;
using UnityEngine;

public class WordCardStack : MonoBehaviour, IChosenWordEntryItem
{
    private List<SingleChosenWordEntryItem> items = new();
    public bool revealed;
    public bool selected;
    public GameObject singleWordEntryPrefab;
    public Transform stackContainer;
    
    void Start()
    {
        
    }
    
    void Update()
    {
        if (Holder == null) return;
        
        var session = GameManager.Instance.Session;
        if (session == null) return;
        
        var index = Holder.index;
        var words = session.LocalGameState.CurrentChosenWords[index];
        revealed = words.IsRevealed;
        selected = ScreenManager.Instance.gameScreen.LocalChosenFinalIndex == index;
        
        var i = 0;
        foreach (var item in items)
        {
            item.revealed = revealed;
            item.selected = selected;
            item.text.text = words.Words[i++].Label;
        }
    }

    public void Init()
    {
        foreach (var item in items)
        {
            Destroy(item.gameObject);
        }
        items.Clear();

        for (var i = 0; i < stackContainer.childCount; i++)
        {
            var child = stackContainer.GetChild(i);
            Destroy(child.gameObject);
        }

        for (var i = 0; i < Holder.count; i++)
        {
            var obj = Instantiate(singleWordEntryPrefab, stackContainer);
            obj.transform.localScale = Vector3.one;

            var card = obj.GetComponent<SingleChosenWordEntryItem>();
            items.Add(card);
        }
    }

    public WordCardHolder Holder { get; set; }
    
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

    public void Disable()
    {
        foreach (var item in items)
        {
            item.Disable();
        }
    }

    public void OnButtonClicked()
    {
        if (Holder == null) return;
        Holder.OnClickedReveal();
    }
}