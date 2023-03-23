using TMPro;
using UnityEngine;

public class VersionText : MonoBehaviour
{
    private TMP_Text _text;

    void Start()
    {
        _text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        _text.text = GameConstants.Version;
    }
}