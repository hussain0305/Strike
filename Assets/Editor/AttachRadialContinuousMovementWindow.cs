using UnityEditor;
using UnityEngine;

public class AttachRadialContinuousMovementWindow : EditorWindow
{
    private GameObject rootObject;
    private Vector3 centerPosition = Vector3.zero;
    private float distance = 1f;

    [MenuItem("Tools/Attach Radial Continuous Movement To Collectibles")]
    public static void ShowWindow()
    {
        GetWindow<AttachRadialContinuousMovementWindow>("Attach Radial Continuous Movement");
    }

    private void OnGUI()
    {
        GUILayout.Label("Root Object and Movement Settings", EditorStyles.boldLabel);
        rootObject = (GameObject)EditorGUILayout.ObjectField("Root GameObject", rootObject, typeof(GameObject), true);

        EditorGUILayout.Space();
        centerPosition = EditorGUILayout.Vector3Field("Center Position", centerPosition);
        distance = EditorGUILayout.FloatField("Distance", distance);

        EditorGUILayout.Space();
        if (GUILayout.Button("Apply"))
        {
            if (rootObject == null)
            {
                Debug.LogError("No root object");
            }
            else
            {
                ApplyMovement(rootObject.transform);
                Debug.Log("Applied Radial Continuous Movement to all");
            }
        }
    }

    private void ApplyMovement(Transform parent)
    {
        foreach (Transform child in parent)
        {
            var collectible = child.GetComponent<Collectible>();
            if (collectible != null)
            {
                AttachMovement(child.gameObject);
            }
            ApplyMovement(child);
        }
    }

    private void AttachMovement(GameObject collectibleObj)
    {
        Undo.RegisterFullObjectHierarchyUndo(collectibleObj, "Attach ContinuousMovement to Collectible");

        GameObject p1 = new GameObject("Point1");
        p1.transform.SetParent(collectibleObj.transform);
        p1.transform.localPosition = Vector3.zero;

        Vector3 worldCenter = centerPosition;
        Vector3 worldCollectible = collectibleObj.transform.position;
        Vector3 dir = (worldCollectible - worldCenter).normalized;
        if (dir == Vector3.zero)
            dir = Vector3.forward;

        Vector3 worldPoint2 = worldCollectible + (dir * distance);
        GameObject p2 = new GameObject("Point2");
        Undo.RegisterCreatedObjectUndo(p2, "Create CM Point2");
        p2.transform.SetParent(collectibleObj.transform);
        p2.transform.position = worldPoint2;

        var cm = Undo.AddComponent<ContinuousMovement>(collectibleObj);
        cm.pointTransforms = new Transform[2];
        cm.pointTransforms[0] = p1.transform;
        cm.pointTransforms[1] = p2.transform;
    }
}
