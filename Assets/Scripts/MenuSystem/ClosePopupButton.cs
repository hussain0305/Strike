using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ClosePopupButton : MonoBehaviour
{
    private MenuManager menuManager;
    
    [Inject]
    public void Construct(MenuManager _menuManager)
    {
        menuManager = _menuManager;
    }

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => menuManager.CloseCurrentPopup());
    }
}