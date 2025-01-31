using UnityEngine;
using UnityEngine.UI;

public class ClosePopupButton : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => MenuManager.Instance.CloseCurrentPopup());
    }
}