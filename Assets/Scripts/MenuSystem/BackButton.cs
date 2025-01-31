using UnityEngine;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => MenuManager.Instance.CloseCurrentMenu());
    }
}