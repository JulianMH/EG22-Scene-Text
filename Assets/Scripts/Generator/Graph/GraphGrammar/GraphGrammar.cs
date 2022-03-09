using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace GraphGrammar
{
    public class GraphGrammar<NodeLabelType, EdgeLabelType> : IDotFormatExportable
    {
        public Graph<NodeLabelType, EdgeLabelType>[] StartSymbols { get; set; }

        public GraphGrammarRule<NodeLabelType, EdgeLabelType>[] Rules { get; private set; }

        public GraphGrammar(Graph<NodeLabelType, EdgeLabelType>[] startSymbols, params GraphGrammarRule<NodeLabelType, EdgeLabelType>[] rules)
        {
            StartSymbols = startSymbols;
            Rules = rules;
        }

        public string ToDotFormat()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("digraph {");

            int index = 0;
            foreach (var rule in Rules)
            {
                stringBuilder.AppendLine("subgraph clusterRule" + index + " {");
                stringBuilder.Append(rule.ToDotFormatInner("Rule" + index));
                stringBuilder.Append("}");
                ++index;
            }

            stringBuilder.AppendLine("}");
            return stringBuilder.ToString();
        }

        private static IEnumerable<GraphMorphism<NodeLabelType, EdgeLabelType>> FindMorphismsForRule(GraphGrammarRule<NodeLabelType, EdgeLabelType> rule,
            Graph<NodeLabelType, EdgeLabelType> graph, NodeLabelType wildcardNodeType, Func<NodeLabelType, NodeLabelType[]> getGroupingHierachy)
        {
            var morphisms = GraphMorphism<NodeLabelType, EdgeLabelType>.FindMorphisms(rule.OriginalGraphFragment, graph, wildcardNodeType, getGroupingHierachy);
            if (rule.FirstNodeConnectionMaximum != null)
                return morphisms.Where(m => graph.Edges.Count(e => e.Item3 == m.NodeMap[0]) < rule.FirstNodeConnectionMaximum.Value);
            else
                return morphisms;
        }

        public Graph<NodeLabelType, EdgeLabelType> DeriveOneStep(Random random, Graph<NodeLabelType, EdgeLabelType> graph, NodeLabelType wildcardNodeType, Func<NodeLabelType, NodeLabelType[]> getGroupingHierachy)
        {
            var possibleRuleApplications = Rules
                .Select(rule => (rule, morphisms: FindMorphismsForRule(rule, graph, wildcardNodeType, getGroupingHierachy)))
                .Where(t => t.morphisms.Any());

            var summedPropabilityValues = possibleRuleApplications.Sum(t => t.rule.Propability);
            var p = random.NextDouble() * summedPropabilityValues;

            foreach (var (rule, morphisms) in possibleRuleApplications)
            {
                p -= rule.Propability;
                if (p < 0)
                {
                    var morphismArray = morphisms.ToArray();
                    return rule.ApplyTo(morphismArray[random.Next(morphismArray.Length)]);
                }
            }
            return null;
        }

        public Graph<NodeLabelType, EdgeLabelType> DeriveManySteps(Random random, int maxSteps, Graph<NodeLabelType, EdgeLabelType> graph, NodeLabelType wildcardNodeType, Func<NodeLabelType, NodeLabelType[]> getGroupingHierachy)
        {
            for(int i = 1; i <= maxSteps; ++i)
            {
                var newGraph = DeriveOneStep(random, graph, wildcardNodeType, getGroupingHierachy);
                if (newGraph == null)
                    return graph;
                graph = newGraph;
            }
            return graph;
        }
    }
}
