#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

public static class ApplyFontUtility
{
    [MenuItem("Tools/Static Chain/Apply Fredoka Font To All Text")]
    public static void ApplyFont()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Fredoka-Bold SDF.asset");
        if (font == null)
        {
            Debug.LogError("Не найден шрифт по пути Assets/Fonts/Fredoka-Bold SDF.asset");
            return;
        }

        var texts = Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        int count = 0;
        foreach (var t in texts)
        {
            Undo.RecordObject(t, "Apply Fredoka Font");
            t.font = font;
            EditorUtility.SetDirty(t);
            count++;
        }

        Debug.Log($"Шрифт применён к {count} текстовым объектам.");
    }
}
#endif
