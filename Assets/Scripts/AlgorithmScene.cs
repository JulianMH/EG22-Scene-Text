using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

public class AlgorithmScene : MonoBehaviour
{
    private IEnumerator algorithmEnumerator = null;

    public UnityEngine.UI.Text progressLabel;
    public UnityEngine.UI.Text descriptionLabel;
    public UnityEngine.UI.Image progressPanel;

    private Stopwatch stopwatch = new Stopwatch();

    private int stepCount;
    private int currentStep;

    [SerializeField]
    RenderToImageCamera renderToImageCamera = null;

    [SerializeField]
    bool takeScreenshots;

    void StartGenerating()
    {
        var graphGenerator = GetComponent<GrammarGraphGenerator>();
        graphGenerator.Generate();

        var sceneGenerator = GetComponent<MCMCSceneGenerator>();
        sceneGenerator.PlaceObjects();
        algorithmEnumerator = sceneGenerator.GenerateInBackgroundCoroutine(() =>  {});
        stepCount = sceneGenerator.metropolisHastingsSteps;
        currentStep = 0;
        algorithmEnumerator.MoveNext();

        var descriptionGenerator = GetComponent<GraphDescriptor>();
        descriptionGenerator.RegenerateTexualDescription();
        descriptionLabel.text = descriptionGenerator.TextualDescription;
    }

    private void Start()    {
#if UNITY_WEBGL        try
        {
            WebGLInput.captureAllKeyboardInput = false;
        }
        catch(System.Exception)
        {

        }

#endif
        string[] args = System.Environment.GetCommandLineArgs();
        if (args.Length > 1 && File.Exists(args[1]))
        {
            var sceneJson = File.ReadAllText(args[1]);
            var scene = GetComponent<GraphScene>();
            scene.PlaceObjects(new SceneLayout(sceneJson));

            if(args[1].EndsWith("scene.json") && File.Exists(args[1].Replace("scene.json", "graph.json"))) 
            {
                var graphJson = File.ReadAllText(args[1].Replace("scene.json", "graph.json"));
                scene.GraphInstance = new SceneRelationshipGraphSaveFile(graphJson).ToGraph();

                var descriptionGenerator = GetComponent<GraphDescriptor>();
                descriptionGenerator.RegenerateTexualDescription();
                descriptionLabel.text = descriptionGenerator.TextualDescription;
            }
        }    }

    // Update is called once per frame
    void Update()
    {
        if (algorithmEnumerator != null)
        {
            stopwatch.Start();

            do
            {
                if (!algorithmEnumerator.MoveNext())
                {
                    algorithmEnumerator = null;
                    progressLabel.text = "";
                    return;
                }                   

                ++currentStep;
            }
            while (stopwatch.ElapsedMilliseconds < 60 && !takeScreenshots);

            var sceneGenerator = GetComponent<MCMCSceneGenerator>();
            progressLabel.text = sceneGenerator.BackgroundProgress;
            progressPanel.rectTransform.anchorMax = new Vector2(sceneGenerator.BackgroundProgressValue, 1);

            stopwatch.Reset();
        }
        
    }

    private void OnPostRender()
    {
        if(takeScreenshots)
        {
            Directory.CreateDirectory("Temp/Screenshots");
            File.WriteAllBytes("Temp/Screenshots/" + currentStep + ".jpg", renderToImageCamera.GetScreenshot());

            var graphScene = GetComponent<GrammarGraphGenerator>();
            var sceneLayout = SceneLayout.GetCurrentLayout(
                graphScene.transform.GetComponentsInChildren<GraphNodeBehaviour>().ToArray(),
                graphScene.GetComponentsInChildren<Path>());

            File.WriteAllText("Temp/Screenshots/" + currentStep + ".scene.json", sceneLayout.ToJson());
        }
    }
}
