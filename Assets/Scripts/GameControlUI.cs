using TMPro;
using UnityEngine;

public class GameControlUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField roundsInput;

    public void OnStartGameClicked()
    {
        int.TryParse(roundsInput.text, out var requestedRounds);
        GameManager.Instance.StartGameRpc(requestedRounds);
    }

    public void OnPlayAgainClicked()
    {
        GameManager.Instance.ResetGameRpc();
    }

    public void OnQuitClicked()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
