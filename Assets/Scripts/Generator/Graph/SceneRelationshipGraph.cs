using System;
using System.Linq;
using System.Text;
using GraphGrammar;

[Serializable]
public class SceneRelationshipGraph : Graph<string, RelationshipType>
{
    public SceneRelationshipGraph(string[] nodes, params (uint, RelationshipType, uint)[] edges) : base(nodes, edges)
    {
    }

    public SceneRelationshipGraph(string rootNode) : base(new string[] { rootNode }, new (uint, RelationshipType, uint)[] { })
    {
    }

    public SceneRelationshipGraph(Graph<string, RelationshipType> graph) : base(graph.Nodes, graph.Edges)
    {

    }

    public const string WildcardNodeType = "";

    public static string[] GetHierachicalNodeGroups(string nodeType)
    {
        if(nodeType.Contains('/'))
        {
            var split = nodeType.Split('/');
            return new string[] { nodeType, split[0], };
        }
        else
        {
            return new string[] { nodeType };
        }

    }

    public uint?[] GetNodeHierachyClustering()
    {
        var nodeParents = Nodes.Select(_ => (uint?)null).ToArray();

        var changedThisStep = true;
        while (changedThisStep)
        {
            changedThisStep = false;
            foreach (var (a, relatesTo, b) in Edges)
            {
                if (relatesTo.IsTypePartOf() && (nodeParents[a] == null || nodeParents[a] != b))
                {
                    if (nodeParents[a] != null)
                        throw new Exception($"Graph is not clusterable, because node {a} would belong to multiple clusters ({nodeParents[a]} and {b})");

                    nodeParents[a] = b;
                    changedThisStep = true;
                }
                else if ((relatesTo.IsTypePartOfOrOnTopOf()) && nodeParents[a] != nodeParents[b] && nodeParents[b] != null)
                {
                    if (nodeParents[a] != null)
                        throw new Exception($"Graph is not clusterable, because node {a} would belong to multiple clusters ({nodeParents[a]} and {nodeParents[b]})");

                    nodeParents[a] = nodeParents[b];
                    changedThisStep = true;
                }
            }
        }

        return nodeParents;
    }

    public override string ToDotFormat()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("digraph {");
        stringBuilder.AppendLine(ToDotFormatInner());

        var clustering = GetNodeHierachyClustering();
        var clusterParents = clustering.Where(c => c != null).Distinct().ToArray();

        foreach (var clusterParent in clusterParents)
        {
            stringBuilder.AppendLine("subgraph cluster" + clusterParent + " {");

            for (int j = 0; j < Nodes.Length; ++j)
            {
                if (clustering[j] == clusterParent || clusterParent == j)
                    stringBuilder.AppendLine(j.ToString());
            }
            for (int j = 0; j < Edges.Length; ++j)
            {
                var (a, relatesTo, b) = Edges[j];

                if ((clustering[a] == clusterParent || a == clusterParent) && (clustering[b] == clusterParent || b == clusterParent))
                    stringBuilder.AppendLine("r" + j.ToString());

            }
            stringBuilder.AppendLine("style=dashed");
            stringBuilder.AppendLine("color=\"#565656\"");
            stringBuilder.AppendLine("}");
        }

        stringBuilder.AppendLine("}");

        return stringBuilder.ToString();
    }

}