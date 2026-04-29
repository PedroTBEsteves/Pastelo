using TMPro;
using UnityEngine;

public class DialoguePresentationView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;

    public TextMeshProUGUI Text => _text;
}
