using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

[ExecuteAlways]
public class Path : MonoBehaviour
{
    [SerializeField]
    public Color pathColor = new Color(0.639f, 0.494f, 0.376f);

    [SerializeField]
    public Vector3[] LocalPositions = { };

    [SerializeField]
    public Path Parent = null;

    [SerializeField]
    public bool IsRootPath = false;

    [SerializeField, Range(0.5f, 20f)]
    public float Width = 1.0f;

    [SerializeField]
    public LineRenderer renderTextureLineRenderer = null;

    [SerializeField]
    public GameObject terrainGameObject = null;

    [SerializeField]
    public bool FixStartX = false;

    [SerializeField]
    public bool FixStartZ = false;

    [SerializeField]
    public bool FixEndX = false;

    [SerializeField]
    public bool FixEndZ = false;

    [SerializeField]
    public bool IgnoreOwnerCollider = false;

    [SerializeField]
    public float TotalAngleLimit = 60f;

    [SerializeField]
    public float TotalLengthLimit = 1f;

    [SerializeField]
    public bool IsIntersection = false;

    public Vector3[] Positions => LocalPositions.Select(p => transform.TransformPoint(p)).ToArray();

    public IEnumerable<Vector3> PositionsWithParent {
        get
        {
            var positions = Positions;
            if (Parent != null && Parent != this && positions.Length > 0)
            {
                yield return Parent.GetClosestPointTo(positions[0]);
            }

            foreach (var position in positions)
                yield return position;
        }
    }

    public IEnumerable<(Vector3 from, Vector3 to)> LineSegments
    {
        get
        {
            var positions = PositionsWithParent.ToArray();

            for (int i = 0; i < positions.Length - 1; ++i)
                yield return (positions[i], positions[i + 1]);
        }
    }



    private (Vector3 closestPoint, float distance) FindClosestPointTo(Vector3 position)
    {
        var minPoint = Positions.FirstOrDefault();
        var minDistance = Vector3.Distance(minPoint, position);

        if (Positions.Length == 1)
            return (Positions[0], Vector3.Distance(Positions[0], position));

        foreach (var (from, to) in LineSegments)
        {
            var lineDirection = to - from;
            var lineLength = lineDirection.magnitude;
            lineDirection /= lineLength;

            var plane = new Plane(lineDirection, position);

            Vector3 closestPoint;
            if (plane.Raycast(new Ray(from, lineDirection), out var collidedAtDistance))
            {
                if (collidedAtDistance < lineLength)
                {
                    closestPoint = from + lineDirection * collidedAtDistance;
                }
                else
                {
                    closestPoint = to;
                }
            }
            else
            {
                closestPoint = from;
            }

            var closestPointDistance = Vector3.Distance(closestPoint, position);
            if (closestPointDistance < minDistance)
            {
                minPoint = closestPoint;
                minDistance = closestPointDistance;
            }
        }

        return (minPoint, minDistance);
    }

    private void OnDrawGizmosSelected()
    {        
        foreach(var point in Positions)
        {
            Gizmos.DrawSphere(point, 0.1f * Width);
        }

        foreach (var (from, to) in LineSegments)
        {
            Gizmos.DrawLine(from, to);
        }
    }

    public void Update()
    {
        if(renderTextureLineRenderer != null)
        {
            var terrainBounds = terrainGameObject.GetComponent<MeshFilter>().sharedMesh.bounds;
            var screenSize = renderTextureLineRenderer.GetComponent<RectTransform>();

            renderTextureLineRenderer.startColor = pathColor;
            renderTextureLineRenderer.endColor = pathColor;

            var width = screenSize.TransformVector(new Vector3(Width / terrainBounds.extents.x * 0.5f * screenSize.rect.width, 0, 0)).x;

            renderTextureLineRenderer.startWidth = width;
            renderTextureLineRenderer.endWidth = width;

            var linePositions = PositionsWithParent;
            if(linePositions.Count() == 1)
            {
                linePositions = linePositions.Concat(linePositions);
            }

            List<Vector3> positions = new List<Vector3>();
            foreach (var linePosition in linePositions)
            {
                var relativePosition = linePosition - terrainGameObject.transform.position;
                positions.Add(new Vector3(-relativePosition.x / terrainBounds.extents.x * screenSize.rect.width * 0.5f,
                    relativePosition.z / terrainBounds.extents.z * screenSize.rect.height * 0.5f,
                    0));
            }

            renderTextureLineRenderer.positionCount = positions.Count;
            renderTextureLineRenderer.SetPositions(positions.ToArray());
        }

    }
    public Vector3 GetClosestPointTo(Vector3 position)
    {
        return FindClosestPointTo(position).closestPoint;
    }


    public bool EstimatePenetrationDepth(Collider collider, out float penetrationDepth)
    {
        var capsuleCollider = GetComponent<CapsuleCollider>();
        if(capsuleCollider == null)
        {
            capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
            capsuleCollider.direction = 0;
        }
        capsuleCollider.radius = Width / 2;

        penetrationDepth = 0.0f;
        foreach (var lineSegment in LineSegments)
        {
            var direction = lineSegment.to - lineSegment.from;
            var distance = direction.magnitude;
            direction /= distance;
            capsuleCollider.height = distance + capsuleCollider.radius + capsuleCollider.radius;

            if (Physics.ComputePenetration(collider,
                 collider.transform.position,
                 collider.transform.rotation,
                 capsuleCollider,
                 lineSegment.from,
                 distance < 0.01f ? Quaternion.identity : Quaternion.FromToRotation(Vector3.right, direction),
                 out var lineSegmentPenetrationDirection,
                 out var lineSegmentPenetrationDepth) && lineSegmentPenetrationDepth > penetrationDepth)
            {
                penetrationDepth = lineSegmentPenetrationDepth;
            }
        }

        return penetrationDepth > 0;
    }
    
    public IEnumerable<double> GetAngles()
    {
        var positions = PositionsWithParent.ToArray();
        for (int i = 1; i < positions.Length - 1; ++i)
        {
            var point = new Vector2(positions[i].x, positions[i].z);
            var nextPoint = new Vector2(positions[i + 1].x, positions[i + 1].z);
            var previousPoint = new Vector2(positions[i - 1].x, positions[i - 1].z);

            if(point == nextPoint || point == previousPoint)
            {
                yield return 0.0;
            }
            else
            {
                // Law of Cosines
                var result = Math.PI - Math.Acos(
                    Math.Min(1.0, Math.Max(-1.0, 
                    ((point - nextPoint).sqrMagnitude + (point - previousPoint).sqrMagnitude - (previousPoint - nextPoint).sqrMagnitude)
                    /
                    (2f * (point - nextPoint).magnitude * (point - previousPoint).magnitude))));

                yield return result;
            }
        }

    }

    public double GetLength()
    {
        return LineSegments.Any() ? LineSegments.Sum(line => Vector3.Distance(line.from, line.to)) : 0;
    }
}
