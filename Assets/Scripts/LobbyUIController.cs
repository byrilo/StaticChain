using Unity.Netcode;
using UnityEngine;

public class LobbyUIController : MonoBehaviour
{
    [SerializeField] private GameObject preConnectGroup; // Host/Join/JoinCodeInput/Nickname
    [SerializeField] private GameObject hostControls;    // RoundsInput/StartGameButton
    [SerializeField] private GameObject leaveButton;

    private bool _initialized;
    private bool _lastConnected;
    private bool _lastIsHost;

    private void Update()
    {
        bool connected = NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;
        bool isHost = connected && NetworkManager.Singleton.IsHost;

        if (_initialized && connected == _lastConnected && isHost == _lastIsHost) return;
        _initialized = true;
        _lastConnected = connected;
        _lastIsHost = isHost;

        preConnectGroup.SetActive(!connected);
        hostControls.SetActive(connected && isHost);
        leaveButton.SetActive(connected && !isHost);
    }
}
