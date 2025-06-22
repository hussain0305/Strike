using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HUDAction_CheckedCameraOptions { }

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

    [Header("Camera Pan Options")]
    public Button panCameraButtonNear;
    public Button panCameraButtonFar;
    public Transform panPointsNear;
    public Transform panPointsFar;
    public Transform panFocalPoint;
    
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

    private Coroutine cameraPanCoroutine;
    private bool panCamera = false;
    
    public bool CameraIsFollowingBall => cameraFollow.enabled && cameraFollow.followBall;
    
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
        panCameraButtonNear.onClick.AddListener(() => { StartCameraPan(panPointsNear); });
        panCameraButtonFar.onClick.AddListener(() => { StartCameraPan(panPointsFar); });
        
        EventBus.Subscribe<BallShotEvent>(BallShot);
        EventBus.Subscribe<NextShotCuedEvent>(NextShotCued);
        EventBus.Subscribe<CameraSwitchedEvent>(CameraSwitchedButtonPressed);
    }

    private void OnDisable()
    {
        cameraToggleButton.onClick.RemoveAllListeners();
        followCamButton.onClick.RemoveAllListeners();
        stayInPlaceCamButton.onClick.RemoveAllListeners();
        panCameraButtonNear.onClick.RemoveAllListeners();
        panCameraButtonFar.onClick.RemoveAllListeners();

        EventBus.Unsubscribe<BallShotEvent>(BallShot);
        EventBus.Unsubscribe<NextShotCuedEvent>(NextShotCued);
        EventBus.Unsubscribe<CameraSwitchedEvent>(CameraSwitchedButtonPressed);
    }

    public void RolloutMenuButtonPressed()
    {
        if (!GameManager.BallShootable)
            return;
        EventBus.Publish(new HUDAction_CheckedCameraOptions());
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

        panCamera = false;
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
        Camera mainCam = Camera.main;
        // currentTransform = currentCameraHoistedAt.transform;
        targetTransform = targetCameraHoistAt.transform;
        Vector3 startPosition = mainCam.transform.position;
        Quaternion startRotation = mainCam.transform.rotation;
        
        mainCam.transform.SetParent(targetCameraHoistAt.parentCameraUnder);
        float timePassed = 0;
        while (timePassed <= timeToMoveCamera)
        {
            float lerpVal = timePassed / timeToMoveCamera;
            mainCam.transform.position = Vector3.Lerp(startPosition, targetTransform.position, lerpVal);
            mainCam.transform.rotation = Quaternion.Lerp(startRotation, targetTransform.rotation, lerpVal);
            
            timePassed += Time.deltaTime;
            yield return null;
        }

        mainCam.transform.position = targetTransform.position;
        mainCam.transform.rotation = targetTransform.rotation;
        currentCameraHoistedAt = targetCameraHoistAt;
        
        EventBus.Publish(new CameraSwitchCompletedEvent(currentCameraHoistedAt));
        yield return null;
        cameraMovementCoroutine = null;
    }

    public void BallShot(BallShotEvent e)
    {
        if (followBallOnShoot)
        {
            panCamera = false;
            cameraFollow.enabled = true;
            cameraFollow.followBall = true;
        }
        ToggleRollOutMenuVisibility(false);
        // MoveToCameraPosition(shotCameraHoistAt);
    }

    public void NextShotCued(NextShotCuedEvent e)
    {
        panCamera = false;
        cameraFollow.followBall = false;
        cameraFollow.enabled = false;
        // MoveToCameraPosition(defaultCameraHoistAt);
        MoveToCameraPosition(currentCameraHoistedAt);
        ToggleRollOutMenuVisibility(false);
    }

    public void CameraSwitchedButtonPressed(CameraSwitchedEvent e)
    {
        if (!GameManager.BallShootable || (cameraMovementCoroutine != null && !panCamera))
        {
            return;
        }
        panCamera = false;
        EventBus.Publish(new CameraSwitchProcessedEvent(e.NewCameraPos, timeToMoveCamera));
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
        panCamera = false;
        currentCameraHoistedAt = defaultCameraHoistAt;
        Transform mainCam = Camera.main.transform;
        mainCam.parent = null;
        mainCam.position = currentCameraHoistedAt.transform.position;
        EventBus.Publish(new CameraSwitchCompletedEvent(currentCameraHoistedAt));
    }

    public void StartCameraPan(Transform positions)
    {
        panCamera = true;
        if (cameraMovementCoroutine != null)
        {
            StopCoroutine(cameraMovementCoroutine);
        }
        cameraMovementCoroutine = StartCoroutine(CameraPan(positions));
    }
    
    private IEnumerator CameraPan(Transform positions)
    {
        Transform mainCam = Camera.main.transform;
        float timePerPathSegment = 1.5f;
        int numPositions = positions.childCount;
        
        int nextPoint = -1;
        while (panCamera)
        {
            nextPoint = (nextPoint + 1) % numPositions;
            float timePassed = 0;
            Vector3 startPosition = mainCam.position;
            Vector3 endPosition = positions.GetChild(nextPoint).position;
            while (timePassed <= timePerPathSegment && panCamera)
            {
                mainCam.position = Vector3.Lerp(startPosition, endPosition, timePassed / timePerPathSegment);
                mainCam.LookAt(panFocalPoint);
                timePassed += Time.deltaTime;
                yield return null;
            }
        }
        cameraMovementCoroutine = null;
    }
}
