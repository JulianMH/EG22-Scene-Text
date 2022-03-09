using UnityEngine;

// Camera Code is based on:
// https://catlikecoding.com/unity/tutorials/movement/orbit-camera/
// Copyright 2020 Jasper Flick
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

[RequireComponent(typeof(Camera)), ExecuteAlways]
public class OrbitalCamera : MonoBehaviour
{
    [SerializeField]
    Transform focus = default;

    [SerializeField, Range(1f, 100f)]
    public float distance = 5f;

    [SerializeField, Range(1f, 360f)]
    float rotationSpeed = 30f;

    [SerializeField, Range(0.5f, 20f)]
    float zoomSpeed = 1f;

    [SerializeField, Range(-89f, 89f)]
    float minVerticalAngle = -0f, maxVerticalAngle = 80f;

    [SerializeField, Range(1f, 100f)]
    float minDistance = 1f;

    [SerializeField, Range(1f, 150f)]
    float maxDistance = 50f;

    [HideInInspector]
    public bool didUserRotate;

    [HideInInspector]
    public bool didUserZoom;

    [SerializeField]
    public bool lockCamera = false;

    public Vector2 orbitAngles = new Vector2(25f, 0f);

    [SerializeField]
    GameObject attachedCompassObject = null;

    private void Start()
    {
    }

    void LateUpdate()
    {
        if (Application.IsPlaying(gameObject) && !lockCamera) 
        {
            ManualRotation();
            ManualZoom();
            if (!didUserRotate && !didUserZoom)
                orbitAngles.y -= Time.unscaledDeltaTime * 10f;

        }
        UpdatePositions();

        UpdateCompass();
    }

    private void UpdateCompass()
    {
        if (attachedCompassObject != null)
        {
            var camera = GetComponentInChildren<Camera>();

            var relativeWidth = 0.2f;
            var width = relativeWidth * System.Math.Min(Screen.height, Screen.width);
            var margin = 0.1f * width;

            var bottomLeftCorner = camera.ScreenToWorldPoint(new Vector3(margin, margin, 2));
            var topRightCorner = camera.ScreenToWorldPoint(new Vector3(width + margin, width + margin, 2));

            attachedCompassObject.transform.position = (bottomLeftCorner + topRightCorner) * 0.5f;
            attachedCompassObject.transform.localScale = Vector3.one * 2.0f * relativeWidth;
        }
    }

    private void UpdatePositions()
    {
        var focusPoint = focus.position;
        var lookRotation = Quaternion.Euler(orbitAngles);
        var lookDirection = lookRotation * Vector3.forward;
        var lookPosition = focusPoint - lookDirection * distance;

        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    void ManualRotation()
    {
        if (Input.GetMouseButton(0))
        {
            didUserRotate = true;
            Vector2 input = new Vector2(
                Input.GetAxis("Camera Vertical"),
                Input.GetAxis("Camera Horizontal")
            );
            const float e = 0.001f;
            if (input.x < -e || input.x > e || input.y < -e || input.y > e)
            {
                orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
            }

            orbitAngles.x =
                Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

            if (orbitAngles.y < 0f)
            {
                orbitAngles.y += 360f;
            }
            else if (orbitAngles.y >= 360f)
            {
                orbitAngles.y -= 360f;
            }
        }
    }

    void ManualZoom()
    {
        var input = Input.GetAxis("Camera Zoom");
        if(Mathf.Abs(input) > 0.001f)
            didUserZoom = true;
        distance += input * zoomSpeed;

        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    void OnValidate()
    {
        if (maxVerticalAngle < minVerticalAngle)
        {
            maxVerticalAngle = minVerticalAngle;
        }
        if (maxDistance < minDistance)
        {
            maxDistance = minDistance;
        }
    }
}
