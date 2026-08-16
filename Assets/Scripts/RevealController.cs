using Unity.Netcode;
using UnityEngine;

// Серверно-авторитетное состояние "какую страницу какой книги сейчас все смотрят"
// на финальном экране — как в Gartic Phone: один синхронный показ на всех.
public class RevealController : NetworkBehaviour
{
    public static RevealController Instance { get; private set; }

    private const float DrawingDisplaySeconds = 5f;
    private const float VoiceExtraSeconds = 0.75f;
    private const float VoiceMinSeconds = 2f;
    private const float VoiceFallbackSeconds = 6f;

    private readonly NetworkVariable<int> _chainIndex = new NetworkVariable<int>(-1);
    private readonly NetworkVariable<int> _entryIndex = new NetworkVariable<int>(0);

    public int ChainIndex => _chainIndex.Value;
    public int EntryIndex => _entryIndex.Value;
    public bool IsRevealing => _chainIndex.Value >= 0;

    private float _timer;
    private bool _started;

    private void Awake() => Instance = this;

    private void Update()
    {
        if (!IsServer) return;
        if (GameManager.Instance == null) return;

        bool finished = GameManager.Instance.GameStarted &&
                        GameManager.Instance.CurrentRound >= GameManager.Instance.TotalRounds &&
                        GameManager.Instance.TotalRounds > 0;

        if (!finished)
        {
            if (_chainIndex.Value != -1) _chainIndex.Value = -1;
            _started = false;
            return;
        }

        if (!_started)
        {
            _started = true;
            _timer = 0f;
            _entryIndex.Value = 0;
            _chainIndex.Value = 0;
        }

        _timer += Time.deltaTime;

        float displayDuration = GetDisplayDuration(_chainIndex.Value, _entryIndex.Value);
        if (_timer < displayDuration) return;
        _timer = 0f;

        int totalEntries = GameManager.Instance.TotalRounds;

        if (_entryIndex.Value + 1 < totalEntries)
        {
            _entryIndex.Value++;
        }
        else if (_chainIndex.Value + 1 < GameManager.Instance.PlayerCount)
        {
            _chainIndex.Value++;
            _entryIndex.Value = 0;
        }
        // иначе остаёмся на последней странице последней книги — показ окончен
    }

    private float GetDisplayDuration(int chainId, int round)
    {
        var contentType = GameManager.GetContentType(round);
        if (contentType == ContentType.Drawing) return DrawingDisplaySeconds;

        if (VoiceManager.Instance != null && VoiceManager.Instance.TryGetVoiceDuration(chainId, round, out var seconds))
            return Mathf.Max(seconds + VoiceExtraSeconds, VoiceMinSeconds);

        return VoiceFallbackSeconds;
    }
}
