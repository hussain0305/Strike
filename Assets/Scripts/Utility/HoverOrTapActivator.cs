using UnityEngine;
using UnityEngine.EventSystems;

public class HoverOrTapActivator : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    public GameObject target;

    void Reset()
    {
        if (target == null)
            target = this.gameObject;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (target != null)
            target.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (target != null)
            target.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (target != null)
            target.SetActive(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (target != null)
            target.SetActive(false);
    }
}