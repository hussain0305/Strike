using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerInput : MonoBehaviour
{
    public TextMeshProUGUI[] powerText;

    public float Power { get; private set; }
    
    private ShotInput shotInput;
    private ShotInput ShotInput
    {
        get
        {
            if (!shotInput)
            {
                shotInput = GetComponent<ShotInput>();
            }
            return shotInput;
        }
    }

    private float powerMultiplier = 100f;
    private Vector2 startTouch;
    private bool isDragging = false;

    private float keyboardInputChange;
    private float nextKeyboardInputAllowedTime = 0;
    private float keyboardInputInterval = 0.05f;
    
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
        if (!ShotInput.IsInputtingPower())
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
        
        else if (!isDragging && Time.time > nextKeyboardInputAllowedTime)
        {
            keyboardInputChange = 0;
            nextKeyboardInputAllowedTime = Time.time + keyboardInputInterval;
            
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ||
                Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                keyboardInputChange = 1;
            }
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ||
                Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                keyboardInputChange = -1;
            }
            
            Power += keyboardInputChange;
            Power = Mathf.Clamp(Power, 0f, 100);
            foreach (TextMeshProUGUI valText in powerText)
            {
                valText.text = Power.ToString("F0");
            }
        }
    }

    void StartSwipe(Vector2 position)
    {
        startTouch = position;
        isDragging = true;
    }

    void UpdateSwipe(Vector2 position)
    {
        if (!isDragging)
            return;

        float verticalDelta = (position.y - startTouch.y) / Screen.height;
        Power += verticalDelta * powerMultiplier;
        Power = Mathf.Clamp(Power, 0f, 100);
        
        foreach (TextMeshProUGUI valText in powerText)
        {
            valText.text = Power.ToString("F0");
        }

        startTouch = position;
    }

    void EndSwipe()
    {
        EventBus.Publish(new StoppedShotInput());
        isDragging = false;
    }

    public void Reset(NextShotCuedEvent e)
    {
        Power = 0;
    }
    
    public void OverridePower(int _power)
    {
        Power = _power;
    }
}