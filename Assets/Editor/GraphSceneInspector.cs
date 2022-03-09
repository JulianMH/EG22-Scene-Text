using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System;

[CustomEditor(typeof(GraphScene))]
public class GraphSceneInspector : Editor
{
    private Texture2D graphTexture = null;
    private SceneRelationshipGraph graphTextureGraph = null;
    private string graphTextureError = null;

    private void RegenerateGraphTexture()
    {
        var scene = target as GraphScene;

        if(scene.GraphInstance != null)
        {
            try
            {
                var fileName = DotFormatViewer.SavePNG(scene.GraphInstance);
                graphTexture = new Texture2D(2, 2);
                graphTexture.LoadImage(File.ReadAllBytes(fileName));
                graphTextureError = null;
            }
            catch(Exception e)
            {
                graphTextureError = e.ToString();
            }
        }
        else
        {
            graphTexture = null;
            graphTextureError = null;
        }
        graphTextureGraph = scene.GraphInstance;
    }

    public override void OnInspectorGUI()
    {
        var scene = target as GraphScene;

        if (graphTextureGraph != scene.GraphInstance)
            RegenerateGraphTexture();

        GUILayout.Label("Graph", EditorStyles.boldLabel);

        if (scene.GraphInstance == null)
        {
            GUILayout.Label("No graph loaded.");

            if (GUILayout.Button("Load Graph"))
            {
                var path = EditorUtility.OpenFilePanel("Load Graph from JSON", "", "json");
                if (path.Length > 0)
                {
                    scene.GraphInstance = new SceneRelationshipGraphSaveFile(File.ReadAllText(path)).ToGraph();
                }
            }
        }
        else
        {
            if (graphTexture != null)
            {
                GUILayout.Box(graphTexture, GUILayout.Height(400), GUILayout.ExpandWidth(true));
            }
            if (graphTextureError != null)
            {
                GUILayout.Label(graphTextureError);
            }

            if (GUILayout.Button("View Graph in External Program"))
            {
                DotFormatViewer.Display(scene.GraphInstance);
            }


            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Graph"))
            {
                var path = EditorUtility.OpenFilePanel("Load Graph from JSON", Application.dataPath + "/SceneLayouts", "json");
                if (path.Length > 0)
                {
                    scene.GraphInstance = new SceneRelationshipGraphSaveFile(File.ReadAllText(path)).ToGraph();
                }
            }

            if (GUILayout.Button("Save Graph"))
            {
                var path = EditorUtility.SaveFilePanel("Save Graph as JSON", Application.dataPath + "/SceneLayouts", "graph.json", "json");
                if (path.Length > 0)
                {
                    File.WriteAllText(path, new SceneRelationshipGraphSaveFile(scene.GraphInstance).ToJson());
                    DotFormatViewer.SaveSVG(scene.GraphInstance, path + ".svg");
                }
            }

            GUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        GUILayout.Label("Scene", EditorStyles.boldLabel);


        GUILayout.BeginHorizontal();
        var sceneGameObject = scene.transform.Find("Scene")?.gameObject;
        if (GUILayout.Button("Load Scene"))
        {
            var path = EditorUtility.OpenFilePanel("Load SceneLayout from JSON", Application.dataPath + "/SceneLayouts", "json");
            if (path.Length > 0)
            {
                scene.PlaceObjects(new SceneLayout(File.ReadAllText(path)));
            }
        }


        if (sceneGameObject != null)
        { 
            if (GUILayout.Button("Save Scene"))
            {
                var path = EditorUtility.SaveFilePanel("Save SceneLayout as JSON", Application.dataPath + "/SceneLayouts", "scene.json", "json");
                if (path.Length > 0)
                {
                    var sceneLayout = SceneLayout.GetCurrentLayout(
                        scene.transform.GetComponentsInChildren<GraphNodeBehaviour>().ToArray(),
                        scene.GetComponentsInChildren<Path>());

                    File.WriteAllText(path, sceneLayout.ToJson());
                }
            }
        }

        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUILayout.Label("Scene Evaluation", EditorStyles.boldLabel);

        if (scene.GraphInstance == null)
        {
            GUILayout.Label("No graph loaded.");
        }
        else
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(scene.CollisionPower)));
            serializedObject.ApplyModifiedProperties();

            try
            {
                var evaluation = scene.CalculateEvaluationFunction(scene.GetAllGraphNodeBehavioursRecursively(), scene.GetComponentsInChildren<Path>());
                EditorGUILayout.LabelField("Scene Evaluation Value", (evaluation.HasValue ? System.Math.Exp(-evaluation.Value).ToString() : "Invalid Objects"));
            }
            catch (ArithmeticException e)
            {
                GUILayout.Label("Scene Evaluation Value: " + e.Message);
            }
            catch (InvalidOperationException e)
            {
                GUILayout.Label("Scene Evaluation Value: " + e.Message);
            }
        }
    }
}
