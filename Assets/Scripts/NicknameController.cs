using TMPro;
using UnityEngine;

public class NicknameController : MonoBehaviour
{
    private const string PrefsKey = "PlayerNickname";

    [SerializeField] private TMP_InputField nicknameInput;

    private bool _sentForThisConnection;

    private void Start()
    {
        nicknameInput.text = PlayerPrefs.GetString(PrefsKey, "");
        nicknameInput.onEndEdit.AddListener(OnNicknameChanged);
    }

    private void Update()
    {
        bool connected = Unity.Netcode.NetworkManager.Singleton != null
                          && Unity.Netcode.NetworkManager.Singleton.IsConnectedClient;

        if (connected && !_sentForThisConnection)
        {
            _sentForThisConnection = true;
            SendNickname();
        }
        else if (!connected)
        {
            _sentForThisConnection = false;
        }
    }

    private void SendNickname()
    {
        if (PlayerListManager.Instance == null) return;

        var nickname = GetSavedNickname();
        if (string.IsNullOrWhiteSpace(nickname)) nickname = "Player";

        PlayerListManager.Instance.SetNicknameRpc(nickname);
    }

    private void OnNicknameChanged(string value)
    {
        PlayerPrefs.SetString(PrefsKey, value);
        PlayerPrefs.Save();
    }

    public static string GetSavedNickname() => PlayerPrefs.GetString(PrefsKey, "");
}
