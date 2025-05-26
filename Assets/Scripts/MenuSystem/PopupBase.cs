using UnityEngine;
using Zenject;

public class PopupBase : MonoBehaviour
{
    private MenuManager menuManager;
    
    [Inject]
    public void Construct(MenuManager _menuManager)
    {
        menuManager = _menuManager;
    }
    
    private void OnEnable()
    {
        menuManager.OpenPopup(gameObject);
    }
}