using UnityEngine;
using TMPro;

public class SwordMold : MonoBehaviour
{
    [SerializeField]
    private string moldText;          // Current mold label

    [Header("Optional display")]
    [SerializeField]
    private TMP_Text moldLabel;       // Assign a TextMeshPro (3D or UI) to show the value

    public string MoldText => moldText;

    public void SetMold(string newMold)
    {
        moldText = newMold;
        UpdateLabel();
    }

    void OnEnable()
    {
        UpdateLabel();
    }

    void UpdateLabel()
    {
        if (moldLabel != null)
            moldLabel.text = moldText;
    }
}
