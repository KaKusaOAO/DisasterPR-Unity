using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class WordCardLock : MonoBehaviour
{
    public Image image;
    public Sprite lockedSprite;
    public Sprite normalSprite;
    public bool locked;
    public float lockedAlpha = 1f;
    public float normalAlpha = 0.25f;

    void Update()
    {
        image.sprite = locked ? lockedSprite : normalSprite;

        var color = image.color;
        color.a = locked ? lockedAlpha : normalAlpha;
        image.color = color;
    }
}