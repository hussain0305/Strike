using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelExporter))]
public class LevelExporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelExporter levelExporter = (LevelExporter)target;
        if (GUILayout.Button("Export Level"))
        {
            levelExporter.ExportLevel();
        }
    }
}