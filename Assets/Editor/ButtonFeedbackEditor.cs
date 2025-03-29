using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ButtonFeedback))]
public class ButtonFeedbackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (target == null)
        {
            return;
        }

        serializedObject.Update();

        ButtonFeedback button = (ButtonFeedback)target;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("groupId"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("buttonLocation"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("outline"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("backToDefaultOnEnable"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("staysSelected"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Sounds", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("playsHoverSound"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("playsClickSound"));

        SerializedProperty overrideHoverSoundProp = serializedObject.FindProperty("overrideHoverSound");
        EditorGUILayout.PropertyField(overrideHoverSoundProp);
        if (overrideHoverSoundProp.boolValue)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hoverClipOverride"));
        }

        SerializedProperty overrideClickSoundProp = serializedObject.FindProperty("overrideClickSound");
        EditorGUILayout.PropertyField(overrideClickSoundProp);
        if (overrideClickSoundProp.boolValue)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clickClipOverride"));
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Hover Pop Animation", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("popTarget"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("popScale"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("popDuration"));

        serializedObject.ApplyModifiedProperties();
    }
}