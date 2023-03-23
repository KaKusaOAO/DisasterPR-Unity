using System;
using System.Collections.Generic;
using UnityEngine;

public class WordCardLayout : MonoBehaviour
{
    public GameObject upperStack;
    public GameObject lowerStack;

    public List<GameObject> Items { get; } = new();

    void Start()
    {
        RearrangeItems();
    }

    void Update()
    {
        
    }

    public void RearrangeItems()
    {
        var count = Items.Count;
        lowerStack.SetActive(count > 4);

        if (Items.Count == 0) return;
        var upper = count <= 4 ? 4 : Mathf.FloorToInt(count / 2f);
        var index = 0;
        for (var i = 0; i < Math.Min(count, upper); i++, index++)
        {
            var item = Items[index];
            item.transform.SetParent(upperStack.transform);
            item.transform.localScale = Vector3.one;
        }

        if (count <= 4) return;
        
        var lower = Mathf.CeilToInt(count / 2f);
        for (var i = 0; i < lower; i++, index++)
        {
            var item = Items[index];
            item.transform.SetParent(lowerStack.transform);
            item.transform.localScale = Vector3.one;
        }
    }

    public void AddItem(GameObject item)
    {
        Items.Add(item);
        RearrangeItems();
    }

    public void RemoveItem(GameObject item)
    {
        Items.Remove(item);
        RearrangeItems();
    }

    public void DestroyAll()
    {
        foreach (var item in Items)
        {
            Destroy(item);
        }
        
        Items.Clear();
        RearrangeItems();
    }
}