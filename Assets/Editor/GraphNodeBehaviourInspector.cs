using UnityEditor;

[CustomEditor(typeof(GraphNodeBehaviour))]
public class GraphNodeBehaviourInspector : Editor
{
    public override void OnInspectorGUI()
    {
        var graphNode = target as GraphNodeBehaviour;

        base.OnInspectorGUI();

        EditorGUILayout.LabelField("Object Importance", graphNode.GetObjectImportance().ToString());
    }
}
