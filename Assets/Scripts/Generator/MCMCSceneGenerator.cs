using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

#if (UNITY_EDITOR)
using Unity.EditorCoroutines.Editor;
#endif
public class MCMCSceneGenerator : MonoBehaviour
{
    public int metropolisHastingsSteps = 10000;
    public int chainCount = 10;
    public double firstChainTemperature = 1.0;
    public double temperatureFactorBetweenChains = 1.3;
    public double stepFactorBetweenChains = 1.0f;
    public double chainSwapPropability = 1.0f;
    public double pathParentSwapPropability = 0.1f;
    public Vector3 positionStep = Vector3.one * 0.1f;
    public float angleStep = 1.0f;
    public bool hierachicalMetropolisHastings = false;
    public bool pathGenerationEnabled = true;

    public bool drawPlots = false;

    private void RemoveChildren(Transform parentTransform)
    {
        for (int i = parentTransform.childCount - 1; i >= 0; --i)
        {
            DestroyImmediate(parentTransform.GetChild(i).gameObject);
        }
    }

    public string GetAssetForNode(string nodeType)
    {
        return "GraphNodes/" + nodeType.ToString();
    }

    public void RandomStep()
    {
        ConnectPathRelationships();
        ConnectDisconnectedPaths(transform);
        var graphBasedScene = GetComponent<GraphScene>();
        var chain = new MCMCChain(graphBasedScene, transform.GetChild(0), firstChainTemperature, positionStep, angleStep, pathParentSwapPropability);
        chain.ApplySceneLayout(chain.SampleJumpDistribution());
    }

    private void SetRenderToTextureCamerasEnabled(bool enabled)
    {
        var cameras = GetComponentsInChildren<Camera>();
        foreach(var camera in cameras)
        {
            camera.enabled = enabled;
        }
    }

    private void ConnectDisconnectedPaths(Transform transform)
    {
        // Connect all disconnected paths to root path
        var paths = transform.GetComponentsInChildren<Path>();
        var rootPaths = paths.Where(n => n.IsRootPath || n.gameObject.transform == transform).ToArray();
        paths = paths.Except(rootPaths).ToArray();

        if(rootPaths.Any())
        {
            foreach (var path in paths)
            {
                if (path.Parent == null && !path.IsRootPath)
                {

                    path.Parent = rootPaths[Random.Range(0, rootPaths.Length)];
                }
            }
        }
    }

    private void ConnectPathRelationships()
    {
        var graphBasedScene = GetComponent<GraphScene>();
        var behaviours = graphBasedScene.GetAllGraphNodeBehavioursRecursively();

        foreach (var (a, relatesTo, b) in graphBasedScene.GraphInstance.Edges)
        {
            if(pathGenerationEnabled && relatesTo == RelationshipType.PathConnectedTo)
            {
                behaviours[a].GetComponent<Path>().Parent = behaviours[b].GetComponent<Path>();
            }
            else if (relatesTo == RelationshipType.NextToChooseDirection && behaviours[b].NodeType.StartsWith("Path"))
            {
                var rootPath = behaviours[b].GetComponent<Path>();
                var paths = behaviours[a].GetComponentsInChildren<Path>();

                foreach (var path in paths)
                {
                    if (path.Parent == null && !path.IsRootPath)
                    {
                        path.Parent = rootPath;
                    }
                }
            }
        }
    }

    public IEnumerable<MCMCChain.StepStatistics[]> RunMetropolisHastings()
    {
        // First initialize all chains

        var graphBasedScene = GetComponent<GraphScene>();

        SetRenderToTextureCamerasEnabled(chainCount == 1);

        var sceneTransform = transform.GetChild(0);
        List<Transform> hierachicalSchedule = GetHierachicalSchedule();
        var stepsPerScheduleEntry = metropolisHastingsSteps / hierachicalSchedule.Count;

        yield return null;

        foreach (var parentTransform in hierachicalSchedule)
        {
            if (parentTransform == sceneTransform)
            {
                ConnectPathRelationships();
            }
            ConnectDisconnectedPaths(parentTransform);

            var parentTransformParent = parentTransform.parent;
            var parentTransformName = parentTransform.name;

            var chainContainer = new GameObject("Chain Container for " + parentTransformName);
            chainContainer.transform.CopyFrom(parentTransform);

            var chains = new MCMCChain[chainCount];

            for (int i = 0; i < chainCount; ++i)
            {
                var temperature = firstChainTemperature * System.Math.Pow(temperatureFactorBetweenChains, i);
                var stepFactor = System.Math.Pow(stepFactorBetweenChains, i);

                var chainGameObject = Instantiate(parentTransform.gameObject,
                    chainContainer.transform, true);
                chainGameObject.name = "Chain " + i + " at temp " + temperature;
                chainGameObject.transform.position += new Vector3(0, 0, -i * 70);

                chains[i] = new MCMCChain(graphBasedScene, chainGameObject.transform, temperature, (float)stepFactor * positionStep, (float)stepFactor * angleStep, pathParentSwapPropability);
            }

            DestroyImmediate(parentTransform.gameObject);

            // Now run metropolis hastings on all chains
            var bestLogEvalutation = chains[0].CurrentLogEvaluation;
            var bestLayout = chains[0].CopyCurrentLayout();

            for (int i = 0; i < stepsPerScheduleEntry; i += chainCount)
            {
                // Update Step
                var statistics = chains.Select(c => c.Step()).ToArray();
                foreach (var chain in chains)
                {
                    if (chain.CurrentLogEvaluation < bestLogEvalutation)
                    {
                        bestLogEvalutation = chain.CurrentLogEvaluation;
                        bestLayout = chain.CopyCurrentLayout();
                    }
                }

                // Swap Step
                if (chainCount > 1 && Random.value < chainSwapPropability)
                {
                    var swapFirstIndex = Random.Range(0, chainCount - 1);
                    var swapSecondIndex = swapFirstIndex + 1;

                    statistics[swapFirstIndex].IsSwapAccepted =
                    statistics[swapSecondIndex].IsSwapAccepted =
                        MCMCChain.SwapStep(chains[swapFirstIndex], chains[swapSecondIndex]);
                }

                yield return statistics;
            }

            // Then copy best output to Parent
            var chainForFinalResult = chains[0];
            chainForFinalResult.ApplySceneLayout(bestLayout);
            chainForFinalResult.ParentTransform.CopyFrom(chainContainer.transform);
            chainForFinalResult.ParentTransform.name = parentTransformName;
            DestroyImmediate(chainContainer);
        }

        SetRenderToTextureCamerasEnabled(true);
    }

