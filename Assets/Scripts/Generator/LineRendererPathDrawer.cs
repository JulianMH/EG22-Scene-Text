using UnityEngine;

[ExecuteAlways]
public class LineRendererPathDrawer : MonoBehaviour
{
    [SerializeField]
    private Material lineRendererMaterial = null;

    private void Update()
    {
        var canvas = GetComponentInChildren<Canvas>();
        var paths = transform.parent.GetComponentsInChildren<Path>();

        if (canvas == null)
            return;

        var canvasChildCount = canvas.transform.childCount;

        while(canvasChildCount > paths.Length)
        {
            DestroyImmediate(canvas.transform.GetChild(--canvasChildCount).gameObject);
        }

        while (canvasChildCount < paths.Length)
        {
            var gameObject = new GameObject("Temp Path Renderer", new System.Type[] { typeof(RectTransform), typeof(LineRenderer) });
            gameObject.transform.SetParent(canvas.transform, false);
            gameObject.layer = 5;
            ++canvasChildCount;
        }

        for(int i = 0; i < canvasChildCount; ++i)
        {
            var lineRendererTransform = canvas.transform.GetChild(i);
            var lineRenderer = lineRendererTransform.GetComponent<LineRenderer>();
            var rectTransform = lineRendererTransform.GetComponent<RectTransform>();
            lineRenderer.sortingOrder = 1;
            lineRenderer.useWorldSpace = false;
            lineRenderer.material = lineRendererMaterial;
            lineRenderer.numCapVertices = 3;
            rectTransform.offsetMin = new Vector2(0, 0);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.offsetMax = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            paths[i].terrainGameObject = gameObject;
            paths[i].renderTextureLineRenderer = lineRenderer;
            lineRendererTransform.name = "LineRenderer for " + paths[i].name;
        }
    }
}
