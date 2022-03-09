using System;
using System.Collections.Generic;
using System.Linq;
using SimpleNLG;

public class GraphNLG
{
    public bool EnableAggregateTextStructure = true;
    public float EnableDelayedIntroductionForSingleObjectsWithImportanceLessThan = 1.1f;
    public bool EnableConsistsOfSentencesIntegratingRelationships = true;
    public bool EnableGenerateReferringExpressionsEntities = true;
    public bool EnableGenerateReferringExpressionsSentences = true;
    public bool EnableCentralElementInversions = true;

    private NLGFactory factory;
    private Realiser realiser;
    private NPPhraseSpec nothingPlaceholderWord;

    public GraphNLG()
    {
        var lexicon = Lexicon.getDefaultLexicon();
        factory = new NLGFactory(lexicon);
        realiser = new Realiser(lexicon);
        nothingPlaceholderWord = factory.createNounPhrase("nothing");
    }

    private class TextEntity
    {
        public NodeInfoProvider NodeInfo { get; private set; }
        public string CommonReferringExpression { get; set; }
        public TextEntity Parent { get; set; }

        public TextEntity(NodeInfoProvider nodeInfo)
        {
            NodeInfo = nodeInfo;
            CommonReferringExpression = nodeInfo.GetDefaultExpression();
        }
    }

    private class TextEntityReference
    {
        public TextEntity[] Entities { get; private set; }
        public INLGElement ReferringExpression { get; set; }
        public bool[] PrefersIndefiniteForm { get; private set; }

        public TextEntityReference(params TextEntity[] entities)
        {
            Entities = entities;
            PrefersIndefiniteForm = entities.Select(e => false).ToArray();
        }
        public TextEntityReference(TextEntity[] entities, bool[] prefersIndefinteForm)
        {
            if (entities.Length != prefersIndefinteForm.Length)
                throw new ArgumentException();

            Entities = entities;
            PrefersIndefiniteForm = prefersIndefinteForm;
        }

        public override string ToString()
        {
            return string.Join(", ", Entities.Select(p => p.CommonReferringExpression));
        }

        public static bool AreReferencingSameEntities(TextEntityReference a, TextEntityReference b)
        {
            return a.Entities.Length == b.Entities.Length && a.Entities.Except(b.Entities).Count() == 0;
        }

        public bool IsReferencingSameEntitiesAs(TextEntityReference other)
        {
            return AreReferencingSameEntities(this, other);
        }

        public class ReferencesSameComparer : IEqualityComparer<TextEntityReference>
        {
            public bool Equals(TextEntityReference x, TextEntityReference y)
            {
                return x.IsReferencingSameEntitiesAs(y);
            }

            public int GetHashCode(TextEntityReference obj)
            {
                return obj.Entities.Aggregate(0, (a,b) => a.GetHashCode() + b.GetHashCode());
            }
        }
    }

    private class TextContent
    {
        public TextEntity[] Entities { get; private set; }
        public (TextEntity, RelationshipType, TextEntity)[] Relationships { get; private set; }

        public TextContent(TextEntity[] entities, (TextEntity, RelationshipType, TextEntity)[] relationships)
        {
            Entities = entities;
            Relationships = relationships;
        }
    }

    private abstract class SentenceSkeleton
    {
        public abstract IEnumerable<TextEntityReference> TextEntityReferences { get; }
    }

    private class ConsistsOfSentenceSkeleton : SentenceSkeleton
    {
        public TextEntityReference Container { get; private set; }
        public TextEntityReference Content { get; private set; }

        public RelatesToSentenceSkeleton ContainedRelationshipInformation { get; private set; }

        public override IEnumerable<TextEntityReference> TextEntityReferences => new TextEntityReference[] { Container, ContainedRelationshipInformation?.Subject, ContainedRelationshipInformation?.Object, Content }.Where(c => c != null);

        public ConsistsOfSentenceSkeleton(TextEntityReference container, TextEntityReference content, RelatesToSentenceSkeleton containedRelationshipInformation = null)
        {
            this.Container = container;
            this.Content = content;
            this.ContainedRelationshipInformation = containedRelationshipInformation;
        }
    }

    private class RelatesToSentenceSkeleton : SentenceSkeleton
    {
        public TextEntityReference Subject { get; private set; }
        public RelationshipType RelateTo { get; private set; }
        public TextEntityReference Object { get; private set; }

