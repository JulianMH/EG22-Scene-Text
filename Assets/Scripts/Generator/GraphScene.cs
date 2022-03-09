using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class GraphScene : MonoBehaviour, ISerializationCallbackReceiver
{
    public SceneRelationshipGraph GraphInstance { get; set; }

    [HideInInspector]
    [SerializeField]
    private string serializedGraph = "";

    public void OnBeforeSerialize()
    {
        if (GraphInstance != null)
        {
            serializedGraph = new SceneRelationshipGraphSaveFile(GraphInstance).ToJson();
        }
        else
        {
            serializedGraph = "";
        }
    }

    public void OnAfterDeserialize()
    {
        if (serializedGraph != null && serializedGraph != "")
        {
            GraphInstance = new SceneRelationshipGraphSaveFile(serializedGraph).ToGraph();
        }
        else
        {
            GraphInstance = null;
        }
    }

    public double CollisionPower = 6.0;

    public GraphNodeBehaviour[] GetUnsortedGraphNodeBehaviours(Transform parentTransform = null)
    {
        var graph = GraphInstance;
        parentTransform = parentTransform ?? transform;

        if (graph == null)
        {
            return null;
        }

        var resultBehaviours = new List<GraphNodeBehaviour>();

        foreach (Transform child in parentTransform)
        {
            var nodeBehaviour = child.GetComponent<GraphNodeBehaviour>();
            if (nodeBehaviour != null)
            {
                if (!nodeBehaviour.MatchesNodeType(graph.Nodes[nodeBehaviour.CorrespondsToNodeIndex]))
                {
                    return null;
                }
                else
                {
                    resultBehaviours.Add(nodeBehaviour);
                }
            }
        }

        if (resultBehaviours.Any())
            return resultBehaviours.ToArray();
        else
            return null;
    }

    public GraphNodeBehaviour[] GetAllGraphNodeBehavioursRecursively(Transform parentTransform = null, bool allowIncompleteSet = false)
    {
        var graph = GraphInstance;
        parentTransform = parentTransform ?? transform;

        if (graph == null)
        {
            throw new System.NullReferenceException("Graph not instatiated yet.");
        }

        var resultBehaviours = new GraphNodeBehaviour[graph.Nodes.Length];
        var nodeBehaviours = parentTransform.GetComponentsInChildren<GraphNodeBehaviour>();

        foreach (var nodeBehaviour in nodeBehaviours)
        {
            if (!nodeBehaviour.MatchesNodeType(graph.Nodes[nodeBehaviour.CorrespondsToNodeIndex]))
            {
                throw new System.InvalidOperationException(
                    $"GraphNodeBehaviour {nodeBehaviour.CorrespondsToNodeIndex} is for {nodeBehaviour.NodeType} but should be for {graph.Nodes[nodeBehaviour.CorrespondsToNodeIndex]}.");

            }
            else
            {
                resultBehaviours[nodeBehaviour.CorrespondsToNodeIndex] = nodeBehaviour;
            }            
        }

        if ((allowIncompleteSet && resultBehaviours.Any(p => p != null)) || resultBehaviours.All(p => p != null))
            return resultBehaviours;
        else
        {
            var missingBehaviourNodeNames = resultBehaviours
                .Select((b, i) => (b, i))
                .Where(item => item.b == null)
                .Select(item => graph.Nodes[item.i]);

            throw new System.InvalidOperationException(
                "Could not find GraphNodeBehaviours for " + string.Join(", ", missingBehaviourNodeNames));
        }
    }

    public double? CalculateEvaluationFunction(GraphNodeBehaviour[] behaviours, Path[] paths)
    {
        return CalculateEvaluationFunctionComponents(behaviours, paths)?.logEvaluation;
    }

    public (double logEvaluation, double logFactorCollision, double logFactorCollisionPaths, double logFactorPaths, double logFactorRelationships)? CalculateEvaluationFunctionComponents(GraphNodeBehaviour[] behaviours, Path[] paths)
    {
        if (behaviours == null)
            return null;

        var graph = GraphInstance;

        var logFactorCollision = CalulateLogFactorCollision(behaviours);
        double logFactorCollisionPaths = CalculateLogFactorCollisionPaths(behaviours, paths);
        double logFactorPaths = CalculateLogFactorPaths(paths);
        double logFactorRelationships = CalculateLogFactorRelationships(behaviours, graph);
        var logFactor = logFactorCollision + logFactorCollisionPaths + logFactorPaths + logFactorRelationships;

        return (-logFactor, -logFactorCollision, -logFactorCollisionPaths, -logFactorPaths, -logFactorRelationships);
    }

    private static double CalculateLogFactorRelationships(GraphNodeBehaviour[] behaviours, SceneRelationshipGraph graph)
    {
        var logFactorRelationships = 0.0;
        foreach (var (a, relatesTo, b) in graph.Edges)
        {
            var graphNodeA = behaviours[(int)a];
            var graphNodeB = behaviours[(int)b];

            if (graphNodeA != null && graphNodeB != null)
            {
                var objectA = graphNodeA.transform;
                var objectB = graphNodeB.transform;

                var logPenalty = relatesTo.ComputeLogEvaluation(graphNodeA, graphNodeB);

#if UNITY_EDITOR
                if (double.IsInfinity(logPenalty))
                {
                    throw new System.ArithmeticException($"{graphNodeA.name} ({a}) {relatesTo} {graphNodeB.name} penalty is infinite.");
                }
#endif
                logFactorRelationships += logPenalty;
            }
        }

        return logFactorRelationships;
    }

    private static double CalculateLogFactorPaths(Path[] paths)
    {
        // Encourage the following for paths:
        // - Keep total length as short as possible
        // - limit angles to 60 degrees or so
        // - also limit angles between incoming paths at designated intersection nodes to 60 degrees or so
        // - No unintended Intersections (this *should* be handled by having angle and length limits anyway)

        var logFactorPaths = 0.0;
        foreach (var path in paths)
        {
            var logFactorPath = 0.0;

            logFactorPath += path.GetAngles().Sum(a => RelationshipTypeEvaluation.FactorLess(Math.Abs(a), Mathf.Deg2Rad * path.TotalAngleLimit));
            logFactorPath += RelationshipTypeEvaluation.FactorLess(path.GetLength(), path.TotalLengthLimit);
            logFactorPaths += logFactorPath;

            if (path.IsIntersection)
            {
                var logFactorIntersection = 0.0;

                var connectedPaths = paths.Where(p => p.Parent == path);
                var connectedPathAngles = connectedPaths
                    .Select(p => p.LineSegments.First())
                    .Select(l => (l.to - l.from).normalized)
                    .Select(v => Mathf.Rad2Deg * Mathf.Atan2(v.z, v.x))
                    .OrderBy(a => a).ToArray();

                for (int i = 0; i < connectedPathAngles.Length; ++i)
                {
                    int j = (i + 1) % connectedPathAngles.Length;

                    var angle = Mathf.DeltaAngle(connectedPathAngles[i], connectedPathAngles[j]);
                    logFactorIntersection += RelationshipTypeEvaluation.FactorGreater(Math.Abs(angle), path.TotalAngleLimit);
                }

            }
        }

        return logFactorPaths;
    }

    private double CalculateLogFactorCollisionPaths(GraphNodeBehaviour[] behaviours, Path[] paths)
    {
        var logFactorCollisionPaths = 0.0;
        for (int a = 0; a < behaviours.Length; ++a)
        {
            var graphNodeA = behaviours[a];

            for (int b = 0; b < paths.Length; ++b)
            {
                var pathB = paths[b];

                if (graphNodeA != null && graphNodeA.Collider != null && graphNodeA.Collider.bounds.extents.y > 0.01f && pathB != null && (pathB.IgnoreOwnerCollider || pathB.gameObject != graphNodeA.gameObject))
                {
                    var colliderA = graphNodeA.Collider;

                    var penalty = 1.0;
                    // EstimatePenetrationDepth requires one of the collider to be convex                    
                    if (pathB.EstimatePenetrationDepth(colliderA, out var distance))
                    {
                        penalty = Math.Abs(Math.Pow(1.0 - distance / (pathB.Width * 0.5), CollisionPower));
                    }
                    logFactorCollisionPaths += System.Math.Log(penalty);
                }
            }
        }

        return logFactorCollisionPaths;
    }

    private double CalulateLogFactorCollision(GraphNodeBehaviour[] behaviours)
    {
        double logFactorCollision = 0.0;
        for (int a = 0; a < behaviours.Length; ++a)
        {
            for (int b = a + 1; b < behaviours.Length; ++b)
            {
                var graphNodeA = behaviours[a];
                var graphNodeB = behaviours[b];

                if (graphNodeA != null && graphNodeB != null)
                {
                    var colliderA = graphNodeA.Collider;
                    var colliderB = graphNodeB.Collider;


                    if (colliderA is MeshCollider m && !m.convex)
                        Debug.Log(colliderA.gameObject.name + " has non convex mesh collider :(");

                    if (graphNodeA.AreaTerrainCollider != null && graphNodeB.AreaTerrainCollider != null)
                    {
                        colliderA = graphNodeA.AreaTerrainCollider;
                        colliderB = graphNodeB.AreaTerrainCollider;
                    }

                    var penalty = 1.0;

                    // ComputePenetration requires one of the two colliders to be convex                    
                    if (colliderA != null && colliderB != null &&
                        Physics.ComputePenetration(colliderA,
                        colliderA.transform.position,
                        colliderA.transform.rotation,
                        colliderB,
                        colliderB.transform.position,
                        colliderB.transform.rotation,
                        out var direction, out var distance))
                    {
                        penalty = Math.Abs(Math.Pow(1.0 - distance / (colliderA.bounds.extents + colliderB.bounds.extents).magnitude, CollisionPower));
                    }

                    logFactorCollision += System.Math.Log(penalty);
                }
            }
        }

        return logFactorCollision;
    }

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

    public void PlaceObjects(SceneLayout sceneLayout)
    {
        var parentTransform = transform;

        RemoveChildren(parentTransform);

        var sceneTransform = new GameObject("Scene").transform;
        sceneTransform.SetParent(parentTransform, false);

        GraphNodeBehaviour[] behaviours = new GraphNodeBehaviour[sceneLayout.Objects.Length];
        for (int i = 0; i < sceneLayout.Objects.Length; ++i)
        {
            var nodeType = sceneLayout.Objects[i].NodeType;
            var nodeIndex = sceneLayout.Objects[i].CorrespondsToNodeIndex;
            var prefab = Resources.Load<GameObject>(GetAssetForNode(nodeType));
            if (prefab == null)
            {
                throw new System.IO.FileNotFoundException("Resource not found: " + GetAssetForNode(nodeType));
            }

            var gameObject = Instantiate(prefab, sceneTransform);
            gameObject.name = "Node " + nodeIndex + " " + nodeType;
            var graphNodeBehaviour = gameObject.GetComponent<GraphNodeBehaviour>();
            graphNodeBehaviour.CorrespondsToNodeIndex = nodeIndex;
            behaviours[i] = graphNodeBehaviour;
        }

        for (int i = 0; i < sceneLayout.Objects.Length; ++i)
        {
            var parentIndex = sceneLayout.Objects[i].ParentIndex;
            if(parentIndex >= 0)
            {
                behaviours[i].transform.parent = behaviours[parentIndex].transform;
            }
        }

        if(sceneLayout.Paths.Length == 0)
        {
            var paths = GetComponentsInChildren<Path>();
            foreach (var path in paths)
            {
                DestroyImmediate(path);
            }

            var pathNodes = behaviours.Select(p => p.GetComponent<GraphNodeBehaviour>()).Where(p => p.NodeType.StartsWith("Path"));
            foreach (var pathNode in pathNodes)
            {
                pathNode.ExpressionHierachy = new string[0];
            }            
        }

        sceneLayout.Apply(behaviours, GetComponentsInChildren<Path>());
    }
}