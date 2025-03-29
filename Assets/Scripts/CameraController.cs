using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraSwitchProcessedEvent
{
    public CameraHoistLocation NewCameraPos { get; }

    public CameraSwitchProcessedEvent(CameraHoistLocation newCameraPos)
    {
        NewCameraPos = newCameraPos;
    }
}

public class CameraController : MonoBehaviour
{
    public Button cameraToggleButton;
    public ScaleUpBulk markersScaleUp;
    public ScaleDownBulk markersScaleDown;
    public GameObject rollOutMenu;
    
    public CameraHoistLocation defaultCameraHoistAt;
    public CameraHoistLocation shotCameraHoistAt;
    public float timeToMoveCamera = 0.5f;
    public CameraFollow cameraFollow;

    [Header("Camera Follow Options")]
    public Button followCamButton;
    public Button stayInPlaceCamButton;
    
    private bool markersCurrentlyVisible = false;
    private bool followBallOnShoot = false;
    private Transform currentTransform;
    private Transform targetTransform;
    private Coroutine cameraMovementCoroutine;
    private CameraHoistLocation currentCameraHoistedAt;
    private CameraHoistLocation targetCameraHoistAt;
    
    private const float ROLLOUT_MENU_AUTOHIDE_DURATION = 5f;
    private float autohideTimeRemaining;
    private Coroutine autohideCoroutine;
    
    public bool CameraIsFollowingBall => cameraFollow.enabled && cameraFollow.followBall;

    private static CameraController instance;
    public static CameraController Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        currentCameraHoistedAt = defaultCameraHoistAt;
    }

    private void OnEnable()
    {
        // cameraToggleButton.onClick.AddListener(MoveToNextCameraPosition);
        cameraToggleButton.onClick.AddListener(RolloutMenuButtonPressed);
        followCamButton.onClick.AddListener(SetToFollowCam);
        stayInPlaceCamButton.onClick.AddListener(SetToStayCam);
        
        EventBus.Subscribe<BallShotEvent>(BallShot);
        EventBus.Subscribe<NextShotCuedEvent>(NextShotCued);
        EventBus.Subscribe<CameraSwitchedEvent>(CameraSwitchedButtonPressed);
    }

    private void OnDisable()
    {
        cameraToggleButton.onClick.RemoveAllListeners();
        followCamButton.onClick.RemoveAllListeners();
        stayInPlaceCamButton.onClick.RemoveAllListeners();
        
        EventBus.Unsubscribe<BallShotEvent>(BallShot);
        EventBus.Unsubscribe<NextShotCuedEvent>(NextShotCued);
        EventBus.Unsubscribe<CameraSwitchedEvent>(CameraSwitchedButtonPressed);
    }

    public void RolloutMenuButtonPressed()
    {
        if (!GameManager.BallShootable) return;
        ToggleRollOutMenuVisibility(!rollOutMenu.activeSelf);
    }

    public void ToggleRollOutMenuVisibility(bool newState)
    {
        rollOutMenu.SetActive(newState);
        if (newState)
        {
            if (autohideCoroutine != null)
            {
                StopCoroutine(autohideCoroutine);
            }
            autohideCoroutine = StartCoroutine(RolloutMenuAutohide());
        }
        else
        {
            if (autohideCoroutine != null)
            {
                StopCoroutine(autohideCoroutine);
            }
        }
    }
    
    public void MoveToCameraPosition(CameraHoistLocation newCameraHoistAt)
    {
        if (CameraIsFollowingBall)
        {
            return;
        }
        if (cameraMovementCoroutine != null)
        {
            StopCoroutine(cameraMovementCoroutine);
        }

        markersCurrentlyVisible = currentCameraHoistedAt.showDistanceMarkers;
        targetCameraHoistAt = newCameraHoistAt;

        if (markersCurrentlyVisible != targetCameraHoistAt.showDistanceMarkers)
        {
            if (targetCameraHoistAt.showDistanceMarkers)
            {
                markersScaleUp.StartScalingUp();
            }
            else
            {
                markersScaleDown.StartScalingDown();
            }
        }
        cameraMovementCoroutine = StartCoroutine(CameraMoveRoutine());
    }
    
    IEnumerator CameraMoveRoutine()
    {
        currentTransform = currentCameraHoistedAt.transform;
        targetTransform = targetCameraHoistAt.transform;
        
        Camera mainCam = Camera.main;
        mainCam.transform.SetParent(targetCameraHoistAt.parentCameraUnder);
        float timePassed = 0;
        while (timePassed <= timeToMoveCamera)
        {
            float lerpVal = timePassed / timeToMoveCamera;
            mainCam.transform.position = Vector3.Lerp(currentTransform.position, targetTransform.position, lerpVal);
            mainCam.transform.rotation = Quaternion.Lerp(currentTransform.rotation, targetTransform.rotation, lerpVal);
            
            timePassed += Time.deltaTime;
            yield return null;
        }

        mainCam.transform.position = targetTransform.position;
        mainCam.transform.rotation = targetTransform.rotation;
        currentCameraHoistedAt = targetCameraHoistAt;
        
        cameraMovementCoroutine = null;
    }

    public void BallShot(BallShotEvent e)
    {
        if (followBallOnShoot)
        {
            cameraFollow.enabled = true;
            cameraFollow.followBall = true;
        }
        ToggleRollOutMenuVisibility(false);
        // MoveToCameraPosition(shotCameraHoistAt);
    }

    public void NextShotCued(NextShotCuedEvent e)
    {
        cameraFollow.followBall = false;
        cameraFollow.enabled = false;
        // MoveToCameraPosition(defaultCameraHoistAt);
        MoveToCameraPosition(currentCameraHoistedAt);
        ToggleRollOutMenuVisibility(false);
    }

    public void CameraSwitchedButtonPressed(CameraSwitchedEvent e)
    {
        if (!GameManager.BallShootable || cameraMovementCoroutine != null)
        {
            return;
        }
        EventBus.Publish(new CameraSwitchProcessedEvent(e.NewCameraPos));
        MoveToCameraPosition(e.NewCameraPos);
        autohideTimeRemaining = ROLLOUT_MENU_AUTOHIDE_DURATION;
    }

    public IEnumerator RolloutMenuAutohide()
    {
        autohideTimeRemaining = ROLLOUT_MENU_AUTOHIDE_DURATION;
        while (autohideTimeRemaining >= 0)
        {
            autohideTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        autohideCoroutine = null;
        ToggleRollOutMenuVisibility(false);
    }

    public void SetToFollowCam()
    {
        followBallOnShoot = true;
        followCamButton.GetComponent<ButtonFeedback>().SetToHighlighted();
        stayInPlaceCamButton.GetComponent<ButtonFeedback>().SetToDefault();
        autohideTimeRemaining = ROLLOUT_MENU_AUTOHIDE_DURATION;
    }

    public void SetToStayCam()
    {
        followBallOnShoot = false;
        followCamButton.GetComponent<ButtonFeedback>().SetToDefault();
        stayInPlaceCamButton.GetComponent<ButtonFeedback>().SetToHighlighted();
        autohideTimeRemaining = ROLLOUT_MENU_AUTOHIDE_DURATION;
    }

    public void ResetCamera()
    {
        currentCameraHoistedAt = defaultCameraHoistAt;
        Camera.main.transform.parent = null;
        Camera.main.transform.position = currentCameraHoistedAt.transform.position;

    }
}
