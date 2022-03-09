using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphGrammar
{
    public class GraphMorphism<NodeLabelType, EdgeLabelType> : IDotFormatExportable
    {
        public Graph<NodeLabelType, EdgeLabelType> FromGraph { get; private set; }
        public Graph<NodeLabelType, EdgeLabelType> ToGraph { get; private set; }
        public uint[] NodeMap { get; private set; }
        public uint[] EdgeMap { get; private set; }

        public GraphMorphism(Graph<NodeLabelType, EdgeLabelType> fromGraph,
            Graph<NodeLabelType, EdgeLabelType> toGraph,
            uint[] nodeMap, uint[] edgeMap)
        {
            FromGraph = fromGraph;
            ToGraph = toGraph;
            NodeMap = nodeMap;
            EdgeMap = edgeMap;
        }

        private static GraphMorphism<NodeLabelType, EdgeLabelType> FromNodeMap(Graph<NodeLabelType, EdgeLabelType> fromGraph,
            Graph<NodeLabelType, EdgeLabelType> toGraph,
            uint[] nodeMap)
        {
            var edgeMap = new List<uint>();

            foreach(var (a, edgeType, b) in fromGraph.Edges)
            {
                var index = Array.IndexOf(toGraph.Edges, (nodeMap[a], edgeType, nodeMap[b]));
                if(index >= 0)
                {
                    edgeMap.Add((uint)index);
                }
                else
                {
                    return null;
                }
           
            }

            return new GraphMorphism<NodeLabelType, EdgeLabelType>(fromGraph, toGraph, nodeMap, edgeMap.ToArray());
        }

        public string ToDotFormat()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("digraph {");
            stringBuilder.Append(FromGraph.ToDotFormatInner("a", "ar"));
            stringBuilder.Append(ToGraph.ToDotFormatInner("b", "br"));

            var index = 0;
            foreach (var node in NodeMap)
            {
                stringBuilder.AppendLine("a" + index + " -> b" + node + " [color=darkgreen; style=dotted];");
                ++index;
            }
            index = 0;
            foreach (var edge in EdgeMap)
            {
                stringBuilder.AppendLine("ar" + index + " -> br" + edge + " [color=darkseagreen; style=dotted];");
                ++index;
            }

            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }


        private static bool IncrementIterationCounters(Dictionary<NodeLabelType, uint[]> nodesGroupedByType,
            ref (NodeLabelType nodeType, uint index)[] iterationCounters)
        {
            // Increment leftmost possible index, abort if index invalid
            for (int i = iterationCounters.Length - 1; i >= 0; --i)
            {
                if (iterationCounters[i].index < nodesGroupedByType[iterationCounters[i].nodeType].Length - 1)
                {
                    iterationCounters[i].index += 1;

                    return true;
                } else
                {
                    iterationCounters[i].index = 0;
                }
            }

            return false;
        }

        public static IEnumerable<GraphMorphism<NodeLabelType, EdgeLabelType>> FindMorphisms(Graph<NodeLabelType, EdgeLabelType> fromGraph,
            Graph<NodeLabelType, EdgeLabelType> toGraph, NodeLabelType wildcardNodeType, Func<NodeLabelType, NodeLabelType[]> getGroupingHierachy)
        {
            var fromGraphGroupedNodes = fromGraph.GetNodesGroupedByType(getGroupingHierachy);
            var toGraphGroupedNodes = toGraph.GetNodesGroupedByType(getGroupingHierachy);

            // Add all Wildcard nodes of the toGraph to every node group and all to the wildcard group.
            // This is done to make sure toGraphGroupedNodes contains in each group all possible targets
            // a node of this group from fromGraph can be mapped to.
            var toGraphWildcardNodes = new uint[] { };
            var toGraphAllNodeIndices = toGraph.Nodes.Select((n, i) => (uint)i).ToArray();
            if (toGraphGroupedNodes.TryGetValue(wildcardNodeType, out toGraphWildcardNodes))
            {
                var keysMissingInToGraph = fromGraphGroupedNodes.Keys.Except(toGraphGroupedNodes.Keys);

                toGraphGroupedNodes = toGraphGroupedNodes
                    .Concat(keysMissingInToGraph.Select(k => new KeyValuePair<NodeLabelType, uint[]>(k, new uint[] { })))
                    .ToDictionary(k => k.Key,k => k.Key.Equals(wildcardNodeType) ? toGraphAllNodeIndices :
                        k.Value.Concat(toGraphWildcardNodes).ToArray());
            }
            else
            {
                toGraphGroupedNodes[wildcardNodeType] = toGraphAllNodeIndices;
                toGraphWildcardNodes = new uint[] { };
            }


            // Check now whether the toGraph contains enough nodes for a morphism from the fromGraph.
            // This means for each node type count of the fromGraph, the toGraph needs to contain at least the same node count
            foreach (var keyValue in fromGraphGroupedNodes)
            {
                if(!keyValue.Key.Equals(wildcardNodeType))
                {
                    var toGraphPossibleMatchCount = toGraphGroupedNodes.TryGetValue(keyValue.Key, out var nodeIndices) ? nodeIndices.Length : toGraphWildcardNodes.Length;

                    if (toGraphPossibleMatchCount < keyValue.Value.Length)
                    {
                        yield break;
                    }
                }
                else
                {
                    int availableNodesForWildcards = toGraphGroupedNodes
                        .Sum(k => k.Value.Length - ((k.Key.Equals(wildcardNodeType) && fromGraphGroupedNodes.TryGetValue(k.Key, out var nodes)) ? nodes.Length : 0));
                        
                    if (availableNodesForWildcards < keyValue.Value.Length)
                    {
                        yield break;
                    }
                }
            }

            // Now iterate possible permutations to find a possible morphism.
            var fromGraphToToGraphGroupedNodesMap = fromGraph.Nodes.Select(n => (nodeType: n, index: (uint)0)).ToArray();

            do
            {
                var fromGraphToToGraphMap = fromGraphToToGraphGroupedNodesMap.
                    Select(t => toGraphGroupedNodes[t.nodeType][t.index])
                    .ToArray();

                // We might end up with mutliple nodes that get mapped to the same node. To avoid this, we skip all instances where this happens.
                var nodeMapHasDuplicates =
                    fromGraphToToGraphMap.Count() != new HashSet<uint>(fromGraphToToGraphMap).Count;

                if (!nodeMapHasDuplicates)
                {
                    var graphMorphism = FromNodeMap(fromGraph, toGraph, fromGraphToToGraphMap);
                    if(graphMorphism != null)
                    {
                        yield return graphMorphism;
                    }
                }
            }
            while (IncrementIterationCounters(toGraphGroupedNodes, ref fromGraphToToGraphGroupedNodesMap));
        }
    }
}
