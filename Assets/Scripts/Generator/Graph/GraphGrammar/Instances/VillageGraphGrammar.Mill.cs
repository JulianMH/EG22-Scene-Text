using static RelationshipType;
using Rule = GraphGrammar.GraphGrammarRule<string, RelationshipType>;

namespace GraphGrammar.Examples
{
    public partial class VillageGraphGrammar : GraphGrammar<string, RelationshipType>
    {
        private static Rule[] MillRules = new Rule[] {
            new Rule("ConstructMillRule", 0.5f,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "Path" },
                    (1, OnTopOf, 0)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "Path", "Mill", "BuildingWindmill", "PropSackFlour", "PropSackFlour" },
                    (1, OnTopOf, 0),
                    (2, OnTopOf, 0),
                    (2, NextTo, 1),
                    (3, IsPartOf, 2),
                    (4, IsPartOf, 2),
                    (5, IsPartOf, 2),
                    (4, NextToChooseSide, 3),
                    (5, NextToChooseSide, 3)
                ),8),


            new Rule("PlaceSackFlour", 0.5f,
                new SceneRelationshipGraph(
                    new string[] { "BuildingWindmill", "Mill" },
                    (0, IsPartOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "BuildingWindmill", "Mill", "PropSackFlour" },
                    (0, IsPartOf, 1),
                    (2, IsPartOf, 1),
                    (2, NextToChooseSide, 0)
                ), 7),
            new Rule("PlaceWheatPallet", 0.3f,
                new SceneRelationshipGraph(
                    new string[] { "BuildingWindmill", "Mill" },
                    (0, IsPartOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "BuildingWindmill", "Mill", "PropWheatPallet" },
                    (0, IsPartOf, 1),
                    (2, IsPartOf, 1),
                    (2, NextToChooseSide, 0)
                ), 7),
            new Rule("PlaceWheatWagon", 0.3f,
                new SceneRelationshipGraph(
                    new string[] { "BuildingWindmill", "Mill" },
                    (0, IsPartOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "BuildingWindmill", "Mill", "PropWagon/Wheat" },
                    (0, IsPartOf, 1),
                    (2, IsPartOf, 1),
                    (2, NextToChooseSide, 0)
                ), 7)
        };

    }
}