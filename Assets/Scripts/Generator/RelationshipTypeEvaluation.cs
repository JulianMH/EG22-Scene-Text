using UnityEngine;
using static RelationshipType;
using System;

public static class RelationshipTypeEvaluation
{
    private static double GetEvaluationOnTopOf(GraphNodeBehaviour objectA, GraphNodeBehaviour objectB)
    {
        var logFactor = FactorEquals(objectA.transform.position.y - objectB.Collider.bounds.max.y);

        if (objectB.Collider is BoxCollider boxCollider)
        {
            var localPosition = boxCollider.transform.InverseTransformPoint(objectA.transform.position) - boxCollider.center;
            logFactor += FactorRange(localPosition.x, -boxCollider.size.x / 2.2, boxCollider.size.x / 2.2);
            logFactor += FactorRange(localPosition.z, -boxCollider.size.z / 2.2, boxCollider.size.z / 2.2);
        }
        else
        {
            logFactor += FactorRange(objectA.transform.position.x, objectB.Collider.bounds.min.x, objectB.Collider.bounds.max.x);
            logFactor += FactorRange(objectA.transform.position.z, objectB.Collider.bounds.min.z, objectB.Collider.bounds.max.z);
        }

        return logFactor;
    }

    private static double GetEvaluationOnTopOfMiddle(GraphNodeBehaviour objectA, GraphNodeBehaviour objectB)
    {
        var logFactor = FactorEquals(objectA.Collider.bounds.min.y - objectB.Collider.bounds.max.y);

        logFactor += FactorEquals(
            new Vector2(objectA.transform.position.x - objectB.transform.position.x,
            objectA.transform.position.z - objectB.transform.position.z).magnitude);
        return logFactor;
    }

    private static double GetEvaluationNextTo(GraphNodeBehaviour objectA, GraphNodeBehaviour objectB)
    {
        var colliderA = objectA.Collider;
        var colliderB = objectB.Collider;

        if (objectA.AreaTerrainCollider != null && objectB.AreaTerrainCollider != null)
        {
            colliderA = objectA.AreaTerrainCollider;
            colliderB = objectB.AreaTerrainCollider;
        }

        if (colliderA == null || colliderB == null)
        {
            if (objectB.NodeType.StartsWith("Path"))
            {
                var path = objectB.GetComponent<Path>();
                if (path == null)
                    return 0;

                var closestPointToAOnB = path.GetClosestPointTo(colliderA.bounds.center);
                var closestPointToBOnA = colliderA.ClosestPoint(closestPointToAOnB);

                return FactorLess((closestPointToAOnB - closestPointToBOnA).magnitude - (path.Width / 2), 2);
            }
            else
            {
                throw new System.InvalidOperationException($"Relationship of kind {objectA.NodeType} next to {objectB.NodeType} is not supported.");
            }
        }
        else
        {
            var closestPointToBOnA = colliderA.ClosestPoint(colliderB.transform.position);
            var closestPointToAOnB = colliderB.ClosestPoint(closestPointToBOnA);

            return FactorLess((closestPointToAOnB - closestPointToBOnA).magnitude, 2);
        }

    }

    private static double GetEvaluationOnTopOfEdge(GraphNodeBehaviour objectA, GraphNodeBehaviour objectB)
    {
        var colliderA = objectA.Collider;
        var colliderB = objectB.Collider;

        if (objectB.Collider is BoxCollider boxCollider)
        {
            var closestPointToAOnB = boxCollider.ClosestPoint(colliderA.transform.position);
            var closestPointFromCenter = boxCollider.transform.InverseTransformPoint(closestPointToAOnB) - boxCollider.center;

            var diff = Mathf.Min(
                Mathf.Abs(Math.Abs(closestPointFromCenter.x) - boxCollider.size.x * 0.5f),
                Mathf.Abs(Mathf.Abs(closestPointFromCenter.z) - boxCollider.size.z * 0.5f))
                + Vector3.Distance(colliderA.transform.position, closestPointToAOnB);

            return FactorEquals(diff) + FactorEquals(colliderA.bounds.min.y - colliderB.bounds.max.y);

        }
        else
        {
            var closestPointToAOnB = colliderB.bounds.ClosestPoint(colliderA.transform.position);
            var closestPointFromCenter = colliderB.transform.InverseTransformPoint(closestPointToAOnB) - colliderB.transform.InverseTransformPoint(colliderB.bounds.center);

            var diff = Mathf.Min(
                Mathf.Abs(Math.Abs(closestPointFromCenter.x) - colliderB.bounds.extents.x),
                Mathf.Abs(Mathf.Abs(closestPointFromCenter.z) - colliderB.bounds.extents.z))
                + Vector3.Distance(colliderA.transform.position, closestPointToAOnB);

            return FactorEquals(diff) + FactorEquals(colliderA.bounds.min.y - colliderB.bounds.max.y);
        }
    }

