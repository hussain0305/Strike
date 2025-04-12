using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialCategorySelectedEvent
{
    public TutorialCategory Category;
    public TutorialCategorySelectedEvent(TutorialCategory cat)
    {
        Category = cat;
    }
}

public class TutorialCategoryButton : MonoBehaviour
{
    public TextMeshProUGUI selectedText;
    public TextMeshProUGUI unSelectedText;
    
    public Button button;
    
    private TutorialCategory associatedCategory;
    private TutorialCategory AssociatedCategory => associatedCategory ??= GetComponent<TutorialCategory>();

    private void OnEnable()
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            EventBus.Publish(new TutorialCategorySelectedEvent(AssociatedCategory));
        });
        EventBus.Subscribe<TutorialCategorySelectedEvent>(NewCategorySelected);
    }

    private void OnDisable()
    {
        button.onClick.RemoveAllListeners();
        EventBus.Unsubscribe<TutorialCategorySelectedEvent>(NewCategorySelected);
    }

    public void NewCategorySelected(TutorialCategorySelectedEvent e)
    {
        selectedText.gameObject.SetActive(e.Category == AssociatedCategory);
        unSelectedText.gameObject.SetActive(e.Category != AssociatedCategory);
    }

    public void Reset()
    {
        selectedText.gameObject.SetActive(false);
        unSelectedText.gameObject.SetActive(true);
    }
    
    public void SetAppearance(TutorialCategory selectedCategory)
    {
        selectedText.gameObject.SetActive(selectedCategory == AssociatedCategory);
        unSelectedText.gameObject.SetActive(selectedCategory != AssociatedCategory);
    }
}
