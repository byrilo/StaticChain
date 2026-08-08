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
    private bool _actionInProgress;

    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();

            // Даёт каждому запущенному .exe на этом компьютере свою анонимную личность,
            // чтобы можно было тестировать несколько окон локально (в реальной игре с
            // друзьями на разных компьютерах это не требуется, но не мешает).
            AuthenticationService.Instance.SwitchProfile("Player" + UnityEngine.Random.Range(0, 1000000));

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            statusText.text = "Ready to connect";
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            statusText.text = "Sign-in error: " + e.Message;
        }
    }

    public async void OnHostClicked()
    {
        // Защита от повторных нажатий, пока предыдущая попытка ещё выполняется
        // (или уже завершилась успехом) — иначе повторный клик уйдёт как вторая
        // попытка и упадёт с ошибкой вида "already a member".
        if (_actionInProgress || _session != null) return;
        _actionInProgress = true;

        try
        {
            var options = new SessionOptions { MaxPlayers = 8 }.WithRelayNetwork();
            _session = await MultiplayerService.Instance.CreateSessionAsync(options);
            statusText.text = $"Room code: {_session.Code}";
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            statusText.text = "Error creating room: " + e.Message;
            _actionInProgress = false;
        }
    }

    public async void OnJoinClicked()
    {
        if (_actionInProgress || _session != null) return;
        _actionInProgress = true;

        try
        {
            var code = joinCodeInput.text.Trim();
            _session = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);
            statusText.text = "Connected!";
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            statusText.text = "Error joining room: " + e.Message;
            _actionInProgress = false;
        }
    }

    public async void OnLeaveLobbyClicked()
    {
        try
        {
            if (_session != null)
            {
                await _session.LeaveAsync();
                _session = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            _actionInProgress = false;
            statusText.text = "Ready to connect";
        }
    }
}