    public List<Transform> GetHierachicalSchedule()
    {
        var graphBasedScene = GetComponent<GraphScene>();
        var sceneTransform = transform.GetChild(0);
        List<Transform> hierachicalSchedule = new List<Transform>();
        foreach (Transform child in sceneTransform)
        {
            if (graphBasedScene.GetUnsortedGraphNodeBehaviours(child) != null)
                hierachicalSchedule.Add(child);
        }
        hierachicalSchedule.Add(sceneTransform);
        return hierachicalSchedule;
    }

    public void PlaceObjects()
    {
        var parentTransform = transform;

        RemoveChildren(parentTransform);

        var sceneTransform = new GameObject("Scene").transform;
        sceneTransform.SetParent(parentTransform, false);

        var graphBasedScene = GetComponent<GraphScene>();
        var graph = graphBasedScene.GraphInstance;
        if (graph == null) return;

        List<GameObject> gameObjects = new List<GameObject>();

        for (int i = 0; i < graph.Nodes.Length; ++i)
        {
            var nodeType = graph.Nodes[i];

            var prefabs = Resources.LoadAll<GameObject>(GetAssetForNode(nodeType));
            if (prefabs == null || prefabs.Length == 0)
            {
                throw new System.IO.FileNotFoundException("Resource not found: " + GetAssetForNode(nodeType));
            }
            var prefab = prefabs[Random.Range(0, prefabs.Length)];

            var gameObject = Instantiate(prefab, sceneTransform);
            gameObject.name = "Node " + i + " " + nodeType;
            gameObject.GetComponent<GraphNodeBehaviour>().CorrespondsToNodeIndex = i;

            if(i > 0)
            {
                gameObject.transform.localRotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);
                gameObject.transform.localPosition = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
            }

            gameObjects.Add(gameObject);
        }

        if(hierachicalMetropolisHastings)
        {
            var clustering = graph.GetNodeHierachyClustering();

            for (int i = 1; i < graph.Nodes.Length; ++i)
            {
                var clusterParentIndex = clustering[i];
                if (clusterParentIndex != null)
                {
                    gameObjects[i].transform.SetParent(gameObjects[(int)clusterParentIndex].transform,
                        true);
                }
            }
        }

        // Already place on top relationships if possible
        // This could be made way more efficent :D
        var onTopRelationships = graph.Edges.Where(e => e.Item2.IsTypePartOfOrOnTopOf()).ToArray();
        foreach (var (topNode, _, bottomNode) in onTopRelationships)
        {
            var objectTop = gameObjects[(int)topNode];

            var objectBottom = gameObjects[(int)bottomNode];
            var graphNodeBottom = objectBottom.GetComponent<GraphNodeBehaviour>();
            var colliderBottom = graphNodeBottom.Collider;

            foreach (var hit in Physics.RaycastAll(colliderBottom.bounds.center
                    + Vector3.up * colliderBottom.bounds.extents.y * 2, Vector3.down))
            {
                if (hit.collider == colliderBottom)
                {
                    objectTop.transform.position = new Vector3(
                        objectBottom.transform.position.x + Random.Range(-colliderBottom.bounds.extents.x, colliderBottom.bounds.extents.x),
                        hit.point.y,
                        objectBottom.transform.position.z + Random.Range(-colliderBottom.bounds.extents.z, colliderBottom.bounds.extents.z));
                    break;
                }
            }
        }
        


