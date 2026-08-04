using TMPro;
using UnityEngine;

public class PhraseSubmitUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField phraseInput;

    public void OnSubmitClicked()
    {
        if (string.IsNullOrWhiteSpace(phraseInput.text)) return;
        PhraseManager.Instance.SubmitPhraseRpc(phraseInput.text);
    }
}