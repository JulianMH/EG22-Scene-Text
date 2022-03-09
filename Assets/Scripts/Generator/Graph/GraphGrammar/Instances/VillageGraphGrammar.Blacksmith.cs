using static RelationshipType;
using Rule = GraphGrammar.GraphGrammarRule<string, RelationshipType>;

namespace GraphGrammar.Examples
{
    public partial class VillageGraphGrammar : GraphGrammar<string, RelationshipType>
    {
        private static Rule[] BlacksmithRules = new Rule[] {
            new Rule("ConstructBlacksmithRule", 0.5f,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "Path" },
                    (1, OnTopOf, 0)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "Path", "Blacksmith",  "Building", "PropForge", "PropBrazier", "PropAnvil", "PropSignBlacksmith" },
                    (1, OnTopOf, 0),
                    (2, OnTopOf, 0),
                    (2, NextTo, 1),
                    (3, OnTopOf, 0),
                    (2, NextToChooseSide, 3),
                    (4, IsPartOf, 2),
                    (5, IsPartOf, 2),
                    (6, IsPartOf, 2),
                    (4, FacingAwayFrom, 3),
                    (5, NextToFront, 4),
                    (5, ImplicitFacingSameDirection, 4),
                    (6, NextToFront, 5),
                    (7, NextToChooseDirection, 3),
                    (7, IsPartOf, 2)
                ),8),

            new Rule("PlaceCoalBucket", 1,
                new SceneRelationshipGraph(
                    new string[] { "PropBrazier", "" },
                    (0, IsPartOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "PropBrazier", "", "PropCoalBucket" },
                    (0, IsPartOf, 1),
                    (2, NextToChooseSide, 0),
                    (2, IsPartOf, 1)
                )),

            new Rule("PlaceWaterBucket", 1,
                new SceneRelationshipGraph(
                    new string[] { "PropBrazier", "" },
                    (0, IsPartOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "PropBrazier", "", "PropWaterBucket" },
                    (0, IsPartOf, 1),
                    (2, NextToChooseSide, 0),
                    (2, IsPartOf, 1)
                )),

            new Rule("PlaceAnvil", 0.5,
                new SceneRelationshipGraph(
                    new string[] { "PropBrazier", "" },
                    (0, IsPartOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "PropBrazier", "", "PropAnvil" },
                    (0, IsPartOf, 1),
                    (2, NextToChooseDirection, 0),
                    (2, IsPartOf, 1)
                )),


            new Rule("PlaceBlacksmithTable1", 1,
                new SceneRelationshipGraph(
                    new string[] { "PropBrazier", "" },
                    (0, OnTopOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "PropBrazier", "", "PropBlacksmithTable" },
                    (0, OnTopOf, 1),
                    (2, NextToChooseSide, 0),
                    (2, OnTopOf, 1)
                )),

            new Rule("PlaceAnvilHammer", 3,
                new SceneRelationshipGraph(
                    new string[] { "PropAnvil" }
                ),
                1, 0,
                new SceneRelationshipGraph(
                    new string[] {  "PropAnvil", "PropTool/Hammer" },
                    (1, OnTopOf, 0)
                ),
                2),

            new Rule("PlaceBlacksmithTable2", 2,
                new SceneRelationshipGraph(
                    new string[] { "Blacksmith", "" },
                    (1, IsPartOf, 0)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "Blacksmith", "", "PropTable" },
                    (1, IsPartOf, 0),
                    (2, IsPartOf, 0),
                    (2, NextToChooseDirection, 1)
                )),

            new Rule("PlaceToolOnBlacksmithTable1", 3,
                new SceneRelationshipGraph(
                    new string[] {  "PropTable", "Blacksmith", },
                    (0, IsPartOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "PropTable", "Blacksmith", "PropTool/Mallet" },
                    (0, IsPartOf, 1),
                    (2, OnTopOf, 0)
                ), 3),
            new Rule("PlaceToolOnBlacksmithTable2", 3,
                new SceneRelationshipGraph(
                    new string[] {  "PropTable", "Blacksmith", },
                    (0, IsPartOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "PropTable", "Blacksmith", "PropTool/MalletBig" },
                    (0, IsPartOf, 1),
                    (2, OnTopOf, 0)
                ), 3),

            new Rule("PlaceToolOnBlacksmithTable3", 3,
                new SceneRelationshipGraph(
                    new string[] { "PropTable", "Blacksmith" },
                    (0, IsPartOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "PropTable", "Blacksmith", "PropTool/Saw" },
                    (0, IsPartOf,1),
                    (2, OnTopOf, 0)
                ), 3),

            new Rule("PlaceToolOnBlacksmithTable4", 3,
                new SceneRelationshipGraph(
                    new string[] { "PropTable", "Blacksmith" },
                    (0, IsPartOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "PropTable", "Blacksmith", "PropTool/SawBig" },
                    (0, IsPartOf, 1),
                    (2, OnTopOf, 0)
                ), 3),
            new Rule("PlaceToolOnBlacksmithTable5", 3,
                new SceneRelationshipGraph(
                    new string[] { "BlacksmithTable", "Blacksmith" },
                    (0, IsPartOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "BlacksmithTable", "Blacksmith", "PropTool/Pingers" },
                    (0, IsPartOf, 1),
                    (2, OnTopOf, 0)
                ), 3),
            new Rule("PlaceToolOnBlacksmithTable6", 3,
                new SceneRelationshipGraph(
                    new string[] { "BlacksmithTable", "Blacksmith" },
                    (0, IsPartOf, 1)
                ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "BlacksmithTable", "Blacksmith", "PropTool/PingersBig" },
                    (0, IsPartOf, 1),
                    (2, OnTopOf, 0)
                ), 3)
        };
    }
}