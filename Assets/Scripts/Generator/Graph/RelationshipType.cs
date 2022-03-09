public enum RelationshipType
{
    NextTo,

    NextToChooseDirection,
    NextToChooseSide,

    NextToNorth,
    NextToEast,
    NextToSouth,
    NextToWest,
    NextToLeft,
    NextToRight,
    NextToFront,
    NextToBehind,

    OnTopOf,
    OnTopOfMiddle,
    OnTopOfEdge,
    OnTopOfFrontEdge,
    IsPartOf,
    IsPartOfMiddle,
    IsPartOfEdge,
    IsPartOfFrontEdge,

    FacingTowards,
    FacingAwayFrom,
    FacingSameDirection,
    FacingOrthogonalDirection,

    ImplicitFacingTowards,
    ImplicitFacingSameDirection,
    ImplicitFacingAwayFrom,
    ImplicitFacingOrthogonalDirection,

    PathConnectedTo
}

public static class RelationshipTypeExtensions
{
    public static string GetExpression(this RelationshipType relationshipType)
    {
        switch (relationshipType)
        {
            case RelationshipType.NextToNorth:
                return "north of";
            case RelationshipType.NextToSouth:
                return "south of";
            case RelationshipType.NextToWest:
                return "west of";
            case RelationshipType.NextToEast:
                return "east of";

            case RelationshipType.NextToChooseDirection:
            case RelationshipType.NextTo:
                return "next to";

            case RelationshipType.NextToChooseSide:
                return "besides of";

            case RelationshipType.NextToLeft:
                return "to the left of";
            case RelationshipType.NextToRight:
                return "to the right of";
            case RelationshipType.NextToFront:
                return "in front of";
            case RelationshipType.NextToBehind:
                return "behind of";

            case RelationshipType.OnTopOf:
                return "on top of";
            case RelationshipType.OnTopOfMiddle:
                return "in the middle of";
            case RelationshipType.OnTopOfEdge:
                return "on the edge of";
            case RelationshipType.OnTopOfFrontEdge:
                return "on the front edge of";
            case RelationshipType.IsPartOf:
            case RelationshipType.IsPartOfMiddle:
            case RelationshipType.IsPartOfEdge:
            case RelationshipType.IsPartOfFrontEdge:
                return "part of";

            case RelationshipType.FacingTowards:
                return "facing";
            case RelationshipType.FacingSameDirection:
                return "facing the same direction as";
            case RelationshipType.FacingOrthogonalDirection:
                return "at an right angle to";
            case RelationshipType.FacingAwayFrom:
                return "facing away from";

            case RelationshipType.PathConnectedTo:
                return "meeting at";
            default:
                return null;
        }
    }

    public static bool IsImplict(this RelationshipType relationshipType)
    {
        return relationshipType.GetExpression() == null;
    }

    public static bool IsTypePartOf(this RelationshipType relationshipType)
    {
        switch(relationshipType)
        {
            case RelationshipType.IsPartOf:
            case RelationshipType.IsPartOfFrontEdge:
            case RelationshipType.IsPartOfEdge:
            case RelationshipType.IsPartOfMiddle:
                return true;
            default:
                return false;
        }
    }

    public static bool IsTypePartOfOrOnTopOf(this RelationshipType relationshipType)
    {
        switch (relationshipType)
        {
            case RelationshipType.IsPartOf:
            case RelationshipType.IsPartOfEdge:
            case RelationshipType.IsPartOfFrontEdge:
            case RelationshipType.IsPartOfMiddle:
            case RelationshipType.OnTopOf:
            case RelationshipType.OnTopOfEdge:
            case RelationshipType.OnTopOfFrontEdge:
            case RelationshipType.OnTopOfMiddle:
                return true;
            default:
                return false;
        }
    }
}
