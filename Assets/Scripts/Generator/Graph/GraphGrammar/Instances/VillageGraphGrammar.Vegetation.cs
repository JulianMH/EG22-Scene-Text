using static RelationshipType;
using Rule = GraphGrammar.GraphGrammarRule<string, RelationshipType>;

namespace GraphGrammar.Examples
{
    public partial class VillageGraphGrammar : GraphGrammar<string, RelationshipType>
    {
        private static Rule[] VegetationRules = new Rule[] {
            new Rule("ConstructTreeLeaves", 0.4f,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "" }
                ),
                2, 0,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "", "PropTreeLeaves" },
                    (2, OnTopOf, 0)
                )),
            new Rule("ConstructBush1", 0.2f,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "" }
                ),
                2, 0,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "", "PropBush" },
                    (2, OnTopOf, 0)
                )),
            new Rule("ConstructGrass1", 0.05f,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "" }
                ),
                2, 0,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "", "PropGrass", "PropGrass", "PropGrass" },
                    (2, OnTopOf, 0),
                    (3, OnTopOf, 0),
                    (4, OnTopOf, 0)
                )),
            new Rule("ConstructGrass2", 0.05f,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "" }
                ),
                2, 0,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "", "PropFlower", "PropGrass", "PropGrass" },
                    (2, OnTopOf, 0),
                    (3, OnTopOf, 0),
                    (4, OnTopOf, 0)
                )),

            new Rule("ConstructTreeConifer", 0.3f,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "" }
                ),
                2, 0,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "", "PropTreeConifer" },
                    (2, OnTopOf, 0)
                )),

            new Rule("CreateSmallForestLeaves", 0.7f,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "PropTreeLeaves" },
                    (1, OnTopOf, 0)
                ),
                2, 0,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "PropTreeLeaves", "ForestSmall", "PropTreeLeaves", "PropTreeLeaves" },
                    (1, IsPartOf, 2),
                    (2, OnTopOf, 0),
                    (3, IsPartOf, 2),
                    (4, IsPartOf, 2)
                )),
            new Rule("CreateSmallForestConifer", 0.7f,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "PropTreeConifer" },
                    (1, OnTopOf, 0)
                ),
                2, 0,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "PropTreeConifer", "ForestSmall", "PropTreeConifer", "PropTreeConifer" },
                    (1, IsPartOf, 2),
                    (2, OnTopOf, 0),
                    (3, IsPartOf, 2),
                    (4, IsPartOf, 2)
                )),

            new Rule("GrowSmallForestConifer", 2f,
                new SceneRelationshipGraph(
                    new string[] { "ForestSmall", "" },
                    (1, IsPartOf, 0)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "ForestSmall", "", "PropTreeConifer", },
                    (1, IsPartOf, 0),
                    (2, IsPartOf, 0)
                ), 8),
            new Rule("GrowSmallForestLeaves", 2f,
                new SceneRelationshipGraph(
                    new string[] { "ForestSmall", "" },
                    (1, IsPartOf, 0)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "ForestSmall", "", "PropTreeLeaves", },
                    (1, IsPartOf, 0),
                    (2, IsPartOf, 0)
                ), 8),
            new Rule("GrowSmallForestBush", 1.5f,
                new SceneRelationshipGraph(
                    new string[] { "ForestSmall"}
                ),
                1, 0,
                new SceneRelationshipGraph(
                    new string[] { "ForestSmall", "PropBush", },
                    (1, IsPartOf, 0)
                ), 9),
            new Rule("GrowSmallForestMushroom", 1.5f,
                new SceneRelationshipGraph(
                    new string[] { "ForestSmall"}
                ),
                1, 0,
                new SceneRelationshipGraph(
                    new string[] { "ForestSmall", "PropMushroom", },
                    (1, IsPartOf, 0)
                ), 9)
        };
    }
}
