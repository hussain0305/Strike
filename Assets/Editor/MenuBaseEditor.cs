using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MenuBase))]
public class MenuBaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty menuTypeProp = serializedObject.FindProperty("menuType"); 
        SerializedProperty disableAfterRegisterProp = serializedObject.FindProperty("disableAfterRegister");
        SerializedProperty waitForSaveFileLoadedProp = serializedObject.FindProperty("waitForSaveFileLoaded");

        EditorGUILayout.LabelField("Menu Settings", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(menuTypeProp, new GUIContent("Menu Type"));
        
        EditorGUI.BeginChangeCheck();
        bool disableAfterRegister = EditorGUILayout.Toggle(new GUIContent("Disable After Register", "Disable menu immediately after registering"), disableAfterRegisterProp.boolValue);
        if (EditorGUI.EndChangeCheck())
        {
            disableAfterRegisterProp.boolValue = disableAfterRegister;
            if (disableAfterRegister) waitForSaveFileLoadedProp.boolValue = false; // Ensure only one is enabled
        }

        EditorGUI.BeginChangeCheck();
        bool waitForSaveFileLoaded = EditorGUILayout.Toggle(new GUIContent("Wait for Save File Loaded", "Keep menu enabled until save file loads"), waitForSaveFileLoadedProp.boolValue);
        if (EditorGUI.EndChangeCheck())
        {
            waitForSaveFileLoadedProp.boolValue = waitForSaveFileLoaded;
            if (waitForSaveFileLoaded) disableAfterRegisterProp.boolValue = false; // Ensure only one is enabled
        }

        serializedObject.ApplyModifiedProperties();
    }
}