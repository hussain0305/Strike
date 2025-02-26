using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrajectoryHistoryViewer : MonoBehaviour
{
    [Tooltip("Number of materials here will dictate how many trajectories are shown")]
    public Material[] trajectoryHistoryMaterials;
    public Material[] trajectoryLabelMaterials;
    public TrajectoryLabel labelPrefab;
    public Transform labelsParent;
    private readonly Queue<TrajectoryLabel> labelPool = new Queue<TrajectoryLabel>();
    private readonly List<TrajectoryLabel> activeLabels = new List<TrajectoryLabel>();
    
    private static TrajectoryHistoryViewer instance;
    public static TrajectoryHistoryViewer Instance => instance;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    
    private void OnEnable()
    {
        EventBus.Subscribe<BallShotEvent>(BallShot);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<BallShotEvent>(BallShot);
    }

    public void BallShot(BallShotEvent e)
    {
        HideTrajectoryHistory();
    }

    
    public bool GetIsTrajectoryHistoryAvailable()
    {
        return RoundDataManager.Instance.GetTrajectoryHistory().Count > 0;
    }
    
    public void ShowTrajectoryHistory()
    {
        List<ShotInfo> trajectoryHistory = RoundDataManager.Instance.GetTrajectoryHistory();
        int numTrajectoriesNeeded = Mathf.Min(trajectoryHistoryMaterials.Length, trajectoryHistory.Count);
        int numTrajectoriesAvailable = transform.childCount;

        for (int i = 0; i < numTrajectoriesAvailable; i++)
        {
            Transform child = transform.GetChild(i);
            LineRenderer lineRenderer = child.GetComponent<LineRenderer>();
        
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 0;
            }
        }

        for (int i = 0; i < numTrajectoriesAvailable; i++)
        {
            Transform child = transform.GetChild(i);
            child.gameObject.SetActive(false);
        }

        for (int i = numTrajectoriesAvailable; i < numTrajectoriesNeeded; i++)
        {
            GameObject newChild = new GameObject($"Trajectory_{i}");
            newChild.transform.parent = transform;

            LineRenderer lineRenderer = newChild.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.sharedMaterial = trajectoryHistoryMaterials[i];
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
        }

        for (int i = 0; i < numTrajectoriesNeeded; i++)
        {
            Transform child = transform.GetChild(i);
            child.gameObject.SetActive(true);

            LineRenderer lineRenderer = child.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                List<Vector3> trajectory = trajectoryHistory[i].trajectory;
                lineRenderer.positionCount = trajectory.Count;
                lineRenderer.SetPositions(trajectory.ToArray());
            }
        }

        LabelTrajectories();
    }

    public void HideTrajectoryHistory()
    {
        foreach (Transform trajectory in transform)
        {
            trajectory.gameObject.SetActive(false);
        }
        foreach (TrajectoryLabel label in activeLabels)
        {
            ReturnLabelToPool(label);
        }
        activeLabels.Clear();
    }
    
    public void LabelTrajectories()
    {
        foreach (TrajectoryLabel label in activeLabels)
        {
            ReturnLabelToPool(label);
        }
        activeLabels.Clear();

        int trajectoryCount = transform.childCount;
        List<ShotInfo> shotHistory = RoundDataManager.Instance.GetTrajectoryHistory();

        for (int i = 0; i < trajectoryCount; i++)
        {
            Transform trajectory = transform.GetChild(i);
            if (!trajectory.gameObject.activeSelf)
            {
                continue;
            }
            LineRenderer lineRenderer = trajectory.GetComponent<LineRenderer>();
            
            TrajectoryLabel label = GetLabelFromPool();
            label.SetInfo(shotHistory[i].angle, shotHistory[i].spin, shotHistory[i].power, shotHistory[i].points, trajectoryLabelMaterials[i]);
            
            activeLabels.Add(label);
        }
    }
    
    private TrajectoryLabel GetLabelFromPool()
    {
        if (labelPool.Count > 0)
        {
            TrajectoryLabel label = labelPool.Dequeue();
            label.gameObject.SetActive(true);
            return label;
        }
        else
        {
            return Instantiate(labelPrefab, labelsParent);
        }
    }

    private void ReturnLabelToPool(TrajectoryLabel label)
    {
        label.gameObject.SetActive(false);
        labelPool.Enqueue(label);
    }
}
