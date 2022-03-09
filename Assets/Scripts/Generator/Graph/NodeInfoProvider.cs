using System;

public class NodeInfoProvider
{
    public string[] ExpressionHierachy { get; set; }
    public string NodeType { get; set; }

    public float ObjectImportance { get; set; }

    public NodeInfoProvider()
    {
    }

    public string GetDefaultExpression()
    {
        if (ExpressionHierachy.Length == 0)
            return "IGNORED";
        else if (ExpressionHierachy.Length == 1)
            return ExpressionHierachy[0];
        else
            return ExpressionHierachy[ExpressionHierachy.Length - 2];
    }
}