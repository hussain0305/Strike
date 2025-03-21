using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerInput : MonoBehaviour
{
    public Slider powerSlider;
    public TextMeshProUGUI[] powerText;

    public float Power { get; private set; }
    private BallParameterController BallParameterController => GameManager.Instance.ballParameterController;

    private float powerMultiplier = 100f;
    private Vector2 startTouch;
    private bool isDragging = false;

    private void OnEnable()
    {
        EventBus.Subscribe<NextShotCuedEvent>(Reset);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<NextShotCuedEvent>(Reset);
    }

    private void Update()
    {
        if (!BallParameterController.IsInputtingPower())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            StartSwipe(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            UpdateSwipe(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndSwipe();
        }
    }

    void StartSwipe(Vector2 position)
    {
        startTouch = position;
        isDragging = true;
    }

    void UpdateSwipe(Vector2 position)
    {
        if (!isDragging) return;

        float verticalDelta = (position.y - startTouch.y) / Screen.height;
        Power += verticalDelta * powerMultiplier;
        Power = Mathf.Clamp(Power, 0f, powerSlider.maxValue);

        powerSlider.value = Power;

        foreach (TextMeshProUGUI valText in powerText)
        {
            valText.text = Power.ToString("F0");
        }

        startTouch = position;
    }

    void EndSwipe()
    {
        isDragging = false;
    }

    public void Reset(NextShotCuedEvent e)
    {
        powerSlider.value = 0;
        Power = 0;
    }
}