using Unity.Netcode;
using UnityEngine;
using TMPro;

public class SubmissionCounterText : MonoBehaviour
{
    [SerializeField] private TMP_Text counterText;

    private void Awake()
    {
        if (counterText != null) counterText.text = "";
    }

    private void Update()
    {
        if (counterText == null) return;

        if (GameManager.Instance == null || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient)
        {
            counterText.text = "";
            return;
        }

        int round = GameManager.Instance.CurrentRound;
        bool roundActive = round >= 0 && round < GameManager.Instance.TotalRounds;
        bool isDrawingRound = roundActive && GameManager.GetContentType(round) == ContentType.Drawing;

        counterText.text = isDrawingRound
            ? $"{GameManager.Instance.SubmittedCount}/{GameManager.Instance.PlayerCount} submitted"
            : "";
    }
}
