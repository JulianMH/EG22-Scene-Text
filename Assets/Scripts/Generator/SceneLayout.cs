using System.Linq;
using UnityEngine;
using System;

[Serializable]
public class SceneLayout
{
    [Serializable]
    public struct ObjectInfo
    {
        public string NodeType;
        public int CorrespondsToNodeIndex;
        public int ParentIndex;
        public Vector3 Position;
        public float RotationY;

        public ObjectInfo(string nodeType, int correspondsToNodeIndex, int parentIndex, Vector3 position, float rotationY)
        {
            NodeType = nodeType;
            ParentIndex = parentIndex;
            CorrespondsToNodeIndex = correspondsToNodeIndex;
            Position = position;
            RotationY = rotationY;
        }
    }

    [Serializable]
    public struct PathInfo
    {
        public Vector3[] NodePositions;
        public int ParentIndex;

        public PathInfo(Vector3[] nodePositions, int parentIndex)
        {
            NodePositions = nodePositions;
            ParentIndex = parentIndex;
        }
    }

    public ObjectInfo[] Objects;
    public PathInfo[] Paths;

    private SceneLayout(
        ObjectInfo[] objects,
        PathInfo[] paths)
    {
        Objects = objects;
        Paths = paths;
    }

    public static SceneLayout GetCurrentLayout(GraphNodeBehaviour[] behaviours, Path[] paths)
    {
        return new SceneLayout(
            behaviours.Select(p => new ObjectInfo(p.NodeType,
                p.CorrespondsToNodeIndex,
                Array.IndexOf(behaviours, p.transform.parent.GetComponent<GraphNodeBehaviour>()),
                p.transform.localPosition,
                p.transform.localEulerAngles.y)).ToArray(),
            paths.Select(p => new PathInfo(p.LocalPositions.ToArray(), Array.IndexOf(paths, p.Parent))).ToArray());
    }

    public void Apply(GraphNodeBehaviour[] behaviours, Path[] paths)
    {
        for(var i = 0; i < behaviours.Length; ++i)
        {
            var behaviour = behaviours[i];
            var objectInfo = Objects[i];
            if (behaviour.NodeType != objectInfo.NodeType || behaviour.CorrespondsToNodeIndex != objectInfo.CorrespondsToNodeIndex)
                throw new InvalidOperationException("Behavouirs Array does not fit to this scene Layout");

            behaviour.gameObject.transform.localPosition = objectInfo.Position;
            behaviour.gameObject.transform.localRotation = Quaternion.AngleAxis(objectInfo.RotationY, Vector3.up);
        }

        for (var i = 0; i < paths.Length; ++i)
        {
            var path = paths[i];
            var pathInfo = Paths[i];

            path.LocalPositions = pathInfo.NodePositions.ToArray();
            path.Parent = pathInfo.ParentIndex >= 0 ? paths[pathInfo.ParentIndex] : null;
        }

        Physics.SyncTransforms();
    }

    public bool CouldBeParentPathIndex(int parentPathIndex, int childPathIndex)
    {
        if (parentPathIndex == childPathIndex)
        {
            return false;
        }
        else
        {
            var parentParentPathIndex = Paths[parentPathIndex].ParentIndex;
            if (parentParentPathIndex < 0)
            {
                return true;
            }
            else
            {
                return CouldBeParentPathIndex(parentParentPathIndex, childPathIndex);
            }
        }
    }

    public SceneLayout Copy()
    {
        return new SceneLayout(
            Objects.ToArray(),
            Paths.Select(p => new PathInfo(p.NodePositions.ToArray(), p.ParentIndex)).ToArray());
    }

    public SceneLayout(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }
}

