using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GrammarGraphGenerator))]
public class GrammarGraphGeneratorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        var scene = target as GrammarGraphGenerator;

        GUILayout.Label("Parameters", EditorStyles.boldLabel);
        if (GUILayout.Button("View Grammar in External Program"))
        {
            DotFormatViewer.Display(scene.Grammar);
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(scene.GrammarName)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(scene.DerivationSteps)));
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        GUILayout.Label("Graph Generation", EditorStyles.boldLabel);
        if (GUILayout.Button("Generate"))
        {
            scene.Generate();
            scene.GetComponent<MCMCSceneGenerator>()?.PlaceObjects();
        }

    }
}
