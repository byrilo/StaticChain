using TMPro;
using UnityEngine;

public class NicknameController : MonoBehaviour
{
    private const string PrefsKey = "PlayerNickname";

    [SerializeField] private TMP_InputField nicknameInput;

    private void Start()
    {
        nicknameInput.text = PlayerPrefs.GetString(PrefsKey, "");
        nicknameInput.onEndEdit.AddListener(OnNicknameChanged);
    }

    private void OnNicknameChanged(string value)
    {
        PlayerPrefs.SetString(PrefsKey, value);
        PlayerPrefs.Save();
    }

    public static string GetSavedNickname() => PlayerPrefs.GetString(PrefsKey, "");
}
