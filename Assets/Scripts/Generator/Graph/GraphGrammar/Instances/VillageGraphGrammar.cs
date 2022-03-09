using System.Linq;
using static RelationshipType;
using Rule = GraphGrammar.GraphGrammarRule<string, RelationshipType>;

namespace GraphGrammar.Examples
{
    public partial class VillageGraphGrammar : GraphGrammar<string, RelationshipType>
    {
        private static Graph<string, RelationshipType>[] VillageStartSymbols =
        {
            new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "Path/Full" }, (1, OnTopOf, 0)),
            new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "Path/Full" }, (1, OnTopOf, 0)),
            new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "Path/Full" }, (1, OnTopOf, 0)),
            new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "Path/Full" }, (1, OnTopOf, 0)),
            new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "Path/Intersection3", "Path/Half", "Path/Half", "Path/Half" },
                    (1, OnTopOf, 0), (2, OnTopOf, 0), (3, OnTopOf, 0), (4, OnTopOf, 0), (2, PathConnectedTo, 1), (3, PathConnectedTo, 1), (4, PathConnectedTo, 1)),
            new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "Path/Intersection3", "Path/Half", "Path/Half", "Path/Half" },
                    (1, OnTopOf, 0), (2, OnTopOf, 0), (3, OnTopOf, 0), (4, OnTopOf, 0), (2, PathConnectedTo, 1), (3, PathConnectedTo, 1), (4, PathConnectedTo, 1)),
            new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "Path/Intersection4", "Path/Half", "Path/Half", "Path/Half", "Path/Half" },
                    (1, OnTopOf, 0), (2, OnTopOf, 0), (3, OnTopOf, 0), (4, OnTopOf, 0), (5, OnTopOf, 0), (2, PathConnectedTo, 1), (3, PathConnectedTo, 1), (4, PathConnectedTo, 1), (5, PathConnectedTo, 1))
        };

        private static Rule[] BuildingRules = new Rule[] {
            new Rule("ConstructBuildingRule", 0.6f,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "Path" },
                    (1, OnTopOf, 0)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains","Path", "Building",},
                    (1, OnTopOf, 0),
                    (2, OnTopOf, 0),
                    (2, NextTo, 1)
                ), 8),


            new Rule("PlaceTreeInBetweenBuildings", 0.6f,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "Building", "Building" },
                    (1, OnTopOf, 0),
                    (2, OnTopOf, 0)
                ),
                3, 2,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "Building", "Building", "PropTreeLeaves" },
                    (1, OnTopOf, 0),
                    (2, OnTopOf, 0),
                    (3, NextToEast, 2),
                    (3, NextToWest, 1),
                    (3, OnTopOf, 0)
                )),
                new Rule("FlowerPotNextToBuilding", 0.8f,
                    new SceneRelationshipGraph(
                        new string[] {  "TerrainPlains", "Building" },
                        (1, OnTopOf, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] {  "TerrainPlains", "Building", "PropFlowerPot"},
                        (1, OnTopOf, 0),
                        (2, NextToChooseDirection, 1)
                    ), 9),
                new Rule("CrateStackNextToBuilding", 0.5f,
                    new SceneRelationshipGraph(
                        new string[] {  "TerrainPlains", "Building" },
                        (1, OnTopOf, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] {  "TerrainPlains", "Building", "PropCrateStack"},
                        (1, OnTopOf, 0),
                        (2, OnTopOf, 0),
                        (2, NextToChooseDirection, 1)
                    ), 8),
                new Rule("BarrelNextToBuilding", 0.3f,
                    new SceneRelationshipGraph(
                        new string[] {  "TerrainPlains", "Building" },
                        (1, OnTopOf, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] {  "TerrainPlains", "Building", "PropBarrel"},
                        (1, OnTopOf, 0),
                        (2, OnTopOf, 0),
                        (2, NextToChooseDirection, 1)
                    ), 8),
                new Rule("ClothesLineNextToBuilding", 1.5f,
                    new SceneRelationshipGraph(
                        new string[] {  "TerrainPlains", "Building" },
                        (1, OnTopOf, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] {  "TerrainPlains", "Building", "PropClothesline"},
                        (1, OnTopOf, 0),
                        (2, OnTopOf, 0),
                        (2, NextToBehind, 1)
                    ), 7)
        };

        private static Rule[] SpezializeRules = new Rule[] {
            
            new Rule("SpezializeNextToLeft", 2,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToChooseDirection, 1)
                ),
                2, 0,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToLeft, 1)
                )),
            new Rule("SpezializeNextToRight", 2,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToChooseDirection, 1)
                ),
                2, 0,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToLeft, 1)
                )),
            new Rule("SpezializeNextToFront", 2,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToChooseDirection, 1)
                ),
                2, 0,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToFront, 1)
                )),
            new Rule("SpezializeNextToBehind", 2,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToChooseDirection, 1)
                ),
                2, 0,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToBehind, 1)
                )),

            new Rule("SpezializeNextToNorth", 1,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToChooseDirection, 1)
                ),
                2, 0,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToNorth, 1)
                )),
            new Rule("SpezializeNextToEast", 1,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToChooseDirection, 1)
                ),
                2, 0,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToEast, 1)
                )),
            new Rule("SpezializeNextToSouth", 1,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToChooseDirection, 1)
                ),
                2, 0,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToSouth, 1)
                )),
            new Rule("SpezializeNextToWest", 1,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToChooseDirection, 1)
                ),
                2, 0,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToWest, 1)
                )),


            new Rule("SpezializeNextToBesidesLeft", 2,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToChooseSide, 1)
                ),
                2, 0,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToLeft, 1)
                )),

            new Rule("SpezializeNextToBesidesRight", 2,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToChooseSide, 1)
                ),
                2, 0,
                new SceneRelationshipGraph(
                    new string[] { "", "" },
                    (0, NextToRight, 1)
                ))
        };

        private static Rule[] DecorationRules = new Rule[] {

             new Rule("ConstructWellRule1", 0.4f,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains" }
                ),
                1, 0,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "PropWell" },
                    (1, OnTopOf, 0)
                ), 4),

             new Rule("ConstructWagon", 0.3f,
                new SceneRelationshipGraph(
                    new string[] { "Path", "TerrainPlains" },
                    (0, OnTopOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "Path", "TerrainPlains", "PropWagon" },
                    (0, OnTopOf, 1),
                    (2, NextTo, 0)
                ), 4),

            new Rule("PlaceWaterBucket", 0.2f,
                new SceneRelationshipGraph(
                    new string[] { "PropWell", "" },
                    (0, OnTopOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "PropWell", "", "PropWaterBucket" },
                    (0, OnTopOf, 1),
                    (2, NextToChooseDirection, 0),
                    (2, OnTopOf, 1)
                ), 4),

             new Rule("PlaceStreetLamp",1.0f,
                new SceneRelationshipGraph(
                    new string[] { "Path", "TerrainPlains" },
                    (0, OnTopOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "Path", "TerrainPlains", "PropStreetLamp" },
                    (0, OnTopOf, 1),
                    (2, OnTopOf, 1),
                    (2, NextTo, 0)
                ), 8),
             new Rule("PlaceNoticeBoard", 0.5f,
                new SceneRelationshipGraph(
                    new string[] { "Path/Intersection3", "TerrainPlains" },
                    (0, OnTopOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "Path/Intersection3", "TerrainPlains", "PropNoticeBoard" },
                    (0, OnTopOf, 1),
                    (2, OnTopOf, 1),
                    (2, NextTo, 0),
                    (2, ImplicitFacingTowards, 0)
                ), 5)
        };

        public VillageGraphGrammar() : base(VillageStartSymbols,
            new Rule[][] {
                BuildingRules,
                SpezializeRules,
                DecorationRules,
                FarmRules,
                MarketRules,
                BlacksmithRules,
                MillRules,
                VegetationRules
            }.SelectMany(r => r).ToArray())
        {
        }
    }
}