        public bool RealizeInverted { get; set; } = false;

        public override IEnumerable<TextEntityReference> TextEntityReferences => RealizeInverted ? new TextEntityReference[] { Object, Subject } : new TextEntityReference[] { Subject, Object };

        public RelatesToSentenceSkeleton(TextEntityReference subject, RelationshipType relateTo, TextEntityReference obj)
        {
            this.Subject = subject;
            this.RelateTo = relateTo;
            this.Object = obj;
        }
    }

    private class TextStructure
    {
        public TextEntity[] Entities { get; private set; }
        public SentenceSkeleton[] Sentences { get; private set; }

        public TextStructure(TextEntity[] entities, SentenceSkeleton[] sentences)
        {
            this.Entities = entities;
            this.Sentences = sentences;
        }
    }


    private TextContent DetermineTextContent(SceneRelationshipGraph graph, NodeInfoProvider[] nodeInfos)
    {
        var nodeIndicesToDelete = graph.Nodes
            .Select((n, i) => nodeInfos[i].ExpressionHierachy.Length == 0 ? (uint?)i : null)
            .Where(i => i.HasValue)
            .Select(i => i.Value).ToArray();

        var prunedGraph = new SceneRelationshipGraph(graph.GraphByRemovingNodesAndConnectingEdges(nodeIndicesToDelete));

        var edgeIndicesToDelete = prunedGraph.Edges.Select((e, i) => e.Item2.IsImplict() ? (uint?)i : null)
            .Where(i => i.HasValue)
            .Select(i => i.Value).ToArray();

        prunedGraph = new SceneRelationshipGraph(prunedGraph.GraphByRemovingEdges(edgeIndicesToDelete));

        nodeInfos = nodeInfos.Where(n => n.ExpressionHierachy.Length > 0).ToArray();
        var subjects = prunedGraph.Nodes.Select((n, i) => new TextEntity(nodeInfos[i])).ToArray();

        var relationships = prunedGraph.Edges.Select((r) => (subjects[r.Item1], r.Item2, subjects[r.Item3])).ToArray();

        var clustering = prunedGraph.GetNodeHierachyClustering();
        for (int i = 0; i < subjects.Length; ++i)
        {
            subjects[i].Parent = clustering[i] == null ? null : subjects[clustering[i].Value];
        }

        return new TextContent(subjects, relationships);
    }

    private TextStructure StructureTextContent(TextContent textContent)
    {
        var parents = textContent.Entities.Where(e => e.Parent == null).OrderByDescending(p => p.NodeInfo.ObjectImportance).ToArray();

        var remainingRelationships = textContent.Relationships.ToArray();
        var parentRelationships = remainingRelationships.Where(r => r.Item1.Parent == null && r.Item3.Parent == null);

        var scene = new TextEntity(new NodeInfoProvider() { ExpressionHierachy = new string[] { "scene" } });
        var entities = new List<TextEntity>() { scene };
        entities.AddRange(parents);
        var sentences = new List<SentenceSkeleton>();
        sentences.Add(new ConsistsOfSentenceSkeleton(new TextEntityReference(scene), new TextEntityReference(parents) /*{ PrefersIndefiniteForm = true } */));
        sentences.AddRange(parentRelationships.Select(r => new RelatesToSentenceSkeleton(new TextEntityReference(r.Item1), r.Item2, new TextEntityReference(r.Item3))));

        remainingRelationships = remainingRelationships.Except(parentRelationships).ToArray();

        foreach (var parent in parents)
        {
            var children = textContent.Entities.Where(e => e.Parent == parent).ToArray();
            if (children.Any())
                sentences.Add(new ConsistsOfSentenceSkeleton(new TextEntityReference(parent), new TextEntityReference(children) /* { PrefersIndefiniteForm = true } */));
            entities.AddRange(children);

            var parentAndChildren = new TextEntity[] { parent }.Concat(children).ToArray();

            remainingRelationships = remainingRelationships.Where(r => !(r.Item2.IsTypePartOf() && r.Item3 == parent)).ToArray();
            var relevantRelationships = remainingRelationships.Where(r => parentAndChildren.Contains(r.Item1) || parentAndChildren.Contains(r.Item3));
            sentences.AddRange(relevantRelationships.Select(r => new RelatesToSentenceSkeleton(new TextEntityReference(r.Item1), r.Item2, new TextEntityReference(r.Item3))));
            remainingRelationships = remainingRelationships.Except(relevantRelationships).ToArray();
        }
        sentences.AddRange(remainingRelationships.Select(r => new RelatesToSentenceSkeleton(new TextEntityReference(r.Item1), r.Item2, new TextEntityReference(r.Item3))));

        return new TextStructure(entities.ToArray(), sentences.ToArray());
    }

