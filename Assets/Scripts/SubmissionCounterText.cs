using UnityEngine;
using TMPro;

public class SubmissionCounterText : MonoBehaviour
{
    [SerializeField] private TMP_Text counterText;

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.GameStarted) return;
        counterText.text = $"{GameManager.Instance.SubmittedCount}/{GameManager.Instance.PlayerCount} submitted";
    }
}
