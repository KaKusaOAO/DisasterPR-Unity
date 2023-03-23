using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class ChatFlyout : MonoBehaviour
{
    public TMP_Text text;
    public string content;

    private RectTransform _transform;
    
    // Start is called before the first frame update
    void Start()
    {
        var rand = new Random();
        _transform = GetComponent<RectTransform>();

        var rect = UIManager.Instance.canvas.GetComponent<RectTransform>().rect;
        var y = rect.y / 2f + rand.Next(Mathf.RoundToInt(rect.height * 0.25f));
        _transform.anchoredPosition = new Vector2(rect.width, y);
    }

    // Update is called once per frame
    void Update()
    {
        _transform.anchoredPosition += new Vector2(-Time.deltaTime * 350, 0);
        text.text = content;

        if (-_transform.anchoredPosition.x > _transform.rect.width)
        {
            Destroy(gameObject);
        }
    }
}