    private TextStructure AggregateTextStructure(TextStructure textStructure)
    {
        var sentences = new List<SentenceSkeleton>();

        foreach (var sentenceSkeleton in textStructure.Sentences)
        {
            switch (sentenceSkeleton)
            {
                case ConsistsOfSentenceSkeleton consistsOfSentence:
                    {
                        sentences.Add(new ConsistsOfSentenceSkeleton(consistsOfSentence.Container, consistsOfSentence.Content));
                    }
                    break;
                case RelatesToSentenceSkeleton relatesToSentenceSkeleton:
                    {
                        var relatesToSentences = sentences.Where(s => s is RelatesToSentenceSkeleton).Cast<RelatesToSentenceSkeleton>().ToArray();

                        var similarSuffixSentence = relatesToSentences.Where(s => s.RelateTo == relatesToSentenceSkeleton.RelateTo &&
                            Enumerable.SequenceEqual(s.Object.Entities, relatesToSentenceSkeleton.Object.Entities)).FirstOrDefault();

                        if(similarSuffixSentence != null)
                        {
                            sentences[sentences.IndexOf(similarSuffixSentence)] = new RelatesToSentenceSkeleton(
                                new TextEntityReference(similarSuffixSentence.Subject.Entities.Concat(relatesToSentenceSkeleton.Subject.Entities).ToArray()),
                                similarSuffixSentence.RelateTo,
                                similarSuffixSentence.Object);
                        }
                        else
                        {
                            var similarPrefixSentence = relatesToSentences.Where(s => s.RelateTo == relatesToSentenceSkeleton.RelateTo &&
                                Enumerable.SequenceEqual(s.Subject.Entities, relatesToSentenceSkeleton.Subject.Entities)).FirstOrDefault();

                            if (similarPrefixSentence != null)
                            {
                                sentences[sentences.IndexOf(similarPrefixSentence)] = new RelatesToSentenceSkeleton(
                                    similarPrefixSentence.Subject,
                                    similarPrefixSentence.RelateTo,
                                    new TextEntityReference(similarPrefixSentence.Object.Entities.Concat(relatesToSentenceSkeleton.Object.Entities).ToArray()));
                            } else
                            {
                                sentences.Add(relatesToSentenceSkeleton);

                            }
                        }
                    }
                    break;
            }
        }

        return new TextStructure(textStructure.Entities, sentences.ToArray());
    }

    private string GenerateReferringExpressionsEntity(TextEntity node, Dictionary<string, TextEntity[]> groupedNodes, Dictionary<string, TextEntity[]> spezializedGroupedNodes)
    {
        var defaultExpression = node.NodeInfo.GetDefaultExpression();
        var spezializedExpression = node.NodeInfo.ExpressionHierachy.Last();

        var similarNodes = groupedNodes[defaultExpression];
        // 1. Use basic expression
        if (similarNodes.Length <= 1)
        {
            return defaultExpression;
        }
        // 2. Use basic expression identified by cluster
        else if (node.Parent != null && !similarNodes.Any(p => p.Parent == node.Parent && p != node))
        {
            return defaultExpression + " of the " + node.Parent.CommonReferringExpression;
        }

        if (defaultExpression != spezializedExpression)
        {
            var spezializedSimilarNodes = spezializedGroupedNodes[spezializedExpression];

            // 3. Use spezialized expression
            if (spezializedSimilarNodes.Length <= 1)
            {
                return spezializedExpression;
            }
            // 4. Use spezialized expression identified by cluster
            else if (node.Parent != null && !spezializedSimilarNodes.Any(p => p.Parent == node.Parent && p != node))
            {
                return spezializedExpression + " of the " + node.Parent.CommonReferringExpression;
            }
        }

        if (node.Parent != null)
        {
            var similarNodesInCluster = similarNodes.Where(p => p.Parent == node.Parent).ToArray();
            var index = Array.IndexOf(similarNodesInCluster, node) + 1;

            // 5. Use numbered expression (numbering relative to cluster)
            if (similarNodes.All(n => n.Parent == node.Parent))
            {
                return GetDisplayStringForObjectIndex(index, similarNodesInCluster.Count()) + " " + node.NodeInfo.GetDefaultExpression();
            }
            // 6. Use numbered expression identified by cluster (numbering relative to cluster)
            else
            {
                return GetDisplayStringForObjectIndex(index, similarNodesInCluster.Count()) + " " + node.NodeInfo.GetDefaultExpression() + " of the " + node.Parent.CommonReferringExpression;
            }
        }
        // 7. Use numbered expression (numbering relative to all objects)
        else
        {
            var index = Array.IndexOf(similarNodes, node) + 1;
            return GetDisplayStringForObjectIndex(index, similarNodes.Count()) + " " + node.NodeInfo.GetDefaultExpression();
        }
    }

