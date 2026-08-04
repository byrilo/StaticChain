using System;
using TMPro;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;

public class SessionManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text statusText;

    private ISession _session;

    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            statusText.text = "Готово к подключению";
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            statusText.text = "Ошибка входа: " + e.Message;
        }
    }

    public async void OnHostClicked()
    {
        try
        {
            var options = new SessionOptions { MaxPlayers = 8 }.WithRelayNetwork();
            _session = await MultiplayerService.Instance.CreateSessionAsync(options);
            statusText.text = $"Код комнаты: {_session.Code}\nNGO IsHost: {Unity.Netcode.NetworkManager.Singleton.IsHost}";
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            statusText.text = "Ошибка создания: " + e.Message;
        }
    }


    public async void OnJoinClicked()
    {
        try
        {
            var code = joinCodeInput.text.Trim();
            _session = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);
            statusText.text = $"Подключено: {code}\nNGO IsClient: {Unity.Netcode.NetworkManager.Singleton.IsClient}, ConnectedClients: {Unity.Netcode.NetworkManager.Singleton.ConnectedClientsList.Count}";
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            statusText.text = "Ошибка входа в комнату: " + e.Message;
        }
    }
}