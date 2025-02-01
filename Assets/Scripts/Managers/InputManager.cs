using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager instance;
    public static InputManager Instance => instance;

    private GameContext currentContext = GameContext.InMenu; 
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance.gameObject);
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBackAction();
        }
    }

    public void SetContext(GameContext context)
    {
        currentContext = context;
    }

    private void HandleBackAction()
    {
        switch (currentContext)
        {
            case GameContext.InMenu:
                MenuManager.Instance.CloseCurrentMenu();
                break;

            case GameContext.InGame:
                if (MenuManager.Instance.IsAnyMenuOpen())
                {
                    MenuManager.Instance.CloseCurrentMenu();
                }
                else
                {
                    MenuManager.Instance.OpenMenu(MenuBase.MenuType.PauseMenu);
                }
                break;
        }
    }
}