    // Sorts similar nodes in the order of first definite mention. This is complicated, since sometimes two or more
    // are referred to at the first definite mention. Then the order between these still needs to be determined.
    private IEnumerable<TextEntity> SortEntitiesByFirstDefiniteMention(IEnumerable<TextEntity> entities, TextStructure textStructure)
    {
        if (entities.Count() <= 1)
            return entities;

        foreach(var sentence in textStructure.Sentences)
        {
            foreach (var reference in sentence.TextEntityReferences)
            {
                var referenceDefiniteEntities = reference.Entities.Where((e, i) => !reference.PrefersIndefiniteForm[i]);
                var intersectingEntities = referenceDefiniteEntities.Intersect(entities);

                if (intersectingEntities.Count() < entities.Count() && intersectingEntities.Any())
                {
                    return SortEntitiesByFirstDefiniteMention(intersectingEntities.ToArray(), textStructure)
                        .Concat(SortEntitiesByFirstDefiniteMention(entities.Except(intersectingEntities).ToArray(), textStructure));
                }
            }
        }

        return entities;
    }

    private void GenerateReferringExpressionsEntities(TextStructure textStructure)
    {
        // Here we assign all entities a unique expression to refer to them by numbering in case of duplicates.
        // The strategy to do so is the following
        // 1. Use basic expression
        // 2. Use basic expression identified by cluster
        // 3. Use spezialized expression
        // 4. Use spezialized expression identified by cluster
        // 5. Use numbered expression (numbering relative to cluster)
        // 6. Use numbered expression identified by cluster (numbering relative to cluster)
        // 7. Use numbered expression (numbering relative to all objects)

        // Put all nodes into groups of similar nodes and sort them by Occurence within the group.
        var groupedNodes = textStructure.Entities
            .GroupBy(e => e.NodeInfo.GetDefaultExpression())
            .ToDictionary(g => g.Key, g => SortEntitiesByFirstDefiniteMention(g, textStructure).ToArray());

        // We leverage the sorting of the previous group
        var spezializedGroupedNodes = textStructure.Entities
            .GroupBy(e => e.NodeInfo.ExpressionHierachy.Last())
            .ToDictionary(g => g.Key, g => SortEntitiesByFirstDefiniteMention(g, textStructure).ToArray());

        foreach (var node in textStructure.Entities)
        {
            node.CommonReferringExpression = GenerateReferringExpressionsEntity(node, groupedNodes, spezializedGroupedNodes);
        }
    }

