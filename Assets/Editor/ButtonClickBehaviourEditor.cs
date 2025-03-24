using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ButtonClickBehaviour))]
public class ButtonClickBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (target == null)
        {
            return;
        }
        
        serializedObject.Update();

        ButtonClickBehaviour button = (ButtonClickBehaviour)target;

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
        
        serializedObject.ApplyModifiedProperties();
    }
}