    private static double GetEvaluationOnTopOfFrontEdge(GraphNodeBehaviour objectA, GraphNodeBehaviour objectB)
    {
        var colliderA = objectA.Collider;
        var colliderB = objectB.Collider;

        if (objectB.Collider is BoxCollider boxCollider)
        {
            var closestPointToAOnB = boxCollider.ClosestPoint(colliderA.transform.position);
            var closestPointFromCenter = boxCollider.transform.InverseTransformPoint(closestPointToAOnB) - boxCollider.center;
            
            var diff = Mathf.Abs(closestPointFromCenter.z - boxCollider.size.z * 0.5f)
                + Vector3.Distance(colliderA.transform.position, closestPointToAOnB);

            return FactorEquals(diff) + FactorEquals(colliderA.bounds.min.y - colliderB.bounds.max.y);
        }
        else
        {
            var closestPointToAOnB = colliderB.bounds.ClosestPoint(colliderA.transform.position);
            var closestPointFromCenter = colliderB.transform.InverseTransformPoint(closestPointToAOnB) - colliderB.transform.InverseTransformPoint(colliderB.bounds.center);

            var diff = Mathf.Abs(closestPointFromCenter.z - colliderB.bounds.extents.z)
                + Vector3.Distance(colliderA.transform.position, closestPointToAOnB);

            return FactorEquals(diff) + FactorEquals(colliderA.bounds.min.y - colliderB.bounds.max.y);
        }
    }

    private static double GetEvaluationNextToAndFacingDirection(GraphNodeBehaviour objectA, GraphNodeBehaviour objectB, Vector3 direction)
    {
        var objectBToObjectA = objectA.transform.position - objectB.transform.position;

        return FactorPointingInSameDirection(direction, objectBToObjectA.normalized) + GetEvaluationNextTo(objectA, objectB);
    }

    private static double GetEvaluationBesidesOf(GraphNodeBehaviour objectA, GraphNodeBehaviour objectB)
    {
        var objectBLeft = objectB.transform.rotation * Vector3.left;
        var objectBRight = objectB.transform.rotation * Vector3.right;
        var objectBToObjectA = objectA.transform.position - objectB.transform.position;
        var objectDistance = objectBToObjectA.magnitude;
        objectBToObjectA /= objectDistance;

        return Math.Max(FactorPointingInSameDirection(objectBRight, objectBToObjectA), FactorPointingInSameDirection(objectBLeft, objectBToObjectA))  +GetEvaluationNextTo(objectA, objectB);
    }

    private static double GetEvaluationFacing(GraphNodeBehaviour objectA, GraphNodeBehaviour objectB)
    {
        var objectAForward = objectA.transform.rotation * Vector3.forward;
        var objectAToObjectB = objectB.transform.position - objectA.transform.position;

        return FactorPointingInSameDirection(objectAForward, objectAToObjectB.normalized);
    }

    private static double GetEvaluationFacingSameDirection(GraphNodeBehaviour objectA, GraphNodeBehaviour objectB)
    {
        var objectAForward = objectA.transform.rotation * Vector3.forward;
        var objectBForward = objectB.transform.rotation * Vector3.forward;

        return FactorPointingInSameDirection(objectAForward, objectBForward);
    }

    private static double GetEvaluationFacingOrthogonalDirection(GraphNodeBehaviour objectA, GraphNodeBehaviour objectB)
    {
        var objectAForward = objectA.transform.rotation * Vector3.forward;
        var objectBForward = objectB.transform.rotation * Vector3.forward;

        return FactorDirectionsAreOrthogonal(objectAForward, objectBForward);
    }

    private static double GetEvaluationFacingAwayFrom(GraphNodeBehaviour objectA, GraphNodeBehaviour objectB)
    {
        var objectAForward = objectA.transform.rotation * Vector3.forward;
        var objectAToObjectB = (objectB.transform.position - objectA.transform.position).normalized;

        return FactorRange(Vector3.Dot(objectAForward, objectAToObjectB), -1.0, 0.0);
    }

