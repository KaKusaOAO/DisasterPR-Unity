using UnityEngine;

public class AvatarManager : MonoBehaviour
{
    private static AvatarManager _instance;
    public static AvatarManager Instance => _instance;
    
    public Sprite[] sprites;

    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}