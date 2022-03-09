using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace GraphGrammar
{
    public class Graph<NodeLabelType, EdgeLabelType> : IDotFormatExportable
    {
        public NodeLabelType[] Nodes { get; private set; }
        public (uint, EdgeLabelType, uint)[] Edges { get; private set; }

        public Graph(NodeLabelType[] nodeLabels, params (uint, EdgeLabelType, uint)[] edges)
        {
            Nodes = nodeLabels.ToArray();
            Edges = edges.ToArray();
        }

        public void ValidateGraphStructure()
        {
            for (int i = 0; i < Edges.Length; ++i)
            {
                var (a, relatesTo, b) = Edges[i];
                if (a >= Nodes.Length)
                {
                    throw new ArgumentOutOfRangeException($"Index {a} of Edge ({a}, {relatesTo}, {b}) out of range for nodes: {string.Join(", ", Nodes)}");
                }
                else if (b >= Nodes.Length)
                {
                    throw new ArgumentOutOfRangeException($"Index {b} of Edge ({a}, {relatesTo}, {b}) out of range for nodes: {string.Join(", ", Nodes)}");
                }
                else if (Edges.Skip(i + 1).Contains(Edges[i]))
                {
                    throw new ArgumentException($"Duplicate edges detected: ({a}, {relatesTo}, {b})");
                }
            }
        }

        public virtual string ToDotFormat()
        {
            return "digraph {" + ToDotFormatInner() + "}";
        }

        public string ToDotFormatInner(string nodeNamePrefix = "", string egdeNamePrefix = "r", (int from,int count)? nodeRange = null, (int from, int count)?egdeRange = null)
        {
            var stringBuilder = new StringBuilder();

            nodeRange = nodeRange ?? (0, Nodes.Length);
            egdeRange = egdeRange ?? (0, Edges.Length);
            var index = nodeRange.Value.from;

            foreach (var node in Nodes.Skip(nodeRange.Value.from).Take(nodeRange.Value.count))
            {
                stringBuilder.AppendLine(nodeNamePrefix + index + " [label=\"" + node.ToString() + "\"]");
                ++index;
            }
            index = egdeRange.Value.from;
            foreach (var (a, r, b) in Edges.Skip(egdeRange.Value.from).Take(egdeRange.Value.count))
            {
                stringBuilder.AppendLine(egdeNamePrefix + index + " [shape=box; color=gray; label=\"" + r.ToString() + "\"]");
                stringBuilder.AppendLine(nodeNamePrefix + a + " -> " + egdeNamePrefix + index + "[dir=none]");
                stringBuilder.AppendLine(egdeNamePrefix + index + " -> " + nodeNamePrefix + b + "");
                ++index;
            }
            return stringBuilder.ToString();
        }

        public Dictionary<NodeLabelType, uint[]> GetNodesGroupedByType(Func<NodeLabelType, NodeLabelType[]> getGroupingHierachy = null)
        {
            getGroupingHierachy = getGroupingHierachy ?? (node => new NodeLabelType[] { node });

            return Nodes.SelectMany((node, index) => getGroupingHierachy(node).Select(group => (index, group)))
                .GroupBy(item => item.group)
                .ToDictionary(k => k.Key, v => v.Select(g => (uint)g.index).ToArray());
        }

        public Graph<NodeLabelType, EdgeLabelType> GraphByRemovingEdges(params uint[] edgeIndices)
        {
            return new Graph<NodeLabelType, EdgeLabelType>(Nodes, Edges.Where((_, i) => !edgeIndices.Contains((uint)i)).ToArray());
        }

        public Graph<NodeLabelType, EdgeLabelType> GraphByRemovingNodes(params uint[] nodeIndices)
        {
            var newNodes = Nodes.ToList();
            var newEdges = Edges.ToArray();

            nodeIndices = nodeIndices.ToArray();

            int i = 0;
            while(i < Nodes.Length)
            {
                if(nodeIndices.Contains((uint)i))
                {
                    newNodes.RemoveAt(i);

                    for (int j = 0; j < nodeIndices.Length; ++j)
                        nodeIndices[j] -= 1;
                    for (int j = 0; j < newEdges.Length; ++j)
                    {
                        if (newEdges[j].Item1 == i || newEdges[j].Item3 == i)
                            throw new InvalidOperationException("Violated the contact condition");

                        if (newEdges[j].Item1 > i) --newEdges[j].Item1;
                        if (newEdges[j].Item3 > i) --newEdges[j].Item3;
                    }
                } else
                {
                    ++i;
                }
            }

            return new Graph<NodeLabelType, EdgeLabelType>(newNodes.ToArray(), newEdges);
        }

        public Graph<NodeLabelType, EdgeLabelType> GraphByRemovingNodesAndConnectingEdges(params uint[] nodeIndices)
        {
            var newEdges = Edges.Where((a) => !nodeIndices.Contains(a.Item1) && !nodeIndices.Contains(a.Item3)).ToArray();

            return new Graph<NodeLabelType, EdgeLabelType>(this.Nodes, newEdges).GraphByRemovingNodes(nodeIndices);
        }


        public Graph<NodeLabelType, EdgeLabelType> GraphByAddingNodes(params NodeLabelType[] nodes)
        {
            return new Graph<NodeLabelType, EdgeLabelType>(Nodes.Concat(nodes).ToArray(), Edges);
        }

        public Graph<NodeLabelType, EdgeLabelType> GraphByAddingEdges(params (uint, EdgeLabelType, uint)[] edges)
        {
            return new Graph<NodeLabelType, EdgeLabelType>(Nodes, Edges.Concat(edges).ToArray());
        }
    }
}