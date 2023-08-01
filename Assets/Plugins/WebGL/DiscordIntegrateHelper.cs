using System.Runtime.InteropServices;
using AOT;

public static class DiscordIntegrateHelper
{
    public delegate void OnAccessTokenUpdatedCallback();

    public static event OnAccessTokenUpdatedCallback AccessTokenUpdated;
    
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    public static extern void DCSetOnAccessTokenUpdated(OnAccessTokenUpdatedCallback callback);

    [DllImport("__Internal")]
    public static extern string DCGetAccessToken();
    
    [DllImport("__Internal")]
    public static extern void DCStartAuthenticate();
#else
    public static void DCSetOnAccessTokenUpdated(OnAccessTokenUpdatedCallback callback)
    {
        // Stub.
    }

    public static string DCGetAccessToken()
    {
        // Stub.
        return "";
    }

    public static void DCStartAuthenticate()
    {
        // Stub.
    }
#endif

    [MonoPInvokeCallback(typeof(OnAccessTokenUpdatedCallback))]
    private static void DelegateOnAccessTokenUpdated()
    {
        AccessTokenUpdated?.Invoke();
    }
    
    static DiscordIntegrateHelper()
    {
        DCSetOnAccessTokenUpdated(DelegateOnAccessTokenUpdated);
    }
}