        var pathConnectedToRelationships = graph.Edges.Where(e => e.Item2 == RelationshipType.PathConnectedTo).ToArray();
        foreach (var (pathNode, _, intersectionNode) in pathConnectedToRelationships)
        {
            gameObjects[(int)pathNode].transform.position = gameObjects[(int)intersectionNode].transform.position;
        }


        if (!pathGenerationEnabled)
        {
            var paths = graphBasedScene.GetComponentsInChildren<Path>();
            foreach (var path in paths)
            {
                DestroyImmediate(path);
            }

            var pathNodes = gameObjects.Select(p => p.GetComponent<GraphNodeBehaviour>()).Where(p => p.NodeType.StartsWith("Path"));
            foreach(var pathNode in pathNodes)
            {
                pathNode.ExpressionHierachy = new string[0];
            }
        }
    }


    public string BackgroundProgress { get; private set; } = null;
    public float BackgroundProgressValue { get; private set; } = 0.0f;
#if UNITY_EDITOR
    EditorCoroutine backgroundRoutine;
    public void StartGeneratingInBackground(System.Action repaint)
    {
        StopGeneratingInBackground();
        backgroundRoutine = EditorCoroutineUtility.StartCoroutine(GenerateInBackgroundCoroutine(repaint), this);
    }

    public void StopGeneratingInBackground()
    {
        if (backgroundRoutine != null) EditorCoroutineUtility.StopCoroutine(backgroundRoutine);
        backgroundRoutine = null;
        BackgroundProgress = null;
        BackgroundProgressValue = 0.0f;
    }
#endif

    public IEnumerator GenerateInBackgroundCoroutine(System.Action repaint, int updateGUISteps = 100)
    {
        var smoothingSteps = 500;

        var swapSmoothingQueue = new Queue<double>();
        var acceptanceSmoothingQueues = Enumerable.Range(0, chainCount).Select(i => new Queue<double>()).ToArray();

        var evaluation = Enumerable.Range(0, chainCount).Select(i => new List<double>()).ToArray();
        var acceptance = Enumerable.Range(0, chainCount).Select(i => new List<double>()).ToArray();
        var swaps = new List<double>();

        int step = 0;
        foreach (var value in RunMetropolisHastings())
        {
            if (value == null)
                continue;

            for (int i = 0; i < chainCount; ++i)
            {
                acceptanceSmoothingQueues[i].Enqueue(value[i].IsStepAccepted ? 1 : 0);

                if (acceptanceSmoothingQueues[i].Count > smoothingSteps)
                {
                    acceptanceSmoothingQueues[i].Dequeue();
                    acceptance[i].Add(acceptanceSmoothingQueues[i].Sum() / acceptanceSmoothingQueues[i].Count);
                }

                evaluation[i].Add(value[i].LogEvaluation);
            }

            swapSmoothingQueue.Enqueue(value.Any(p => p.IsSwapAccepted) ? 1 : 0);

            if (swapSmoothingQueue.Count > smoothingSteps)
            {
                swapSmoothingQueue.Dequeue();
                swaps.Add(swapSmoothingQueue.Sum() / swapSmoothingQueue.Count);
            }

            step += chainCount;
            if (step % updateGUISteps == 0)
            {
                BackgroundProgress = $"Step {step}/{metropolisHastingsSteps} for {value.First().NodeName}";
                BackgroundProgressValue = step / (float)metropolisHastingsSteps;
                repaint();
                yield return null;
            }
        }
        BackgroundProgress = $"Step {step}/{metropolisHastingsSteps}";
        BackgroundProgressValue = step / (float) metropolisHastingsSteps;
        repaint();
        yield return null;

        if(drawPlots)
        {
            var hierachicalSchedule = GetHierachicalSchedule();
            var stepsPerScheduleEntry = metropolisHastingsSteps / chainCount / hierachicalSchedule.Count;
            var groups = hierachicalSchedule.Select((h, i) => (h.name, (double)(i + 1) * stepsPerScheduleEntry));

            PlotViewer.Plot(Enumerable.Range(0, chainCount).Select(j => evaluation[j].Select((v, i) => ((double)i, v))),
                    "Log Evaluation",
                    Enumerable.Range(0, chainCount).Select(i => "chain " + i), groups);
            PlotViewer.Plot(Enumerable.Range(0, chainCount).Select(j => acceptance[j].Select((v, i) => ((double)i + smoothingSteps * 0.5f, v))),
                "Acceptance",
                Enumerable.Range(0, chainCount).Select(i => "chain " + i), groups);

            if (chainCount > 1)
            {
                PlotViewer.Plot(new List<IEnumerable<(double, double)>>() { swaps.Select((v, i) => ((double)i + smoothingSteps * 0.5f, v))},
                    "Swap Rate",
                    null, groups);
            }
        }

#if(UNITY_EDITOR)
        backgroundRoutine = null;
#endif
        BackgroundProgress = null;

        BackgroundProgressValue = 0.0f;
    }
}
