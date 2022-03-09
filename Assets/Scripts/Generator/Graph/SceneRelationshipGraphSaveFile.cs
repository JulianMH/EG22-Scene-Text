using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class SceneRelationshipGraphSaveFile
{
    [Serializable]
    public class EdgeSaveFile
    {
        public uint from;
        public string type;
        public uint to;

        public EdgeSaveFile((uint, RelationshipType, uint) edge)
        {
            var (from, type, to) = edge;
            this.from = from;
            this.type = type.ToString();
            this.to = to;
        }

        public (uint, RelationshipType, uint) ToEdge()
        {
            return (from, (RelationshipType)Enum.Parse(typeof(RelationshipType), type), to);
        }
    }

    public string[] nodes = null;
    public EdgeSaveFile[] edges = null;

    public SceneRelationshipGraphSaveFile(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }

    public SceneRelationshipGraphSaveFile(SceneRelationshipGraph graph)
    {
        this.nodes = graph.Nodes.ToArray();
        this.edges = graph.Edges.Select(e => new EdgeSaveFile(e)).ToArray();
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }

    public SceneRelationshipGraph ToGraph()
    {
        return new SceneRelationshipGraph(nodes, edges.Select(e => e.ToEdge()).ToArray());
    }
}
