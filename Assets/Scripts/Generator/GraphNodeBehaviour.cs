using UnityEngine;

[ExecuteAlways()]
public class GraphNodeBehaviour : MonoBehaviour
{
    public Collider Collider = null;
    public Collider AreaTerrainCollider = null;
    public string NodeType = null;
    public string[] ExpressionHierachy = new string[2];

    public bool AllowSwapMove = true;
    public bool AllowDiffusionPositionMove = true;
    public bool AllowDiffusionRotationMove = true;

    [HideInInspector]
    public int CorrespondsToNodeIndex = 0;

    public NodeInfoProvider GetNodeInfoProvider()
    {
        return new NodeInfoProvider()
        {
            NodeType = NodeType,
            ExpressionHierachy = ExpressionHierachy,
            ObjectImportance = GetObjectImportance()
        };
    }

    public float GetObjectImportance()
    {
        if (Collider == null)
            return 10;
        else if (Collider is MeshCollider meshCollider)
            return meshCollider.sharedMesh.bounds.extents.magnitude;
        else if (Collider is BoxCollider boxCollider)
            return (boxCollider.size * 0.5f).magnitude;
        else
            return Collider.bounds.extents.magnitude;
    }

    public bool MatchesNodeType(string nodeType)
    {
        return NodeType == nodeType || NodeType.Split('/')[0] == nodeType;
    }

    private void Update()
    {
        Debug.DrawRay(transform.position, (transform.rotation * Vector3.forward));
    }
}
