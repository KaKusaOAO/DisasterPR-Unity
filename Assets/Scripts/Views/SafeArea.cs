using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class SafeArea : MonoBehaviour
{
    private Rect lastSafeArea;
    private RectTransform parentRectTransform;

    void Start()
    {
        parentRectTransform = GetComponentInParent<RectTransform>();
    }

    void Update()
    {
        ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        var safeAreaRect = Screen.safeArea; // new Rect(120, 25, Screen.width - 240, Screen.height - 25);
        safeAreaRect.y -= Screen.height - safeAreaRect.height;

        if (Screen.width == 0) return;
        var scaleRatio = parentRectTransform.rect.width / Screen.width;

        var left = safeAreaRect.xMin * scaleRatio;
        var right = -(Screen.width - safeAreaRect.xMax) * scaleRatio;
        var top = -safeAreaRect.yMin * scaleRatio;
        var bottom = (Screen.height - safeAreaRect.yMax) * scaleRatio;

        var rectTransform = GetComponent<RectTransform>();
        rectTransform.offsetMin = new Vector2(left, bottom);
        rectTransform.offsetMax = new Vector2(right, top);

        lastSafeArea = Screen.safeArea;
    }
}