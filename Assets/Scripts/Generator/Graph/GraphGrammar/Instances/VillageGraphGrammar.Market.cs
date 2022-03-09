using System.Linq;
using static RelationshipType;
using Rule = GraphGrammar.GraphGrammarRule<string, RelationshipType>;

namespace GraphGrammar.Examples
{
    public partial class VillageGraphGrammar : GraphGrammar<string, RelationshipType>
    {

        private static Rule[] BuildMarketStandRules(string name, SceneRelationshipGraph maketStand, params Rule[] otherRules)
        {
            return new Rule[]
            {
                new Rule("ConstructMarketRule" + name, 0.15f,
                    new SceneRelationshipGraph(
                        new string[] { "TerrainPlains", "Path" },
                        (1, OnTopOf, 0)
                    ),
                2, 1,
                new SceneRelationshipGraph(
                    new string[] { "TerrainPlains", "Path", "MarketCenter" }.Concat(maketStand.Nodes).ToArray(),
                    new (uint, RelationshipType, uint)[]
                    {
                        (1, OnTopOf, 0),
                        (2, NextTo, 1),
                        (3, NextTo, 1),
                        (2, OnTopOf, 0),
                        (3, OnTopOf, 0),
                        (3, NextToChooseDirection, 2),
                        (3, FacingTowards, 2)
                    }.Concat(maketStand.Edges.Select(r => (r.Item1 + 3, r.Item2, r.Item3 + 3))).ToArray()),
                6),

             new Rule("ExpandMarketRule" + name, 0.5f,
                new SceneRelationshipGraph(
                    new string[] { "MarketCenter", "TerrainPlains", "Path", },
                    (0, OnTopOf, 1),
                    (2, OnTopOf, 1),
                    (0, NextTo, 2)
                ),
                3, 3,
                new SceneRelationshipGraph(
                    new string[] { "MarketCenter", "TerrainPlains", "Path"}.Concat(maketStand.Nodes).ToArray(),
                    new (uint, RelationshipType, uint)[]
                    {
                    (0, OnTopOf, 1),
                    (2, OnTopOf, 1),
                    (0, NextTo, 2),

                    (3, OnTopOf, 1),
                    (3, NextToChooseDirection, 0),
                    (3, FacingTowards, 0),
                    (3, NextTo, 2),
                    }.Concat(maketStand.Edges.Select(r => (r.Item1 + 3, r.Item2, r.Item3 + 3))).ToArray()),
                   7),
            }.Concat(otherRules.Select(r => new Rule("MarketStand" + name + r.Name, r.Propability, r.OriginalGraphFragment, r.SharedNodeCount, r.SharedEdgeCount, r.ReplacementGraphFragment, r.FirstNodeConnectionMaximum))).ToArray();
        }

