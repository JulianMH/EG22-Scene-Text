using UnityEngine;

public class RenderToImageCamera : MonoBehaviour
{
    RenderTexture renderTexture = null;

    [SerializeField]
    UnityEngine.UI.RawImage rawImage = null;

    private void Awake()
    {
        var camera = GetComponent<Camera>();
        renderTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.Default);
        rawImage.texture = renderTexture;
        camera.targetTexture = renderTexture;
    }

    public byte[] GetScreenshot()
    {
        var renderTexture = (RenderTexture)rawImage.mainTexture;

        Texture2D newTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);

        var oldActive = RenderTexture.active;
        RenderTexture.active = renderTexture;
        newTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        newTexture.Apply();
        RenderTexture.active = oldActive;
        return newTexture.EncodeToJPG(95);
    }
}
