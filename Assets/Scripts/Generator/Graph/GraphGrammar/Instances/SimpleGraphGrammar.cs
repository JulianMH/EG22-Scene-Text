using System.Linq;
using static RelationshipType;
using Rule = GraphGrammar.GraphGrammarRule<string, RelationshipType>;

namespace GraphGrammar.Examples
{
    public class SimpleGraphGrammar : GraphGrammar<string, RelationshipType>
    {
        private static Graph<string, RelationshipType>[] SimpleStartSymbols =
        {
            new SceneRelationshipGraph(
                    new string[] { "SimpleTerrain", "Path/Full" }, (1, OnTopOf, 0)),
            new SceneRelationshipGraph(
                    new string[] { "SimpleTerrain", "Path/Full" }, (1, OnTopOf, 0)),
            new SceneRelationshipGraph(
                    new string[] { "SimpleTerrain", "Path/Full" }, (1, OnTopOf, 0)),
            new SceneRelationshipGraph(
                    new string[] { "SimpleTerrain", "Path/Full" }, (1, OnTopOf, 0)),
            new SceneRelationshipGraph(
                    new string[] { "SimpleTerrain", "Path/Intersection3", "Path/Half", "Path/Half", "Path/Half" },
                    (1, OnTopOf, 0), (2, OnTopOf, 0), (3, OnTopOf, 0), (4, OnTopOf, 0), (2, PathConnectedTo, 1), (3, PathConnectedTo, 1), (4, PathConnectedTo, 1)),
            new SceneRelationshipGraph(
                    new string[] { "SimpleTerrain", "Path/Intersection3", "Path/Half", "Path/Half", "Path/Half" },
                    (1, OnTopOf, 0), (2, OnTopOf, 0), (3, OnTopOf, 0), (4, OnTopOf, 0), (2, PathConnectedTo, 1), (3, PathConnectedTo, 1), (4, PathConnectedTo, 1)),
            new SceneRelationshipGraph(
                    new string[] { "SimpleTerrain", "Path/Intersection4", "Path/Half", "Path/Half", "Path/Half", "Path/Half" },
                    (1, OnTopOf, 0), (2, OnTopOf, 0), (3, OnTopOf, 0), (4, OnTopOf, 0), (5, OnTopOf, 0), (2, PathConnectedTo, 1), (3, PathConnectedTo, 1), (4, PathConnectedTo, 1), (5, PathConnectedTo, 1))
        };

