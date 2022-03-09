using System;
using System.Linq;
using System.Text;

namespace GraphGrammar
{
    public class GraphGrammarRule<NodeLabelType, EdgeLabelType> : IDotFormatExportable
    {
        public string Name { get; private set; }
        public double Propability { get; private set; }

        public Graph<NodeLabelType, EdgeLabelType> OriginalGraphFragment { get; private set; }
        public uint SharedNodeCount { get; private set; }
        public uint SharedEdgeCount { get; private set; }
        public Graph<NodeLabelType, EdgeLabelType> ReplacementGraphFragment { get; private set; }

        public uint? FirstNodeConnectionMaximum { get; private set; }

        public GraphGrammarRule(
            string name,
            double propability,
            Graph<NodeLabelType, EdgeLabelType> originalGraphFragment,
            uint sharedNodeCount,
            uint sharedEdgeCount,
            Graph<NodeLabelType, EdgeLabelType> replacementGraphFragment,
            uint? firstNodeConnectionMaximum = null)
        {
            Name = name;
            Propability = propability;

            OriginalGraphFragment = originalGraphFragment;
            SharedNodeCount = sharedNodeCount;
            SharedEdgeCount = sharedEdgeCount;
            ReplacementGraphFragment = replacementGraphFragment;

            FirstNodeConnectionMaximum = firstNodeConnectionMaximum;


            CheckValidRule();
        }

        private void CheckValidRule()
        {
            OriginalGraphFragment.ValidateGraphStructure();
            ReplacementGraphFragment.ValidateGraphStructure();

            if (OriginalGraphFragment.Nodes.Length < SharedNodeCount || OriginalGraphFragment.Edges.Length < SharedEdgeCount)
                throw new IndexOutOfRangeException(Name + ":" + nameof(OriginalGraphFragment) + " has not enough nodes and edges for the shared subgraph");
            if (ReplacementGraphFragment.Nodes.Length < SharedNodeCount || ReplacementGraphFragment.Edges.Length < SharedEdgeCount)
                throw new IndexOutOfRangeException(Name + ":" + nameof(ReplacementGraphFragment) + " has not enough nodes and edges for the shared subgraph");


            if (!Enumerable.SequenceEqual(OriginalGraphFragment.Nodes.Take((int)SharedNodeCount),
                ReplacementGraphFragment.Nodes.Take((int)SharedNodeCount)))
            {
                throw new InvalidOperationException(Name + ":" + "Shared nodes of " + nameof(OriginalGraphFragment) + " and "
                    + nameof(ReplacementGraphFragment) + " are not identical for rule " + Name);
            }

            if (!Enumerable.SequenceEqual(OriginalGraphFragment.Edges.Take((int)SharedEdgeCount),
                ReplacementGraphFragment.Edges.Take((int)SharedEdgeCount)))
            {

                throw new InvalidOperationException(Name + ":" + "Shared eges of " + nameof(OriginalGraphFragment) + " and "
                    + nameof(ReplacementGraphFragment) + " are not identical for rule " + Name);
            }
        }

        public string ToDotFormatInner(string rulePrefix = "")
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("subgraph cluster" + rulePrefix + "Right {");
            stringBuilder.AppendLine("subgraph cluster" + rulePrefix + "RightShared {");
            stringBuilder.Append(ReplacementGraphFragment.ToDotFormatInner(rulePrefix + "r", rulePrefix + "rr", (0, (int)SharedNodeCount), (0, (int)SharedEdgeCount)));
            stringBuilder.AppendLine("style=filled");
            stringBuilder.AppendLine("color=\"#dedede\"");
            stringBuilder.AppendLine("}");
            stringBuilder.Append(ReplacementGraphFragment.ToDotFormatInner(rulePrefix + "r", rulePrefix + "rr", ((int)SharedNodeCount, int.MaxValue), ((int)SharedEdgeCount, int.MaxValue)));
            stringBuilder.AppendLine("label = \"Rule produces\"");
            stringBuilder.AppendLine("style=dashed");
            stringBuilder.AppendLine("}");

            stringBuilder.AppendLine("subgraph cluster" + rulePrefix + "Left {");
            stringBuilder.AppendLine("subgraph cluster" + rulePrefix + "LeftShared {");
            stringBuilder.Append(OriginalGraphFragment.ToDotFormatInner(rulePrefix + "l", rulePrefix + "lr", (0, (int)SharedNodeCount), (0, (int)SharedEdgeCount)));
            stringBuilder.AppendLine("style=filled");
            stringBuilder.AppendLine("color=\"#dedede\"");
            stringBuilder.AppendLine("}");
            stringBuilder.Append(OriginalGraphFragment.ToDotFormatInner(rulePrefix + "l", rulePrefix + "lr", ((int)SharedNodeCount, int.MaxValue), ((int)SharedEdgeCount, int.MaxValue)));
            stringBuilder.AppendLine("label = \"Rule matches\"");
            stringBuilder.AppendLine("style=dashed");
            stringBuilder.AppendLine("}");
            stringBuilder.AppendLine("label = \"" + Name + " (l = " + Propability.ToString("0.00") + ")\"");