    private TextStructure SkipContainsSentenceForSomeEntities(TextStructure textStructure)
    {
        var newSentences = new List<SentenceSkeleton>();

        foreach (var sentence in textStructure.Sentences)
        {
            if(sentence is ConsistsOfSentenceSkeleton consistsOfSentence)
            {
                var newContent = new List<TextEntity>();

                foreach(var entity in consistsOfSentence.Content.Entities)
                {
                    // Objects can be introduced later by a relates to sentence iff all these conditions are satisfied:

                    // 1. The entity is not important enough to be mentioned in the consists of sentence
                    if(!(entity.NodeInfo.ObjectImportance < EnableDelayedIntroductionForSingleObjectsWithImportanceLessThan &&

                    // 2. The entity is the only entity of this type mentioned in the consists of sentence
                        consistsOfSentence.Content.Entities.Count(p => p.NodeInfo.GetDefaultExpression() == entity.NodeInfo.GetDefaultExpression()) == 1 &&

                    // 3. The entity is mentioned in some later relates to sentence as the subject and the object of this sentence is part of the same parent.
                    textStructure.Sentences.Any(s => s is RelatesToSentenceSkeleton r &&
                            (r.Subject.Entities.Contains(entity) && r.Object.Entities.Any(e => e.Parent == entity.Parent)))))
                    {
                        newContent.Add(entity);
                    }
                }

                newSentences.Add(new ConsistsOfSentenceSkeleton(
                    new TextEntityReference(consistsOfSentence.Container.Entities.ToArray()),
                    new TextEntityReference(newContent.ToArray())));
            }
            else
            {
                newSentences.Add(sentence);
            }
        }

        return new TextStructure(textStructure.Entities, newSentences.ToArray());
    }


    private TextStructure MoveSomeRelatesToIntoConsistsOfSentences(TextStructure textStructure)
    {
        var newSentences = new List<SentenceSkeleton>();
        var deletedSentences = new List<SentenceSkeleton>();

        foreach (var sentence in textStructure.Sentences)
        {
            if (sentence is ConsistsOfSentenceSkeleton consistsOfSentence)
            {
                RelatesToSentenceSkeleton sentenceToIntegrate = null;
                foreach (var entity in consistsOfSentence.Content.Entities)
                {
                    // Let's look for a canidate sentence to integrate into the introduction sentence. Criterias are:
                    sentenceToIntegrate = textStructure.Sentences.FirstOrDefault(s => s is RelatesToSentenceSkeleton r &&
                        // 1. All entities the canidate sentence refers to need to be contained in the consists of sentence
                        !r.TextEntityReferences.SelectMany(t => t.Entities).Except(consistsOfSentence.Content.Entities).Any() &&

                        // 2. Both subject and object of the canidate sentence should only consists of objects of one type each.
                        r.Subject.Entities.Select(e => e.NodeInfo.GetDefaultExpression()).Distinct().Count() == 1 &&
                        r.Object.Entities.Select(e => e.NodeInfo.GetDefaultExpression()).Distinct().Count() == 1 &&

                        // 3. The canidate sentence should concern all objects in the consists of sentence which are of the types
                        //    its subject and subject are of.
                        !consistsOfSentence.Content.Entities.Except(r.TextEntityReferences.SelectMany(t => t.Entities))
                            .Any(e => r.TextEntityReferences.SelectMany(t => t.Entities).Any(e2 => e2.NodeInfo.GetDefaultExpression() == e.NodeInfo.GetDefaultExpression()))) as RelatesToSentenceSkeleton;

                    if(sentenceToIntegrate != null)
                    {
                        break;
                    }
                }
                if(sentenceToIntegrate != null)
                {
                    newSentences.Add(new ConsistsOfSentenceSkeleton(consistsOfSentence.Container, new TextEntityReference(consistsOfSentence.Content.Entities.Except(sentenceToIntegrate.TextEntityReferences.SelectMany(t => t.Entities)).ToArray()), sentenceToIntegrate));
                    deletedSentences.Add(sentenceToIntegrate);
                }
                else
                {
                    newSentences.Add(sentence);
                }
            }
            else
            {
                newSentences.Add(sentence);
            }
        }

        return new TextStructure(textStructure.Entities, newSentences.Except(deletedSentences).ToArray());
    }

    private void MakeSomeReferencesIndefinite(TextStructure textStructure)
    {
        var introducedEntities = new List<TextEntity>() { textStructure.Entities.First() };
        foreach (var sentence in textStructure.Sentences)
        {
            foreach (var reference in sentence.TextEntityReferences)
            {
                for(int i = 0; i <reference.Entities.Length; ++i)
                {
                    var entity = reference.Entities[i];
                    if(!introducedEntities.Contains(entity))
                    {
                        reference.PrefersIndefiniteForm[i] = true;
                        introducedEntities.Add(entity);
                    }
                }
            }
        }
    }

