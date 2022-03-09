using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MCMCSceneGenerator), true)]
public class MCMCSceneGeneratorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        var scene = target as MCMCSceneGenerator;

        if (scene.BackgroundProgress != null && scene.BackgroundProgress != "")
        {
            GUILayout.Label("Scene Generation", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Progress", scene.BackgroundProgress);
            if (GUILayout.Button("Cancel"))
            {
                scene.StopGeneratingInBackground();
            }
        }
        else
        {
            GUILayout.Label("Parameters", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(scene.metropolisHastingsSteps)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(scene.chainCount)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(scene.firstChainTemperature)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(scene.temperatureFactorBetweenChains)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(scene.stepFactorBetweenChains)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(scene.chainSwapPropability)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(scene.pathParentSwapPropability)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(scene.positionStep)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(scene.angleStep)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(scene.drawPlots)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(scene.hierachicalMetropolisHastings)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(scene.pathGenerationEnabled)));
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            GUILayout.Label("Scene Generation", EditorStyles.boldLabel);
            if (scene.GetComponent<GraphScene>()?.GraphInstance == null)
            {
                GUILayout.Label("No graph loaded.");
            }
            else
            {
                if (GUILayout.Button("Reset Objects"))
                {
                    scene.PlaceObjects();
                    scene.GetComponent<GraphDescriptor>()?.RegenerateTexualDescription();
                }
                if (GUILayout.Button("Random Step"))
                {
                    scene.RandomStep();
                }
                if (GUILayout.Button("Generate"))
                {
                    scene.PlaceObjects();
                    scene.GetComponent<GraphDescriptor>()?.RegenerateTexualDescription();
                    scene.StartGeneratingInBackground(Repaint);
                }
            }
        }
    }
}

