using static RelationshipType;
using Rule = GraphGrammar.GraphGrammarRule<string, RelationshipType>;

namespace GraphGrammar.Examples
{
    public partial class VillageGraphGrammar : GraphGrammar<string, RelationshipType>
    {
        private static Rule[] FarmRules = new Rule[] {
            new Rule("ConstructFarmRule", 1.0f,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "Path" },
                    (1, OnTopOf, 0)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "Path", "Farm", "Building", "BuildingShed", "Farmland", "PropWheatSheaf" },
                    (1, OnTopOf, 0),
                    (2, OnTopOf, 0),
                    (2, NextTo, 1),
                    (3, IsPartOf, 2),
                    (4, IsPartOf, 2),
                    (5, IsPartOfEdge, 2),
                    (6, IsPartOf, 2),
                    (6, NextToChooseSide, 4)
                ), 4),
            new Rule("PlaceFarmField", 2.0f,
                new SceneRelationshipGraph(
                    new string[] { "Farm" }
                ),
                1, 0,
                new SceneRelationshipGraph(
                    new string[] { "Farm", "Farmland"},
                    (1, IsPartOfEdge, 0)
                ), 6),
            new Rule("PlaceFarmBarn", 0.5f,
                new SceneRelationshipGraph(
                    new string[] { "Farm" }
                ),
                1, 0,
                new SceneRelationshipGraph(
                    new string[] { "Farm", "BuildingShed"},
                    (1, IsPartOf, 0)
                ), 6),
            new Rule("PlaceFarmTreeFruit", 1.2f,
                new SceneRelationshipGraph(
                    new string[] { "Farm", "" },
                    (1, IsPartOf, 0)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "Farm", "", "PropTreeFruit"},
                    (1, IsPartOf, 0),
                    (2, IsPartOf, 0),
                    (2, NextToChooseDirection, 1)
                ), 8),
            new Rule("PlaceFarmWheatSheaf", 0.5f,
                new SceneRelationshipGraph(
                    new string[] { "Farm", "BuildingShed" },
                    (1, IsPartOf, 0)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "Farm", "BuildingShed", "PropWheatSheaf" },
                    (1, IsPartOf, 0),
                    (2, IsPartOf, 0),
                    (2, NextToChooseSide, 1)
                ), 9),
            new Rule("PlaceFarmWheatPallet", 0.8f,
                new SceneRelationshipGraph(
                    new string[] { "Farm", "BuildingShed" },
                    (1, IsPartOf, 0)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "Farm", "BuildingShed", "PropWheatPallet" },
                    (1, IsPartOf, 0),
                    (2, IsPartOf, 0),
                    (2, NextToChooseSide, 1)
                ), 9),
            new Rule("PlaceFarmWagon", 0.4f,
                new SceneRelationshipGraph(
                    new string[] { "Farm", "" },
                    (1, IsPartOf, 0)
                ),
                2, 0,
                new SceneRelationshipGraph(
                    new string[] { "Farm", "", "PropWagon"},
                    (1, IsPartOf, 0),
                    (2, IsPartOf, 0),
                    (2, NextToChooseSide, 1)
                ), 8),
                new Rule("FlowerPotNextToFarmBuilding", 0.8f,
                    new SceneRelationshipGraph(
                        new string[] { "Farm", "Building" },
                        (1, IsPartOf, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "Farm", "Building", "PropFlowerPot"},
                        (1, IsPartOf, 0),
                        (2, IsPartOf, 0),
                        (2, NextToChooseDirection, 1)
                    ), 9),
                new Rule("CrateStackNextToFarmBuilding", 0.5f,
                    new SceneRelationshipGraph(
                        new string[] { "Farm", "Building" },
                        (1, IsPartOf, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "Farm", "Building", "PropCrateStack"},
                        (1, IsPartOf, 0),
                        (2, IsPartOf, 0),
                        (2, NextToChooseDirection, 1)
                    ), 8),
                new Rule("BarrelNextToFarmBuilding", 0.3f,
                    new SceneRelationshipGraph(
                        new string[] {  "Farm", "Building" },
                        (1, IsPartOf, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "Farm", "Building", "PropBarrel"},
                        (1, IsPartOf, 0),
                        (2, IsPartOf, 0),
                        (2, NextToChooseDirection, 1)
                    ), 8),
                new Rule("ScytheNextToFarmland", 0.4f,
                    new SceneRelationshipGraph(
                        new string[] {  "Farm", "Farmland" },
                        (1, IsPartOfEdge, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "Farm", "Farmland", "PropToolScythe"},
                        (1, IsPartOfEdge, 0),
                        (2, IsPartOf, 0),
                        (2, NextTo, 1)
                    ), 9),
                new Rule("RakeNextToFarmland", 0.4f,
                    new SceneRelationshipGraph(
                        new string[] {  "Farm", "Farmland" },
                        (1, IsPartOfEdge, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "Farm", "Farmland", "PropToolRake"},
                        (1, IsPartOfEdge, 0),
                        (2, IsPartOf, 0),
                        (2, NextTo, 1)
                    ), 9)
        };
    }
}