    private void GenerateRefferingExpressionsSentencesBasic(TextStructure textStructure)
    {
        foreach (var sentence in textStructure.Sentences)
        {
            foreach (var reference in sentence.TextEntityReferences)
            {
                reference.ReferringExpression =
                    CoordinatedPhraseFromElements(reference.Entities.Zip(reference.PrefersIndefiniteForm, (item, prefersIndefiniteForm) =>
                    prefersIndefiniteForm ? factory.createNounPhrase("a", item.NodeInfo.GetDefaultExpression()) : factory.createNounPhrase("the", item.CommonReferringExpression)));                    
            }
        }
    }

    private INLGElement CoordinatedPhraseFromElements(IEnumerable<INLGElement> elements)
    {
        elements = elements.Where(e => e != nothingPlaceholderWord);

        if(!elements.Any())
        {
            return nothingPlaceholderWord;               
        }
        else if (elements.Count() == 1)
        {
            return elements.First();
        }
        else
        {
            var result = new CoordinatedPhraseElement();
            foreach (var element in elements)
            {
                if(element is CoordinatedPhraseElement coordinatedPhraseElement)
                {
                    foreach (var child in coordinatedPhraseElement.getChildren())
                        result.addCoordinate(child);
                }
                else
                {
                    result.addCoordinate(element);
                }
            }
            return result;
        }
    }

