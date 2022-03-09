using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MCMCChain
{
    public struct StepStatistics
    {
        public StepStatistics(string nodeName, double logEvaluation, bool isStepAccepted) { NodeName = nodeName; LogEvaluation = logEvaluation; IsStepAccepted = isStepAccepted; IsSwapAccepted = false; }

        public string NodeName;
        public double LogEvaluation;
        public bool IsStepAccepted;
        public bool IsSwapAccepted;
    }
    
    private double inverseTemperature;
    private Vector3 stepPositionChange;
    private float stepAngleChange;
    private double propabilityPathParentSwap;

    private GraphScene graphBasedScene;
    private GraphNodeBehaviour[] behaviours;
    private GraphNodeBehaviour[] evaluationBehaviours;
    private Path[] paths;

    private SceneLayout currentLayout;

    private (MoveDelegate move, int indexToTransform)[] possibleMoves;

    public Transform ParentTransform { get; }
    public double CurrentLogEvaluation { get; private set; }

    public MCMCChain(GraphScene graphBasedScene, Transform parentTransform, double temperature, Vector3 stepPositionChange, float stepAngleChange, double pathParentSwapPropability)
    {
        this.graphBasedScene = graphBasedScene;
        this.ParentTransform = parentTransform;

        this.stepPositionChange = stepPositionChange;
        this.stepAngleChange = stepAngleChange;

        this.propabilityPathParentSwap = pathParentSwapPropability;

        inverseTemperature = 1.0 / temperature;

        evaluationBehaviours = graphBasedScene.GetAllGraphNodeBehavioursRecursively(parentTransform, true);
        // Add Parent to evaluation, since parentTransform is usually a temporary container object for generation,
        // but some constraints relate to the parentObject, so we should be able to use it in evaluation.
        {
            var nodeBehaviour = parentTransform.parent?.GetComponent<GraphNodeBehaviour>();
            if (nodeBehaviour?.MatchesNodeType(graphBasedScene.GraphInstance.Nodes[nodeBehaviour.CorrespondsToNodeIndex]) == true)
            {
                evaluationBehaviours[nodeBehaviour.CorrespondsToNodeIndex] = nodeBehaviour;
            }
        }

        behaviours = graphBasedScene.GetUnsortedGraphNodeBehaviours(parentTransform);
        paths = parentTransform.GetComponentsInChildren<Path>();
        possibleMoves = GetPossibleMoves();

        currentLayout = SceneLayout.GetCurrentLayout(behaviours, paths);

        CurrentLogEvaluation = graphBasedScene.CalculateEvaluationFunction(evaluationBehaviours, paths).Value;
    }

    public StepStatistics Step()
    {
        var newLayout = SampleJumpDistribution();

        double newLogEvaluation = 0.0;
        try
        {
            newLogEvaluation = graphBasedScene.CalculateEvaluationFunction(evaluationBehaviours, paths).Value;
        }
        catch (System.ArithmeticException) { }

        var acceptance = System.Math.Exp((inverseTemperature * -newLogEvaluation) - (inverseTemperature * -CurrentLogEvaluation));

        if (double.IsInfinity(CurrentLogEvaluation) || acceptance > Random.value)
        {
            currentLayout = newLayout;
            CurrentLogEvaluation = newLogEvaluation;
            return new StepStatistics(ParentTransform.parent.gameObject.name, newLogEvaluation, true);
        }
        else
        {
            ApplySceneLayout(currentLayout);
            return new StepStatistics(ParentTransform.parent.gameObject.name, CurrentLogEvaluation, false);
        }
    }

    // Source: https://stackoverflow.com/a/218600
    private static float SampleGaussian(float mean, float deviation)
    {
        float u1 = 1f - UnityEngine.Random.Range(0.0f, 0.999f); //uniform(0,1] random float
        float u2 = 1f - UnityEngine.Random.Range(0.0f, 0.999f);
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                     Mathf.Sin(2.0f * Mathf.PI * u2);
        return mean + deviation * randStdNormal; //random normal(mean,stdDev^2)
    }

    private void DiffusionMovePosition(SceneLayout newLayout, int indexToTransform)
    {
        var position = newLayout.Objects[indexToTransform].Position;

        newLayout.Objects[indexToTransform].Position = new Vector3(SampleGaussian(position.x, stepPositionChange.x),
            SampleGaussian(position.y, stepPositionChange.y),
            SampleGaussian(position.z, stepPositionChange.z));
    }

    private void DiffusionMoveRotation(SceneLayout newLayout, int indexToTransform)
    {
        var angle = newLayout.Objects[indexToTransform].RotationY;
        newLayout.Objects[indexToTransform].RotationY = SampleGaussian(angle, stepAngleChange);
    }

    private void SwapMove(SceneLayout newLayout, int indexToTransform)
    {
        var position = newLayout.Objects[indexToTransform].Position;
        var angle = newLayout.Objects[indexToTransform].RotationY;

        var possibleSelections = behaviours.Select((b, i) => (b, i))
            .Where(item => item.b != null && item.i != indexToTransform && item.b.AllowSwapMove)
            .ToArray();
        if (!possibleSelections.Any())
            return;

        var swapIndexToTransform = possibleSelections[Random.Range(0, possibleSelections.Length)].i;
        var swapPosition = newLayout.Objects[swapIndexToTransform].Position;
        var swapAngle = newLayout.Objects[swapIndexToTransform].RotationY;

        if (stepPositionChange.y == 0)
        {
            newLayout.Objects[indexToTransform].Position = new Vector3(swapPosition.x, position.y, swapPosition.z);
            newLayout.Objects[swapIndexToTransform].Position = new Vector3(position.x, swapPosition.y, position.z);
            newLayout.Objects[indexToTransform].RotationY = swapAngle;
            newLayout.Objects[swapIndexToTransform].RotationY = angle;
        }
        else
        {
            newLayout.Objects[indexToTransform].Position = swapPosition;
            newLayout.Objects[swapIndexToTransform].Position = position;
            newLayout.Objects[indexToTransform].RotationY = swapAngle;
            newLayout.Objects[swapIndexToTransform].RotationY = angle;
        }
    }

    private void PathMove(SceneLayout newLayout, int indexToTransform)
    {
        var pathIndex = indexToTransform;
        var path = paths[pathIndex];
        var pathParentIndex = newLayout.Paths[pathIndex].ParentIndex;
        var canidatePaths = Enumerable.Range(0, paths.Length).Where(i => i != pathParentIndex && newLayout.CouldBeParentPathIndex(i, pathIndex)).ToArray();

        if (Random.value < propabilityPathParentSwap && !path.IsRootPath && pathParentIndex != -1 && canidatePaths.Any())
        {
            var pathNewParentIndex = Random.Range(0, canidatePaths.Length);
            newLayout.Paths[pathIndex].ParentIndex = canidatePaths[pathNewParentIndex];
        }
        else
        {
            var pathNodeIndex = Random.Range(0, path.LocalPositions.Length);
            var pathNodePosition = newLayout.Paths[pathIndex].NodePositions[pathNodeIndex];

            var isFirstNode = pathNodeIndex == 0;
            var isLastNode = pathNodeIndex == path.LocalPositions.Length - 1;

            var newPathNodePosition = new Vector3(
                Mathf.Clamp(SampleGaussian(pathNodePosition.x, stepPositionChange.x), -30f, 30f),
                Mathf.Clamp(SampleGaussian(pathNodePosition.y, stepPositionChange.y), -30f, 30f),
                Mathf.Clamp(SampleGaussian(pathNodePosition.z, stepPositionChange.z), -30f, 30f));

            // For in between nodes we want to push them towards the middle of two vertices
            if (!isLastNode && !isFirstNode)
            {
                var fromPrevious = pathNodePosition - newLayout.Paths[pathIndex].NodePositions[pathNodeIndex - 1];
                var fromNext = pathNodePosition - newLayout.Paths[pathIndex].NodePositions[pathNodeIndex + 1];
                var previousToNext = newLayout.Paths[pathIndex].NodePositions[pathNodeIndex + 1] -
                    newLayout.Paths[pathIndex].NodePositions[pathNodeIndex - 1];

                var fromPreviousMagnitude = fromPrevious.magnitude;
                var fromNextMagnitude = fromNext.magnitude;

                if (fromPreviousMagnitude > fromNextMagnitude)
                {
                    newPathNodePosition += Mathf.Abs(SampleGaussian(0, stepPositionChange.x)) * -previousToNext.normalized;
                }
                else if (fromPreviousMagnitude < fromNextMagnitude)
                {
                    newPathNodePosition += Mathf.Abs(SampleGaussian(0, stepPositionChange.x)) * previousToNext.normalized;
                }
            }
            // Start And Nodes might be fixed in location
            else
            {
                if ((isFirstNode && path.FixStartX) || (isLastNode && path.FixEndX))
                    newPathNodePosition.x = pathNodePosition.x;
                if ((isFirstNode && path.FixStartZ) || (isLastNode && path.FixEndZ))
                    newPathNodePosition.z = pathNodePosition.z;
            }

            newLayout.Paths[pathIndex].NodePositions[pathNodeIndex] = newPathNodePosition;
        }
    }

    delegate void MoveDelegate(SceneLayout newLayout, int indexToTransform);

    private (MoveDelegate move, int index)[] GetPossibleMoves()
    {
        var possibleMoves = new List<(MoveDelegate move, int index)>();

        for (int i = 0; i < behaviours.Length; ++i)
        {
            var behaviour = behaviours[i];
            if (behaviours[i] != null)
            {
                // 1. Randomly change position
                if (behaviour.AllowDiffusionPositionMove)
                {
                    possibleMoves.Add((DiffusionMovePosition, i));
                }
                // 2. Randomly change rotation
                if (behaviour.AllowDiffusionRotationMove)
                {
                    possibleMoves.Add((DiffusionMoveRotation, i));
                }
                // 3. Randomly swap with another object
                if (behaviour.AllowSwapMove)
                {
                    possibleMoves.Add((SwapMove, i));
                }
            }
        }
        for (int i = 0; i < paths.Length; ++i)
        {
            possibleMoves.Add((PathMove, i));
        }

        return possibleMoves.ToArray();
    }

    public SceneLayout SampleJumpDistribution()
    {
        // Only transform a single object inspired by Merell et al. 2011

        var newLayout = currentLayout.Copy();

        // Select an object to transform first

        // Let's select between possible transformations with equal propability
        var (selectedMove, indexToTransform) = possibleMoves[Random.Range(0, possibleMoves.Length)];
        selectedMove(newLayout, indexToTransform);

        ApplySceneLayout(newLayout);

        return newLayout;
    }


    public void ApplySceneLayout(SceneLayout sceneLayout)
    {
        sceneLayout.Apply(behaviours, paths);
    }

    public SceneLayout CopyCurrentLayout()
    {
        return currentLayout.Copy();
    }

    public static bool SwapStep(MCMCChain firstChain, MCMCChain secondChain)
    {
        var acceptance = System.Math.Exp((firstChain.inverseTemperature - secondChain.inverseTemperature) *
            (firstChain.CurrentLogEvaluation - secondChain.CurrentLogEvaluation));

        if (acceptance > Random.value)
        {
            var swapEvaluation = firstChain.CurrentLogEvaluation;
            var swapLayout = firstChain.currentLayout;
            firstChain.CurrentLogEvaluation = secondChain.CurrentLogEvaluation;
            firstChain.currentLayout = secondChain.currentLayout;
            secondChain.CurrentLogEvaluation = swapEvaluation;
            secondChain.currentLayout = swapLayout;

            return true;
        }
        else
        {
            return false;
        }
    }
}