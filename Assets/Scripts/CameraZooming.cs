using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZooming : MonoBehaviour {

    public float minOrthoSize = 50f;
    public float maxOrthoSize = 150f;
    public float orthoZoomSpeed = .5f;
    public float thresholdBeforeZoom = .5f;

    private Camera cameraComponent;

    public delegate void ZoomChange(float change);
    public static event ZoomChange OnZoomChange;

    private void Start()
    {
        cameraComponent = GetComponent<Camera>();
        if (cameraComponent == null)
        {
            Debug.Log("There is no camera object for cameras moves script!");
        }
    }

    private void Update()
    {
        if (Input.touchCount == 2)
        {
            CheckZoom();
        }
    }

    private void CheckZoom()
    {
        Vector2[] prevPositions = new Vector2[2];
        for (int i = 0; i < 2; i++)
        {
            prevPositions[i] = Input.GetTouch(i).position - Input.GetTouch(i).deltaPosition;
        }

        float prevDistance = (prevPositions[1] - prevPositions[0]).magnitude;
        float currentDistance = (Input.GetTouch(1).position - Input.GetTouch(0).position).magnitude;
        float deltaDistance = currentDistance - prevDistance;
        if (Mathf.Abs(deltaDistance) > thresholdBeforeZoom)
        {
            Zoom(deltaDistance);
        }
    }

    private void Zoom(float deltaDistance)
    {
        if (cameraComponent.orthographic)
        {
            float oldSize = cameraComponent.orthographicSize;

            cameraComponent.orthographicSize -= deltaDistance * orthoZoomSpeed;
            cameraComponent.orthographicSize = Mathf.Clamp(cameraComponent.orthographicSize, minOrthoSize, maxOrthoSize);

            float newSize = cameraComponent.orthographicSize;
            if (OnZoomChange != null)
            {
                OnZoomChange(newSize / oldSize);
            }
        }
    }
}