        private static Rule[] MarketRules => new Rule[][] 
        {
            #region Food Stand
            BuildMarketStandRules("Food1", 
                new SceneRelationshipGraph(
                    new string[] { "MarketStand/Food", "PropFoodTable", "PropFood", "PropFood", "PropSignFood" },
                        (1, IsPartOfFrontEdge, 0),
                        (1, ImplicitFacingSameDirection, 0),
                        (2, OnTopOf, 1),
                        (3, OnTopOf, 1),
                        (4, IsPartOf, 0),
                        (4, NextToChooseSide, 1),
                        (4, ImplicitFacingSameDirection, 1)),
                new Rule("TableFood1", 6,
                    new SceneRelationshipGraph(
                        new string[] { "PropFoodTable", "MarketStand/Food", },
                        (0, IsPartOfFrontEdge, 1)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "PropFoodTable", "MarketStand/Food", "PropFood"},
                        (0, IsPartOfFrontEdge, 1),
                        (2, OnTopOf, 0)
                    ), 3),
                new Rule("TableFood2", 3,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Food", },
                        (0, IsPartOfFrontEdge, 1)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Food", "PropFruitBasket"},
                        (0, IsPartOfFrontEdge, 1),
                        (2, OnTopOf, 0)
                    ), 3),
                new Rule("TableFood3", 3,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Food", },
                        (0, IsPartOfFrontEdge, 1)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Food", "PropVegetableBox"},
                        (0, IsPartOfFrontEdge, 1),
                        (2, OnTopOf, 0)
                    ), 3),
                new Rule("FloorFood1", 1,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Food", "PropTable", },
                        (1, IsPartOfFrontEdge, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Food", "PropTable", "PropVegetableBox"},
                        (1, IsPartOfFrontEdge, 0),
                        (2, IsPartOf, 0),
                        (2, NextToBehind, 1)
                    ), 7),
                new Rule("FloorFood2", 1,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Food", "PropTable", },
                        (1, IsPartOfFrontEdge, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Food", "PropTable", "PropBottleBox"},
                        (1, IsPartOfFrontEdge, 0),
                        (2, IsPartOf, 0),
                        (2, NextToBehind, 1)
                    ), 7),
                new Rule("FloorFood3", 1,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Food", "PropFoodTable", },
                        (1, IsPartOfFrontEdge, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Food", "PropFoodTable", "PropVegetableBox"},
                        (1, IsPartOfFrontEdge, 0),
                        (2, IsPartOf, 0),
                        (2, NextToBehind, 1)
                    ), 7)),
            #endregion

            #region Food Stand 2
            BuildMarketStandRules("Food2", 
                new SceneRelationshipGraph(
                    new string[] { "MarketStand/Food", "PropTable", "PropVegetableBox", "PropVegetableBox", "PropSignFood" },
                        (1, IsPartOfFrontEdge, 0),
                        (1, ImplicitFacingSameDirection, 0),
                        (2, OnTopOf, 1),
                        (3, OnTopOf, 1),
                        (4, IsPartOf, 0),
                        (4, NextToChooseSide, 1),
                        (4, ImplicitFacingSameDirection, 1))),
            #endregion

            #region Beverages Stand
            BuildMarketStandRules("Beverages", 
                new SceneRelationshipGraph(
                    new string[] { "MarketStand/Beverages", "PropTable", "PropBottleBox", "PropBottleBox", "PropSignBeverages" },
                        (1, IsPartOfFrontEdge, 0),
                        (1, ImplicitFacingSameDirection, 0),
                        (2, OnTopOf, 1),
                        (3, OnTopOf, 1),
                        (4, IsPartOf, 0),
                        (4, NextToChooseSide, 1),
                        (4, ImplicitFacingSameDirection, 1)),
                new Rule("TableBottles", 3,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Beverages", },
                        (0, IsPartOfFrontEdge, 1)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Beverages", "PropBottleBox"},
                        (0, IsPartOfFrontEdge, 1),
                        (2, OnTopOf, 0)
                    ), 3),
                new Rule("TableBottle", 3,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Beverages", },
                        (0, IsPartOfFrontEdge, 1)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Beverages", "PropBottle", "PropBottle"},
                        (0, IsPartOfFrontEdge, 1),
                        (2, OnTopOf, 0),
                        (3, OnTopOf, 0)
                    ), 5),
                new Rule("FloorBottles", 1,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Beverages", "PropFoodTable", },
                        (1, IsPartOfFrontEdge, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Beverages", "PropFoodTable", "PropBottleBox"},
                        (1, IsPartOfFrontEdge, 0),
                        (2, IsPartOf, 0),
                        (2, NextToBehind, 1)
                    ), 7)),
            #endregion

            #region Flowers Stand
            BuildMarketStandRules("Flowers", 
                new SceneRelationshipGraph(
                    new string[] { "MarketStand/Flowers", "PropTable", "PropFlowerPot", "PropFlowerPot", "PropSignPlants", "PropFlowerPot",  },
                        (1, IsPartOfFrontEdge, 0),
                        (1, ImplicitFacingSameDirection, 0),
                        (2, OnTopOf, 1),
                        (3, OnTopOf, 1),
                        (4, IsPartOf, 0),
                        (4, NextToChooseSide, 1),
                        (4, ImplicitFacingSameDirection, 1),
                        (5, OnTopOf, 1)),
                new Rule("TableFlowers", 3,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Flowers", },
                        (0, IsPartOfFrontEdge, 1)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Flowers", "PropFlowerPot"},
                        (0, IsPartOfFrontEdge, 1),
                        (2, OnTopOf, 0)
                    ), 7),
                new Rule("FloorFlowers", 1,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Flowers", "PropTable", },
                        (1, IsPartOfFrontEdge, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Flowers", "PropTable", "PropFlowerPot"},
                        (1, IsPartOfFrontEdge, 0),
                        (2, IsPartOf, 0),
                        (2, NextToBehind, 1)
                    ), 9)),
            #endregion

            #region Lumber Stand
            BuildMarketStandRules("Lumber", 
                new SceneRelationshipGraph(
                    new string[] { "MarketStand/Lumber", "PropTable", "PropLumberPile", "PropSignPlants" },
                        (1, IsPartOfFrontEdge, 0),
                        (1, ImplicitFacingSameDirection, 0),
                        (2, NextToBehind, 1),
                            (2, IsPartOf, 0),
                        (3, IsPartOf, 0),
                        (3, NextToChooseSide, 1),
                        (3, ImplicitFacingSameDirection, 1)),
                new Rule("FloorLumberPiles", 1,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Lumber", "PropTable", },
                        (1, IsPartOfFrontEdge, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Lumber", "PropTable", "PropLumberPile"},
                        (1, IsPartOfFrontEdge, 0),
                        (2, IsPartOf, 0),
                        (2, NextToBehind, 1)
                    ), 5),
                new Rule("FloorChoppingBlock", 0.7,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Lumber", "PropTable", },
                        (1, IsPartOfFrontEdge, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Lumber", "PropTable", "PropLumberChoppingBlock"},
                        (1, IsPartOfFrontEdge, 0),
                        (2, IsPartOf, 0),
                        (2, NextToBehind, 1)
                    ), 5),
                new Rule("FloorLumberSawStand", 0.7,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Lumber", "PropTable", },
                        (1, IsPartOfFrontEdge, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Lumber", "PropTable", "PropLumberSawStand"},
                        (1, IsPartOfFrontEdge, 0),
                        (2, IsPartOf, 0),
                        (2, NextToBehind, 1)
                    ), 5),
                new Rule("TableTool1", 1,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Lumber", },
                        (0, IsPartOfFrontEdge, 1)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Lumber", "PropTool/SawBig"},
                        (0, IsPartOfFrontEdge, 1),
                        (2, OnTopOf, 0)
                    ), 5),
                new Rule("TableTool2", 1.5,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Lumber", },
                        (0, IsPartOfFrontEdge, 1)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Lumber", "PropTool/Axe"},
                        (0, IsPartOfFrontEdge, 1),
                        (2, OnTopOf, 0)
                    ), 5),
                new Rule("TableTool3", 1,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Lumber", },
                        (0, IsPartOfFrontEdge, 1)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Lumber", "PropTool/Hatchet"},
                        (0, IsPartOfFrontEdge, 1),
                        (2, OnTopOf, 0)
                    ), 5)),
            #endregion
 
            #region Book Stand
            BuildMarketStandRules("Books",
                new SceneRelationshipGraph(
                    new string[] { "MarketStand/Books", "PropTable", "PropBook", "PropBook", "PropSignBooks", "PropChair" },
                        (1, IsPartOfFrontEdge, 0),
                        (1, ImplicitFacingSameDirection, 0),
                        (2, OnTopOf, 1),
                        (3, OnTopOf, 1),
                        (4, IsPartOf, 0),
                        (4, NextToChooseSide, 1),
                        (4, ImplicitFacingSameDirection, 1),
                        (5, IsPartOf, 0),
                        (5, NextToBehind, 1),
                        (5, ImplicitFacingSameDirection, 1)),
                new Rule("PlaceCandle1", 1.0f,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "PropBook" },
                        (1, OnTopOf, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "PropBook", "PropCandle"},
                        (1, OnTopOf, 0),
                        (2, OnTopOf, 0),
                        (2, NextTo, 1)
                    ), 7), 
                new Rule("PlaceCandle2", 0.5f,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "PropBookStack" },
                        (1, OnTopOf, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "PropBookStack", "PropCandle"},
                        (1, OnTopOf, 0),
                        (2, OnTopOf, 0),
                        (2, NextTo, 1)
                    ), 7),
                new Rule("PlaceStool", 0.6f,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Books", "PropTable" },
                             (1, IsPartOfFrontEdge, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Books","PropTable", "PropStool", "PropBook"},
                             (1, IsPartOfFrontEdge, 0),
                        (2, IsPartOf, 0),
                        (2, NextToBehind, 1),
                        (3, OnTopOf, 2)
                    ), 6)),
            #endregion
 
            #region Soup Stand
            BuildMarketStandRules("Soup",
                new SceneRelationshipGraph(
                    new string[] { "MarketStand/Soup", "PropTable",  "PropSignInn", "PropPotSoup"},
                        (1, IsPartOfFrontEdge, 0),
                        (1, ImplicitFacingSameDirection, 0),
                        (2, IsPartOf, 0),
                        (2, NextToChooseSide, 1),
                        (2, ImplicitFacingSameDirection, 1),
                        (3, OnTopOfMiddle, 1)),
                new Rule("PlaceCuttingBoard", 0.4f,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Soup", "PropPotSoup" },
                             (0, IsPartOfFrontEdge, 1),
                             (2, OnTopOfMiddle, 0)
                    ),
                    3, 2,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Soup",  "PropPotSoup", "PropCuttingBoard", "PropKnife"},
                        (0, IsPartOfFrontEdge, 1),
                        (2, OnTopOfMiddle, 0),
                        (3, OnTopOf, 0),
                        (3, NextToChooseDirection, 2),
                        (4, OnTopOf, 3)
                    ), 6),
                new Rule("PlaceCuttingBoardFood", 0.3f,
                    new SceneRelationshipGraph(
                        new string[] { "PropCuttingBoard", "MarketStand/Soup",  "PropTable" },
                             (2, IsPartOfFrontEdge, 1),
                             (0, OnTopOf, 2)
                    ),
                    3, 2,
                    new SceneRelationshipGraph(
                        new string[] { "PropCuttingBoard", "MarketStand/Soup", "PropTable", "PropFood" },
                        (2, IsPartOfFrontEdge, 1),
                        (0, OnTopOf,2),
                        (3, OnTopOf, 0)
                    ), 5),
                new Rule("PlaceDishPile", 0.3f,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Soup", },
                        (0, IsPartOfFrontEdge, 1)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Soup", "PropDishPile" },
                        (0, IsPartOfFrontEdge, 1),
                        (2, OnTopOf, 0)
                    ), 6),
                new Rule("PlaceCutleryBox", 0.3f,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Soup", },
                        (0, IsPartOfFrontEdge, 1)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Soup", "PropCutleryBox" },
                        (0, IsPartOfFrontEdge, 1),
                        (2, OnTopOf, 0)
                    ), 6),
                new Rule("PlaceEatingTable", 0.3f,
                    new SceneRelationshipGraph(
                        new string[] { "MarketCenter", "MarketStand/Soup", },
                        (1, NextToChooseDirection, 0)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "MarketCenter", "MarketStand/Soup", "PropTableSmall", "PropStool", "PropStool" },
                        (1, NextToChooseDirection, 0),
                        (2, IsPartOf, 0),
                        (3, IsPartOf, 0),
                        (4, IsPartOf, 0),
                        (2, NextToFront, 1),
                        (3, NextToChooseDirection, 2),
                        (4, NextToChooseDirection, 2),
                        (3, ImplicitFacingTowards, 2),
                        (4, ImplicitFacingTowards, 2)
                    ), 8)),
            #endregion
 
            #region Tools Stand
            BuildMarketStandRules("Tools",
                new SceneRelationshipGraph(
                    new string[] { "MarketStand/Tools", "PropTable",  "PropSignBlacksmith", "PropTool", "PropTool"},
                        (1, IsPartOfFrontEdge, 0),
                        (1, ImplicitFacingSameDirection, 0),
                        (2, IsPartOf, 0),
                        (2, NextToChooseSide, 1),
                        (2, ImplicitFacingSameDirection, 1),
                        (3, OnTopOf, 1),
                        (4, OnTopOf, 1)),
                new Rule("PlaceTableTool", 0.3f,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Tools", },
                        (0, IsPartOfFrontEdge, 1)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "PropTable", "MarketStand/Tools", "PropTool" },
                        (0, IsPartOfFrontEdge, 1),
                        (2, OnTopOf, 0)
                    ), 6),
                new Rule("PlaceTableSmallTool", 0.3f,
                    new SceneRelationshipGraph(
                        new string[] { "PropTableSmall", "MarketStand/Tools", },
                        (0, IsPartOf, 1)
                    ),
                    2, 1,
                    new SceneRelationshipGraph(
                        new string[] { "PropTableSmall", "MarketStand/Tools", "PropTool" },
                        (0, IsPartOf, 1),
                        (2, OnTopOf, 0)
                    ), 5),
                new Rule("PlaceStool", 0.2f,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Tools" }
                    ),
                    1, 0,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Tools", "PropStool", "PropTool" },
                        (1, IsPartOf, 0),
                        (2, OnTopOf, 1)
                    ), 6),
                new Rule("PlaceTableSmall", 0.3f,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Tools" }
                    ),
                    1, 0,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Tools", "PropTableSmall", "PropTool" },
                        (1, IsPartOf, 0),
                        (2, OnTopOf, 1)
                    ), 6),
                new Rule("PlaceGrindstone", 0.4f,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Tools" }
                    ),
                    1, 0,
                    new SceneRelationshipGraph(
                        new string[] { "MarketStand/Tools", "PropGrindstone" },
                        (1, IsPartOf, 0)
                    ), 6))
            #endregion
        }.SelectMany(r => r).ToArray();
    }
}
