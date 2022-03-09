using UnityEngine;

public static class TransformExtensions
{
    public static void CopyFrom(this Transform transform, Transform source)
    {
        transform.SetParent(source.parent);
        transform.localPosition = source.localPosition;
        transform.localRotation = source.localRotation;
        transform.localScale = source.localScale;
    }
}

