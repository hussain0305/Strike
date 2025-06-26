using System;
using TMPro;
using UnityEngine;

public class SpinInput : MonoBehaviour
{
    public RectTransform controlArea;
    public RectTransform pointer;
    private float swipeSpeed = 5f;
    public TextMeshProUGUI[] spinValueText;

    public Vector2 SpinVector { get; private set; }
    
    private Vector3 RestingPosition => new(0, 0, -0.1f);

    private bool isInteracting;
    private Vector2 startTouchPosition;
    private Vector2 controlBounds;
    private Vector2 keyboardInputChange;

    private void Start()
    {
        controlBounds = controlArea.rect.size / 2f;
    }

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
    
    private void Update()
    {
        if (!ShotInput.IsInputtingSpin())
        {
            return;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            isInteracting = true;
            startTouchPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isInteracting = false;
            EventBus.Publish(new StoppedShotInput());
        }

        if (Input.GetMouseButton(0) && isInteracting)
        {
            Vector2 currentTouchPosition = Input.mousePosition;
            Vector2 swipeDelta = currentTouchPosition - startTouchPosition;

            swipeDelta *= swipeSpeed;
            SpinChanged(swipeDelta);
            startTouchPosition = currentTouchPosition;
        }
        
        else if (!isInteracting)
        {
            keyboardInputChange = Vector2.zero;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                keyboardInputChange += 2 * Vector2.up;
            }
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                keyboardInputChange += 2 * Vector2.down;
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                keyboardInputChange += 2 * Vector2.left;
            }
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                keyboardInputChange += 2 * Vector2.right;
            }
            SpinChanged(keyboardInputChange);
        }
    }

    public void SpinChanged(Vector2 swipeDelta)
    {
        SpinVector += swipeDelta / new Vector2(Screen.width, Screen.height);
        SpinVector = Vector2.ClampMagnitude(SpinVector, 1f);
            
        string spinText = $"{SpinVector.x * 100:F0}, {SpinVector.y * 100:F0}";
        foreach (TextMeshProUGUI valText in spinValueText)
        {
            valText.text = spinText;
        }
            
        UpdatePointer();
    }

    public void UpdatePointer()
    {
        Vector2 pointerPos = new Vector2(SpinVector.x * controlBounds.x, SpinVector.y * controlBounds.y);
        pointer.localPosition = pointerPos;
    }
    
    private void OnEnable()
    {
        EventBus.Subscribe<NextShotCuedEvent>(ResetPointer);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<NextShotCuedEvent>(ResetPointer);
    }

    public void ResetPointer(NextShotCuedEvent e)
    {
        pointer.localPosition = RestingPosition;
        SpinVector = Vector2.zero;
    }
}