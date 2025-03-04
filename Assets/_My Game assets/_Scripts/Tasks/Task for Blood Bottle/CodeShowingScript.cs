using UnityEngine;
using TMPro; // Required for TextMeshPro

public class CodeShowingScript : MonoBehaviour
{
    [SerializeField] private string textToSet = "----"; // Text to display
    [SerializeField] private TextMeshProUGUI textMeshPro; // Reference to the TextMeshPro component
    [SerializeField] ChestUnlock_BloodBottleTask ChestUnlock_BloodBottleTask;

    public void SetText()
    {
        textToSet = $"{ChestUnlock_BloodBottleTask.savedCode[0]} {ChestUnlock_BloodBottleTask.savedCode[1]} {ChestUnlock_BloodBottleTask.savedCode[2]} {ChestUnlock_BloodBottleTask.savedCode[3]}";
        // Check if the TextMeshPro component exists
        if (textMeshPro != null)
        {
            textMeshPro.text = textToSet;
        }
    }
}