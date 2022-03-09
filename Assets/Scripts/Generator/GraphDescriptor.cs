using System.Linq;
using UnityEngine;

public class GraphDescriptor : MonoBehaviour
{
    public bool EnableAggregateTextStructure = true;
    public float EnableDelayedIntroductionForSingleObjectsWithImportanceLessThan = 1.1f;
    public bool EnableConsistsOfSentencesIntegratingRelationships = true;
    public bool EnableGenerateReferringExpressionsEntities = true;
    public bool EnableGenerateReferringExpressionsSentences = true;
    public bool EnableCentralElementInversions = true;
    
    private GraphNLG graphNLG = new GraphNLG();

    public string TextualDescription = "";

    public void RegenerateTexualDescription()
    {
        var graphScene = GetComponent<GraphScene>();
        try
        {
            var behaviours = graphScene.GetAllGraphNodeBehavioursRecursively();

            var nodeInfos = behaviours.Select(p => p.GetNodeInfoProvider());
            graphNLG.EnableAggregateTextStructure = EnableAggregateTextStructure;
            graphNLG.EnableDelayedIntroductionForSingleObjectsWithImportanceLessThan = EnableDelayedIntroductionForSingleObjectsWithImportanceLessThan;
            graphNLG.EnableConsistsOfSentencesIntegratingRelationships = EnableConsistsOfSentencesIntegratingRelationships;
            graphNLG.EnableGenerateReferringExpressionsEntities = EnableGenerateReferringExpressionsEntities;
            graphNLG.EnableGenerateReferringExpressionsSentences = EnableGenerateReferringExpressionsSentences;
            graphNLG.EnableCentralElementInversions = EnableCentralElementInversions;
            TextualDescription = graphNLG.Describe(graphScene.GraphInstance, nodeInfos.ToArray());
        }
        catch
        {
            TextualDescription = "Graph Scene not instanciated.";
        }
    }
}