    public static double ComputeLogEvaluation(this RelationshipType relatesTo, GraphNodeBehaviour objectA, GraphNodeBehaviour objectB)
    {
        switch (relatesTo)
        {
            case OnTopOf:
            case IsPartOf:
                return GetEvaluationOnTopOf(objectA, objectB);
            case OnTopOfEdge:
            case IsPartOfEdge:
                return GetEvaluationOnTopOfEdge(objectA, objectB);
            case OnTopOfFrontEdge:
            case IsPartOfFrontEdge:
                return GetEvaluationOnTopOfFrontEdge(objectA, objectB);
            case OnTopOfMiddle:
            case IsPartOfMiddle:
                return GetEvaluationOnTopOfMiddle(objectA, objectB);

            case NextToChooseDirection:
            case NextTo:
                return GetEvaluationNextTo(objectA, objectB);

            case NextToNorth:
                return GetEvaluationNextToAndFacingDirection(objectA, objectB, Vector3.forward);
            case NextToSouth:
                return GetEvaluationNextToAndFacingDirection(objectA, objectB, Vector3.back);
            case NextToEast:
                return GetEvaluationNextToAndFacingDirection(objectA, objectB, Vector3.right);
            case NextToWest:
                return GetEvaluationNextToAndFacingDirection(objectA, objectB, Vector3.left);

            case NextToChooseSide:
                return GetEvaluationBesidesOf(objectA, objectB);

            case NextToLeft:
                return GetEvaluationNextToAndFacingDirection(objectA, objectB, objectB.transform.rotation * Vector3.left);
            case NextToRight:
                return GetEvaluationNextToAndFacingDirection(objectA, objectB, objectB.transform.rotation * Vector3.right);
            case NextToFront:
                return GetEvaluationNextToAndFacingDirection(objectA, objectB, objectB.transform.rotation * Vector3.forward);
            case NextToBehind:
                return GetEvaluationNextToAndFacingDirection(objectA, objectB, objectB.transform.rotation * Vector3.back);

            case FacingTowards:
            case ImplicitFacingTowards:
                return GetEvaluationFacing(objectA, objectB);
            case FacingSameDirection:
            case ImplicitFacingSameDirection:
                return GetEvaluationFacingSameDirection(objectA, objectB);
            case FacingOrthogonalDirection:
            case ImplicitFacingOrthogonalDirection:
                return GetEvaluationFacingOrthogonalDirection(objectA, objectB);
            case FacingAwayFrom:
            case ImplicitFacingAwayFrom:
                return GetEvaluationFacingAwayFrom(objectA, objectB);

            case PathConnectedTo:
                return 0;

            default:
                throw new NotImplementedException(relatesTo.ToString());
        }
    }

    public static double FactorEquals(double x, double y)
    {
        return FactorEquals(System.Math.Abs(x - y));
    }

    public static double FactorEquals(double difference)
    {
        // Yeh et al 2012
        var variance = 0.1f;
        return System.Math.Log(GaussianDensity(0, difference, variance) / GaussianDensity(0, 0, variance));
    }

    public static double FactorGreater(double x, double y)
    {
        // Yeh et al 2012
        var h = 3.0;
        var difference = x - y;
        return System.Math.Log(Sigmoid(difference, h));
    }

    public static double FactorLess(double x, double y)
    {
        return FactorGreater(y, x);
    }

    public static double FactorRange(double x, double min, double max)
    {
        return FactorGreater(x, min) + FactorLess(x, max);
    }

    public static double FactorPointingInSameDirection(Vector3 a, Vector3 b)
    {
        var dotProduct = Vector3.Dot(a, b);
        return FactorEquals(Mathf.Acos(dotProduct), 0.0) * 2;
    }

    public static double FactorDirectionsAreOrthogonal(Vector3 a, Vector3 b)
    {
        var dotProduct = Vector3.Dot(a, b);
        return FactorEquals(Math.Abs(Mathf.Acos(dotProduct)), Math.PI / 2.0f) * 2;
    }

    public static double GaussianDensity(double x, double mean, double variance)
    {
        var first = 1.0 / Math.Sqrt(2.0 * Mathf.PI * variance);
        var second = Math.Exp(-(Math.Pow(x - mean, 2.0) / (2.0 * variance)));
        return first * second;
    }

    public static double Sigmoid(double x, double h)
    {
        return 1.0f / (1.0f + Math.Exp(-h * x));
    }
}
