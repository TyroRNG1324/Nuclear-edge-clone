using UnityEngine;
using TMPro;

public class ClearInput : MonoBehaviour
{
    [SerializeField]private TMP_InputField input;

    private void OnEnable()
    {
        input = GetComponent<TMP_InputField>();
    }

    private void OnDisable()
    {
        input.Select();
        input.text = "";
    }
}
