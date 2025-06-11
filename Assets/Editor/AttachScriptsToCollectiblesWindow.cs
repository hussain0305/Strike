using UnityEditor;
using UnityEngine;

public class AttachScriptsToCollectiblesWindow : EditorWindow
{
    private GameObject rootObject;
    private bool attachContinuousMovement = false;
    private bool attachContinuousRotation = false;
    private Vector3 point1 = Vector3.zero;
    private Vector3 point2 = Vector3.zero;
    private Vector3 rotationAxis = Vector3.up;
    private float rotationSpeed = 1.0f;

    [MenuItem("Tools/Attach Scripts to Collectibles")] 
    public static void ShowWindow()
    {
        GetWindow<AttachScriptsToCollectiblesWindow>("Attach to Collectibles");
    }

    private void OnGUI()
    {
        GUILayout.Label("Options", EditorStyles.boldLabel);
        rootObject = (GameObject)EditorGUILayout.ObjectField("Root GameObject", rootObject, typeof(GameObject), true);

        EditorGUILayout.Space();

        attachContinuousMovement = EditorGUILayout.Toggle("Attach ContinuousMovement", attachContinuousMovement);
        if (attachContinuousMovement)
        {
            point1 = EditorGUILayout.Vector3Field("Point 1 Local Position", point1);
            point2 = EditorGUILayout.Vector3Field("Point 2 Local Position", point2);
        }

        attachContinuousRotation = EditorGUILayout.Toggle("Attach ContinuousRotation", attachContinuousRotation);

        if (attachContinuousRotation)
        {
            rotationAxis = EditorGUILayout.Vector3Field("Rotation Axis", rotationAxis);
            rotationSpeed = EditorGUILayout.FloatField("Rotation Speed", rotationSpeed);
        }
        
        EditorGUILayout.Space();

        if (GUILayout.Button("Apply to Collectibles"))
        {
            if (rootObject == null)
            {
                Debug.LogError("Assign Root Object");
            }
            else
            {
                ApplyToCollectibles(rootObject.transform);
                Debug.Log("Scripts attached to all Collectibles");
            }
        }
    }

    private void ApplyToCollectibles(Transform parent)
    {
        foreach (Transform child in parent)
        {
            var collectible = child.GetComponent<Collectible>();
            if (collectible != null)
            {
                AttachScripts(child.gameObject);
            }

            ApplyToCollectibles(child);
        }
    }

    private void AttachScripts(GameObject collectibleObj)
    {
        Undo.RegisterFullObjectHierarchyUndo(collectibleObj, "Attach Scripts to Collectible");

        if (attachContinuousMovement)
        {
            GameObject p1 = new GameObject("Point1");
            GameObject p2 = new GameObject("Point2");

            p1.transform.SetParent(collectibleObj.transform);
            p2.transform.SetParent(collectibleObj.transform);

            p1.transform.localPosition = point1;
            p2.transform.localPosition = point2;

            var cm = Undo.AddComponent<ContinuousMovement>(collectibleObj);

            cm.pointTransforms = new Transform[2];
            cm.pointTransforms[0] = p1.transform;
            cm.pointTransforms[1] = p2.transform;
        }

        if (attachContinuousRotation)
        {
            var cr = Undo.AddComponent<ContinuousRotation>(collectibleObj);
            cr.rotationAxis = rotationAxis;
            cr.rotationSpeed = rotationSpeed;
        }
    }
}