    private void GenerateReferringExpressionsSentences(TextStructure textStructure)
    {
        var lastSentenceForwardLookingCenters = new List<TextEntityReference>();

        foreach (var sentence in textStructure.Sentences)
        {
            var thisSentenceForwardLookingCenters = new List<TextEntityReference>();

            var coordinatedPhraseElementBuilders = new List<Action>();
            foreach (var reference in sentence.TextEntityReferences)
            {
                var lastSentencePronounUse = lastSentenceForwardLookingCenters.FirstOrDefault(c => c.ReferringExpression.category is LexicalCategory_PRONOUN);
                var usedPronounForReferenceLastSentence = lastSentencePronounUse?.IsReferencingSameEntitiesAs(reference);

                // If the backward looking center is identical to the expression we are trying to describe, we can
                // just use a pronoun.
                if (usedPronounForReferenceLastSentence == true || (lastSentenceForwardLookingCenters.FirstOrDefault()?.IsReferencingSameEntitiesAs(reference) == true &&
                    usedPronounForReferenceLastSentence == null))
                {
                    var pronounWord = reference.Entities.Length == 1 ? "it" : "they";
                    reference.ReferringExpression = factory.createWord(pronounWord, new LexicalCategory_PRONOUN());

                    // If we use a pronoun, we prefer this reference to be in front of the sentence, in order to keep the central element from changing.
                    // This is why we will introduce an inversion.
                    if (EnableCentralElementInversions && sentence is RelatesToSentenceSkeleton r && r.TextEntityReferences.FirstOrDefault() != reference)
                    {
                        r.RealizeInverted = !r.RealizeInverted;
                        thisSentenceForwardLookingCenters.Insert(0, reference);
                    }
                    else
                    {
                        thisSentenceForwardLookingCenters.Add(reference);
                    }
                }
                // Otherwise we need to manually build an expression that describe all objects of the reference
                else 
                {
                    var groupedSubReferences = reference.Entities.Zip(reference.PrefersIndefiniteForm, (entity, prefersDefiniteForm) => (entity, prefersDefiniteForm))
                        .OrderBy(p => p.prefersDefiniteForm).ThenByDescending(p => p.entity.NodeInfo.ObjectImportance)
                        .GroupBy(p => p.entity.NodeInfo.GetDefaultExpression())
                        .Select(p => new TextEntityReference(p.Select(q => q.entity).ToArray(), p.Select(q => q.prefersDefiniteForm).ToArray())).ToArray();

                    foreach (var subReference in groupedSubReferences)
                    {
                        var expression = subReference.Entities.First().NodeInfo.GetDefaultExpression();
                        var count = subReference.Entities.Count();

                        var lastSentenceMentionedThisEntity = count == 1 ? lastSentenceForwardLookingCenters.Any(c => c.Entities.Contains(subReference.Entities.First())) : lastSentenceForwardLookingCenters.Any(c => c.IsReferencingSameEntitiesAs(subReference));
                        var previousSentenceSimilarEntities = lastSentenceForwardLookingCenters.SelectMany(c => c.Entities.Where(e => e.NodeInfo.GetDefaultExpression() == expression)).Distinct();
                        var lastSentenceMentionedThisEntityAndNoSimilarEntities = lastSentenceMentionedThisEntity && previousSentenceSimilarEntities.Count() == count;

                        var prefersIndefiniteForm = subReference.PrefersIndefiniteForm.Any(e => e);

                        if (count == 1)
                        {
                            if(prefersIndefiniteForm)
                            {
                                subReference.ReferringExpression = factory.createNounPhrase("a", expression);
                            }
                            // Only use "this expression" over the in cases where the CommonReferringExpression is more complicated.
                            else if (lastSentenceMentionedThisEntityAndNoSimilarEntities && expression != subReference.Entities.First().CommonReferringExpression)
                            {
                                subReference.ReferringExpression = factory.createNounPhrase("this", expression);
                            }
                            else
                            {
                                subReference.ReferringExpression = factory.createNounPhrase("the", subReference.Entities.First().CommonReferringExpression);
                            }
                        }
                        else
                        {
                            var describesAllItemsOfThisType = textStructure.Entities.Count(p => p.NodeInfo.GetDefaultExpression() == expression) == count;

                            expression = expression.Trim();

                            var complement = "";
                            var adjectives = "";
                            var ofIndex = expression.IndexOf(" of ");
                            if(ofIndex >= 0)
                            {
                                complement = expression.Substring(ofIndex + 1);
                                expression = expression.Substring(0, ofIndex);
                            }
                                
                            var multiwordSeperatorIndex = expression.LastIndexOf(" ");
                            if (multiwordSeperatorIndex >= 0)
                            {
                                adjectives = expression.Substring(0, multiwordSeperatorIndex);
                                expression = expression.Substring(multiwordSeperatorIndex + 1);
                            }

                            var numberString = GetDisplayStringForNumber(count);

                            NPPhraseSpec referringExpression;

                            if(prefersIndefiniteForm)
                            {
                                referringExpression = factory.createNounPhrase(numberString, expression);
                                referringExpression.setPlural(true);

                            } else if(lastSentenceMentionedThisEntityAndNoSimilarEntities)
                            {
                                referringExpression = factory.createNounPhrase("these " + numberString, expression);
                                referringExpression.setPlural(true);
                            }
                            else if (describesAllItemsOfThisType)
                            {
                                referringExpression = factory.createNounPhrase(count > 2 ? "the " + numberString : "both", expression);
                                referringExpression.setPlural(true);
                            }
                            else
                            {
                                referringExpression = factory.createNounPhrase(numberString + " of the", expression);
                                referringExpression.setPlural(true);
                            }

                            if (complement != "")
                            {
                                referringExpression.addComplement(complement);
                            }
                            if (adjectives != "")
                            {
                                referringExpression.addPreModifier(adjectives);
                            }

                            subReference.ReferringExpression = referringExpression;
                        }
                    }

                    thisSentenceForwardLookingCenters.Add(reference);
                    reference.ReferringExpression = CoordinatedPhraseFromElements(groupedSubReferences.Select(r => r.ReferringExpression));
                }
            }


            lastSentenceForwardLookingCenters = thisSentenceForwardLookingCenters;
        }
    }

