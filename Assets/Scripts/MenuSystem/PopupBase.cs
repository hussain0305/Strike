using UnityEngine;

public class PopupBase : MonoBehaviour
{
    private void OnEnable()
    {
        MenuManager.Instance.OpenPopup(gameObject);
    }
}