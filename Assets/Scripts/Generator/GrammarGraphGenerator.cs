using GraphGrammar;
using UnityEngine;

public class GrammarGraphGenerator : MonoBehaviour
{
    public string GrammarName = "VillageGraphGrammar";

    public GraphGrammar<string, RelationshipType> Grammar
    {
        get
        {
            return (GraphGrammar<string, RelationshipType>)System.Type.GetType("GraphGrammar.Examples." + GrammarName)
                           .GetConstructor(new System.Type[] { })
                           .Invoke(new object[] { });
        }
    }

    [SerializeField, Range(0, 25)]
    public int DerivationSteps = 12;

    public void Generate()
    {
        var graphScene = GetComponent<GraphScene>();

        graphScene.GraphInstance = new SceneRelationshipGraph(Grammar.DeriveManySteps(new System.Random(), DerivationSteps,
            Grammar.StartSymbols[UnityEngine.Random.Range(0, Grammar.StartSymbols.Length)],
            SceneRelationshipGraph.WildcardNodeType,
            SceneRelationshipGraph.GetHierachicalNodeGroups));
    }
}