        private static Rule[] SampleRules = new Rule[] {
            new Rule("ConstructBox1", 0.1f,
                new SceneRelationshipGraph(
                    new string[] { "SimpleTerrain" }
                ),
                1, 0,
                new SceneRelationshipGraph(
                    new string[] { "SimpleTerrain", "SimpleLargeBox/Green" },
                    (1, OnTopOf, 0)
                ), 11),
            new Rule("ConstructBox2", 0.1f,
                new SceneRelationshipGraph(
                    new string[] { "SimpleTerrain" }
                ),
                1, 0,
                new SceneRelationshipGraph(
                    new string[] { "SimpleTerrain", "SimpleSmallBox/Green" },
                    (1, OnTopOf, 0)
                ), 11),

            new Rule("ConstructSphere", 0.1f,
                new SceneRelationshipGraph(
                    new string[] { "SimpleTerrain" }
                ),
                1, 0,
                new SceneRelationshipGraph(
                    new string[] { "SimpleTerrain", "SimpleSphere/Green" },
                    (1, OnTopOf, 0)
                ), 11),

            new Rule("ConstructTowerA", 0.1f,
                new SceneRelationshipGraph(
                    new string[] { "Path", "SimpleTerrain" },
                    (0, OnTopOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] {  "Path", "SimpleTerrain", "SimpleTower", "SimpleLargeBox", "SimpleLargeBox", "SimpleLargeBox", "SimpleLargeBox", "SimpleSmallBox" },
                    (0, OnTopOf, 1),
                    (2, OnTopOf, 1),
                    (2, NextTo, 0),
                    (3, IsPartOf, 2),
                    (4, OnTopOfMiddle, 3),
                    (5, OnTopOfMiddle, 4),
                    (6, OnTopOfMiddle, 5),
                    (7, OnTopOfMiddle, 6)
                ), 3),            
            new Rule("ConstructTowerB", 0.1f,
                new SceneRelationshipGraph(
                    new string[] { "Path", "SimpleTerrain" },
                    (0, OnTopOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] {  "Path", "SimpleTerrain", "SimpleTower", "SimpleLargeBox", "SimpleLargeBox", "SimpleSmallBox", "SimpleSmallBox", "SimpleSmallBox" , "SimpleSphere" },
                    (0, OnTopOf, 1),
                    (2, OnTopOf, 1),
                    (2, NextTo, 0),
                    (3, IsPartOf, 2),
                    (4, OnTopOfMiddle, 3),
                    (5, OnTopOfMiddle, 4),
                    (6, OnTopOfMiddle, 5),
                    (7, OnTopOfMiddle, 6),
                    (8, OnTopOfMiddle, 7)
                ), 3),
            new Rule("ConstructTowerC", 0.1f,
                new SceneRelationshipGraph(
                    new string[] { "Path", "SimpleTerrain" },
                    (0, OnTopOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] {  "Path", "SimpleTerrain", "SimpleTower", "SimpleLargeBox", "SimpleLargeBox", "SimpleLargeBox", "SimpleLargeBox", "SimpleLargeBox", "SimpleLargeBox" },
                    (0, OnTopOf, 1),
                    (2, OnTopOf, 1),
                    (2, NextTo, 0),
                    (3, IsPartOf, 2),
                    (4, OnTopOfMiddle, 3),
                    (5, OnTopOfMiddle, 4),
                    (6, OnTopOfMiddle, 5),
                    (7, OnTopOfMiddle, 6),
                    (8, OnTopOfMiddle, 7)
                ), 3),
            new Rule("ConstructBoxGroup", 0.4f,
                new SceneRelationshipGraph(
                    new string[] { "Path", "SimpleTerrain" },
                    (0, OnTopOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] {  "Path", "SimpleTerrain", "SimpleBoxGroup", "SimpleLargeBox", "SimpleLargeBox" },
                    (0, OnTopOf, 1),
                    (2, OnTopOf, 1),
                    (2, NextTo, 0),
                    (3, IsPartOf, 2),
                    (4, IsPartOf, 2),
                    (4, NextToChooseDirection, 3)
                ), 4),
            new Rule("AddBoxGroupLargeBoxNextTo", 0.3f,
                new SceneRelationshipGraph(
                    new string[] { "SimpleBoxGroup", "SimpleLargeBox" },
                    (1, IsPartOf, 0)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "SimpleBoxGroup", "SimpleLargeBox", "SimpleLargeBox" },
                    (1, IsPartOf, 0),
                    (2, IsPartOf, 0),
                    (2, NextToChooseDirection, 1)
                ), 8),
            new Rule("AddBoxGroupSmallBoxNextTo", 0.3f,
                new SceneRelationshipGraph(
                    new string[] { "SimpleBoxGroup", "SimpleLargeBox" },
                    (1, IsPartOf, 0)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "SimpleBoxGroup", "SimpleLargeBox", "SimpleSmallBox" },
                    (1, IsPartOf, 0),
                    (2, IsPartOf, 0),
                    (2, NextToChooseDirection, 1)
                ), 8),
            new Rule("AddBoxGroupSmallBoxOnTop1", 0.3f,
                new SceneRelationshipGraph(
                    new string[] { "SimpleBoxGroup", "SimpleLargeBox" },
                    (1, IsPartOf, 0)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "SimpleBoxGroup", "SimpleLargeBox", "SimpleSmallBox" },
                    (1, IsPartOf, 0),
                    (2, OnTopOfMiddle, 1)
                ), 8),
            new Rule("AddBoxGroupSmallBoxOnTop2", 0.3f,
                new SceneRelationshipGraph(
                    new string[] { "SimpleBoxGroup", "SimpleSmallBox" },
                    (1, IsPartOf, 0)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "SimpleBoxGroup", "SimpleSmallBox", "SimpleSmallBox" },
                    (1, IsPartOf, 0),
                    (2, OnTopOfMiddle, 1)
                ), 8),
            new Rule("ConstructSphereGroup", 0.2f,
                new SceneRelationshipGraph(
                    new string[] { "Path", "SimpleTerrain" },
                    (0, OnTopOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] {  "Path", "SimpleTerrain", "SimpleSphereGroup", "SimpleSphere", "SimpleSphere" },
                    (0, OnTopOf, 1),
                    (2, OnTopOf, 1),
                    (2, NextTo, 0),
                    (3, IsPartOf, 2),
                    (4, IsPartOf, 2),
                    (4, NextTo, 3)
                ), 4),
            new Rule("AddSphereGroupSphereNextTo", 0.9f,
                new SceneRelationshipGraph(
                    new string[] { "SimpleSphereGroup", "SimpleSphere" },
                    (1, IsPartOf, 0)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "SimpleSphereGroup", "SimpleSphere", "SimpleSphere" },
                    (1, IsPartOf, 0),
                    (2, IsPartOf, 0),
                    (1, NextTo, 0)
                ), 8),
            new Rule("AddSphereGroupSphere", 0.9f,
                new SceneRelationshipGraph(
                    new string[] { "SimpleSphereGroup" }
                ),
                1, 0,
                new SceneRelationshipGraph(
                    new string[] { "SimpleSphereGroup", "SimpleSphere" },
                    (1, IsPartOf, 0)
                ), 8)
        };

        public SimpleGraphGrammar() : base(SimpleStartSymbols,
            new Rule[][] {
                SampleRules
            }.SelectMany(r => r).ToArray())
        {
        }
    }
}
