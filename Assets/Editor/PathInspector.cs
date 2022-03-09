using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Path))]
public class PathInspector : Editor
{
    bool lockYAxis = true;
    bool drawHandles = true;
    
    private void OnSceneGUI()
    {
        var path = target as Path;
        var transform = path.transform;

        if(drawHandles)
        {
            Undo.RecordObject(path, "Path change");
            for (int i = 0; i < path.LocalPositions.Length; ++i)
            {
                var old = path.LocalPositions[i];
                path.LocalPositions[i] = transform.InverseTransformPoint(
                    Handles.PositionHandle(
                        transform.TransformPoint(path.LocalPositions[i]), Quaternion.identity));

                if ((i == 0 && path.FixStartX) || (i == path.LocalPositions.Length - 1 && path.FixEndX))
                    path.LocalPositions[i].x = old.x;
                if (lockYAxis)
                    path.LocalPositions[i].y = old.y;
                if ((i == 0 && path.FixStartZ) || (i == path.LocalPositions.Length - 1 && path.FixEndZ))
                    path.LocalPositions[i].z = old.z;
            }
            path.Update();
        }
    }

    public override void OnInspectorGUI()
    {
        var path = target as Path;

        drawHandles = EditorGUILayout.Toggle("Draw Handles", drawHandles);
        lockYAxis = EditorGUILayout.Toggle("Lock Y Axis", drawHandles);

        base.OnInspectorGUI();

        GUILayout.Label("Current Angles: " + string.Join(", ", path.GetAngles().Select(a => $"{a * Mathf.Rad2Deg:0.00}°")));
        GUILayout.Label($"Current Length: {path.GetLength():0.00}");

        if (path.Parent != null)
        {
            var firstLineSement = path.LineSegments.First();
            GUILayout.Label($"Current Length First Segment: {Vector3.Distance(firstLineSement.from, firstLineSement.to):0.00}");
        }
    }
}
