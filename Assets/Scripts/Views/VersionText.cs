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
        var env = Application.platform switch
        {
            RuntimePlatform.WebGLPlayer => "WebGL",
            RuntimePlatform.IPhonePlayer => "iOS",
            RuntimePlatform.OSXPlayer => "macOS",
            RuntimePlatform.OSXEditor => "macOS",
            RuntimePlatform.WSAPlayerX64 => "UWPx64",
            RuntimePlatform.WSAPlayerX86 => "UWPx86",
            RuntimePlatform.WSAPlayerARM => "UWP_arm",
            _ => "" + Application.platform
        };
        
        env = env.Replace("Editor", "").Replace("Player", "");
        if (Application.isEditor)
        {
            env += "(Editor)";
        }

        if (!Application.genuine)
        {
            env += " (modded?)";
        }
        
        _text.text = GameConstants.Version + "-" + env;
    }
}