    private string RealizeText(TextStructure textStructure)
    {
        var sentences = new List<INLGElement>();

        foreach(var sentenceSkeleton in textStructure.Sentences)
        {
            switch(sentenceSkeleton)
            {
                case ConsistsOfSentenceSkeleton consistsOfSentence:
                    {
                        var sentence = factory.createClause();

                        var subject = consistsOfSentence.Container.ReferringExpression;
                        sentence.setSubject(subject);
                        sentence.setVerb("consist of");

                        if (consistsOfSentence.ContainedRelationshipInformation != null)
                        {
                            var relatesToPhrase = factory.createNounPhrase(consistsOfSentence.ContainedRelationshipInformation.Subject.ReferringExpression);

                            var objectPhrase = factory.createPrepositionPhrase(consistsOfSentence.ContainedRelationshipInformation.RelateTo.GetExpression());
                            objectPhrase.addComplement(consistsOfSentence.ContainedRelationshipInformation.Object.ReferringExpression);
                            relatesToPhrase.addPostModifier(objectPhrase);

                            sentence.setObject(CoordinatedPhraseFromElements(new INLGElement[] { relatesToPhrase, consistsOfSentence.Content.ReferringExpression }));
                        }
                        else 
                        {
                            sentence.setObject(consistsOfSentence.Content.ReferringExpression);
                        }

                        sentences.Add(sentence);
                    }
                    break;
                case RelatesToSentenceSkeleton relatesToSentenceSkeleton:
                    {
                        var itemA = relatesToSentenceSkeleton.Subject.ReferringExpression;
                        var itemB = relatesToSentenceSkeleton.Object.ReferringExpression;

                        if(!relatesToSentenceSkeleton.RealizeInverted)
                        {
                            var sentence = factory.createClause();
                            sentence.setSubject(itemA);

                            sentence.setVerbPhrase(factory.createVerbPhrase("be"));

                            var objectPhrase = factory.createPrepositionPhrase(relatesToSentenceSkeleton.RelateTo.GetExpression());
                            objectPhrase.addComplement(itemB);

                            sentence.setObject(objectPhrase);

                            sentences.Add(sentence);
                        }
                        else
                        {
                            var sentence = factory.createClause();

                            var subjectPhrase = factory.createPrepositionPhrase(relatesToSentenceSkeleton.RelateTo.GetExpression());
                            subjectPhrase.addComplement(itemB);

                            sentence.setSubject(subjectPhrase);

                            var verbPhrase = factory.createVerbPhrase("be");
                            verbPhrase.setPlural(relatesToSentenceSkeleton.Subject.Entities.Count() > 1);
                            sentence.addComplement(verbPhrase);

                            sentence.addPostModifier(itemA);

                            sentences.Add(sentence);
                        }
                    }
                    break;
            }
        }

        return string.Join(" ", sentences.Select(p => realiser.realiseSentence(p)));
    }

    public string Describe(SceneRelationshipGraph graph, NodeInfoProvider[] nodeInfos)
    {
        var textContent = DetermineTextContent(graph, nodeInfos);
        var textStructure = StructureTextContent(textContent);

        if (EnableAggregateTextStructure)
            textStructure = AggregateTextStructure(textStructure);

        if (EnableDelayedIntroductionForSingleObjectsWithImportanceLessThan > 0.0f)
            textStructure = SkipContainsSentenceForSomeEntities(textStructure);

        if(EnableConsistsOfSentencesIntegratingRelationships)
            textStructure = MoveSomeRelatesToIntoConsistsOfSentences(textStructure);

        MakeSomeReferencesIndefinite(textStructure);

        if (EnableGenerateReferringExpressionsEntities)
            GenerateReferringExpressionsEntities(textStructure);

        if (EnableGenerateReferringExpressionsSentences)
            GenerateReferringExpressionsSentences(textStructure);
        else
            GenerateRefferingExpressionsSentencesBasic(textStructure);

        return RealizeText(textStructure);
    }

    public string Describe(SceneRelationshipGraph graph, Dictionary<string, NodeInfoProvider> nodeInfoMap)
    {
        return Describe(graph, graph.Nodes.Select(n => nodeInfoMap[n]).ToArray());
    }

    private static string GetDisplayStringForNumber(int number)
    {
        switch (number)
        {
            case 0: return "zero";
            case 1: return "one";
            case 2: return "two";
            case 3: return "three";
            case 4: return "four";
            case 5: return "five";
            case 6: return "six";
            case 7: return "seven";
            case 8: return "eight";
            case 9: return "nine";
            case 10: return "ten";
            case 11: return "eleven";
            case 12: return "twelve";
            default: return "many";
        }
    }

    private static string GetDisplayStringForObjectIndex(int number, int groupSize)
    {
        switch (number)
        {
            case 0: return "zeroth";
            case 1: return "first";
            case 2: return groupSize == 2 ? "other" : "second";
            case 3: return "third";
            case 4: return "fourth";
            case 5: return "fiveth";
            case 6: return "sixth";
            case 7: return "seventh";
            case 8: return "eighth";
            case 9: return "nineth";
            case 10: return "tenth";
            case 11: return "eleventh";
            case 12: return "twelveth";
            default: return "some";
        }
    }
}