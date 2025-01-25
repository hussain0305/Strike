using UnityEngine;
using UnityEngine.SceneManagement;

public class PressAnyKeyToStart : MonoBehaviour
{
    private bool isKeyPressed = false;

    void Update()
    {
        if (!isKeyPressed && (Input.anyKey || Input.GetMouseButtonDown(0)))
        {
            isKeyPressed = true;
            gameObject.SetActive(false);
        }
    }
}