            return stringBuilder.ToString();
        }

        public string ToDotFormat()
        {
            return "digraph {" + ToDotFormatInner() + "}";
        }

        public Graph<NodeLabelType, EdgeLabelType> ApplyTo(Graph<NodeLabelType, EdgeLabelType> graph, NodeLabelType wildcardNodeType, Func<NodeLabelType, NodeLabelType[]> getGroupingHierachy)
        {
            var chosenMorphism = GraphMorphism<NodeLabelType, EdgeLabelType>.FindMorphisms(OriginalGraphFragment, graph, wildcardNodeType, getGroupingHierachy).First();

            return ApplyTo(chosenMorphism);
        }

        public Graph<NodeLabelType, EdgeLabelType> ApplyTo(GraphMorphism<NodeLabelType, EdgeLabelType> chosenMorphism)
        {
            // Step 1: Find a spot to apply the rule
            // Already done since we provide a GraphMorphism as parameter to the rule

            // Check Contact condition: Will happen automatically when calling GraphByRemovingNodes later.
            // Should never fail as long as I do define rules correctly.

            // Step 2: Remove all nodes from the graph that match the left side, but are not part of the shared subgraph.
            var nodesToRemove = OriginalGraphFragment.Nodes.Skip((int)SharedNodeCount)
                .Select((_, i) => chosenMorphism.NodeMap[SharedNodeCount + i]).ToArray();
            var edgesToRemove = OriginalGraphFragment.Edges.Skip((int)SharedEdgeCount)
                .Select((_, i) => chosenMorphism.EdgeMap[SharedEdgeCount + i]).ToArray();

            var newNodeMap = chosenMorphism.NodeMap.Select(i => i - (uint)nodesToRemove.Count(j => j <= i)).ToArray();
            var newEdgeMap = chosenMorphism.EdgeMap.Select(i => i - (uint)edgesToRemove.Count(j => j <= i)).ToArray();

            Graph<NodeLabelType, EdgeLabelType> intermediateGraph;
            try
            {
                intermediateGraph = chosenMorphism.ToGraph.GraphByRemovingEdges(edgesToRemove).GraphByRemovingNodes(nodesToRemove);
            }
            catch(InvalidOperationException)
            {
                throw new InvalidOperationException(Name + ": Violated Contact Condition.");
            }
            var intermediateMorphism = new GraphMorphism<NodeLabelType, EdgeLabelType>(OriginalGraphFragment, intermediateGraph, newNodeMap, newEdgeMap);

            // Step 3: Add all nodes and edges that belong to the right side, but not the shared subgraph.
            var nodesToAdd = ReplacementGraphFragment.Nodes.Skip((int)SharedNodeCount).ToArray();
            var edgesToAdd = ReplacementGraphFragment.Edges.Skip((int)SharedEdgeCount)
                .Select(e =>
                {
                    var (a, edgeType, b) = e;

                    // If the ege points into or from the shared subgraph, we can use the intermediate morphism to
                    // transform it from the shared graph subgraph to the intermediate graph.
                    // If it does not, we need to transform its indices to point to the newly added nodes at the end of the
                    // intermediate Graph.
                    if (a < SharedNodeCount)
                    {
                        a = intermediateMorphism.NodeMap[a];
                    }
                    else
                    {
                        a = (uint)(a - SharedNodeCount + intermediateGraph.Nodes.Length);
                    }

                    if (b < SharedNodeCount)
                    {
                        b = intermediateMorphism.NodeMap[b];
                    }
                    else
                    {
                        b = (uint)(b - SharedNodeCount + intermediateGraph.Nodes.Length);
                    }

                    return (a, edgeType, b);

                }).ToArray();

            return intermediateGraph.GraphByAddingNodes(nodesToAdd).GraphByAddingEdges(edgesToAdd);
        }


        public Graph<NodeLabelType, EdgeLabelType> GetSharedSubgraph()
        {
            return new Graph<NodeLabelType, EdgeLabelType>(OriginalGraphFragment.Nodes.Take((int)SharedNodeCount).ToArray(),
                OriginalGraphFragment.Edges.Take((int)SharedEdgeCount).ToArray());
        }
    }
}
